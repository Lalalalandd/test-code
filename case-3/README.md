# Case 3 — Database, Integrity, and Advanced Reasoning

Dokumen ini berisi penjelasan dan jawaban rinci untuk 10 poin tugas pada **Case 3**. Seluruh implementasi SQL executable dapat ditemukan di file [`case-3.sql`](file:///Users/jefriachmadm/Documents/Ngoding/test-code-agit/case-3/case-3.sql).

---

## 1. Schema and Constraints (DDL)
Skema relasional dirancang menggunakan pemodelan **Header-Detail** (`Plannings` dan `PlanningSlots`):
- **`Plannings` (Header)**:
  - `PlanningId` (PK, Non-Null).
  - `RequestCode` (Unique Index untuk menjaga idempotency request).
  - `CandidateToken` (Audit trail kandidat/user).
  - `Status` & `CreatedAt` (Timestamp otomatis).
  - `OriginalTotal` & `BalancedTotal` dengan `CHECK (Quantity >= 0)`.
- **`PlanningSlots` (Detail per Hari)**:
  - `PlanningSlotId` (PK).
  - `PlanningId` (FK dengan constraint `ON DELETE CASCADE`).
  - `SlotOrder` (1 s.d. 7, UNIQUE per `PlanningId`).
  - `OriginalQuantity` & `BalancedQuantity` dengan `CHECK (Quantity >= 0)`.
  - `IsActive` (0 atau 1).

---

## 2. Seed Data (10 Kasus)
Menggunakan token `VEH-AGIT-001` dengan variasi data lengkap:
1. **Normal Case**: Contoh dari lembar assessment (`[4, 5, 1, 7, 6, 4, 0]`).
2. **Semua 0**: Seluruh slot tidak aktif (`[0, 0, 0, 0, 0, 0, 0]`).
3. **Satu Slot Aktif**: Hanya 1 hari beroperasi (`[0, 0, 15, 0, 0, 0, 0]`).
4. **Tie-Breaker Case**: Slot bernilai sama berebut sisa bagi (+1) berdasarkan urutan slot awal.
5. **Total Habis Dibagi**: Sisa bagi = 0 (contoh: total 12 dibagi 4 slot aktif = masing-masing 3).
6. **Total Bersisa Besar**: Sisa bagi lebih dari 1.
7. **Large Values**: Angka jutaan untuk menguji integritas tipe data dan kalkulasi agregasi.
8. **7 Hari Aktif Penuh**: Tanpa hari libur.
9. **Extreme Skew**: Nilai menumpuk di 1 hari saja.
10. **2 Hari dengan Gap Lebar**: 100 vs 1 diseimbangkan menjadi 51 vs 50.

---

## 3. Total Validation Query
Query untuk memvalidasi invarian bahwa total awal sama dengan total hasil, serta memverifikasi kesesuaian antara angka agregat di header dengan jumlah riil di tabel detail (`PlanningSlots`):
```sql
SELECT 
    p.PlanningId,
    p.RequestCode,
    p.OriginalTotal,
    p.BalancedTotal,
    SUM(ps.OriginalQuantity) AS CalculatedOriginalSum,
    SUM(ps.BalancedQuantity) AS CalculatedBalancedSum,
    CASE 
        WHEN p.OriginalTotal = p.BalancedTotal 
         AND p.OriginalTotal = SUM(ps.OriginalQuantity)
         AND p.BalancedTotal = SUM(ps.BalancedQuantity)
        THEN 1 
        ELSE 0 
    END AS IsTotalValid
FROM Plannings p
JOIN PlanningSlots ps ON p.PlanningId = ps.PlanningId
GROUP BY p.PlanningId, p.RequestCode, p.OriginalTotal, p.BalancedTotal;
```

---

## 4. History Query
Query riwayat diurutkan dari yang terbaru (`CreatedAt DESC`) dengan menghitung jumlah slot aktif:
```sql
SELECT 
    p.RequestCode,
    p.CreatedAt,
    COUNT(CASE WHEN ps.IsActive = 1 THEN 1 END) AS ActiveSlotsCount,
    p.OriginalTotal,
    p.BalancedTotal,
    p.Status
FROM Plannings p
JOIN PlanningSlots ps ON p.PlanningId = ps.PlanningId
GROUP BY p.PlanningId, p.RequestCode, p.CreatedAt, p.OriginalTotal, p.BalancedTotal, p.Status
ORDER BY p.CreatedAt DESC;
```

---

## 5. Anomaly Detection Query
Mendeteksi 4 jenis anomali data:
1. Slot nonaktif (`IsActive = 0`) namun mendapat `BalancedQuantity > 0`.
2. Total awal tidak sama dengan total hasil (`OriginalTotal <> BalancedTotal`).
3. Jumlah slot detail tidak lengkap (kurang dari 7 hari).
4. `RequestCode` terduplikasi.

Query menggunakan `UNION ALL` untuk menggabungkan seluruh kriteria anomali.

---

## 6. Largest Adjustments Query
Menampilkan 3 slot dengan perubahan kuota terbesar secara absolut (`|BalancedQuantity - OriginalQuantity|`). Jika bernilai sama, slot dengan urutan hari lebih awal (`SlotOrder ASC`) diprioritaskan:
```sql
SELECT 
    ps.PlanningId,
    p.RequestCode,
    ps.SlotOrder,
    ps.SlotName,
    ps.OriginalQuantity,
    ps.BalancedQuantity,
    ABS(ps.BalancedQuantity - ps.OriginalQuantity) AS AbsoluteAdjustment
FROM PlanningSlots ps
JOIN Plannings p ON ps.PlanningId = p.PlanningId
ORDER BY AbsoluteAdjustment DESC, ps.SlotOrder ASC
LIMIT 3;
```

---

## 7. Atomic Save (Transaction Script)
Menjamin integritas ACID. Jika salah satu baris detail gagal di-*insert*, seluruh transaksi dibatalkan (*Rollback*) sehingga tidak ada data menggantung/parsial:
```sql
BEGIN TRANSACTION;

-- 1. Simpan Header
INSERT INTO Plannings (...) VALUES (...);

-- 2. Simpan 7 Detail Slots
INSERT INTO PlanningSlots (...) VALUES (...), (...), ...;

-- Jika seluruh statement berhasil:
COMMIT;

-- Jika salah satu constraint / foreign key gagal:
-- ROLLBACK;
```

---

## 8. Latest Processing Version (RebalanceRun / Versioning)
Untuk mendukung audit dan eksekusi ulang algoritma rebalancing terhadap planning yang sama, dibuat tabel `RebalanceRuns`:
```sql
CREATE TABLE RebalanceRuns (
    RunId TEXT PRIMARY KEY,
    PlanningId TEXT NOT NULL,
    RunVersion INTEGER NOT NULL,
    RunAt TIMESTAMP NOT NULL,
    AlgorithmVersion VARCHAR(50),
    FOREIGN KEY (PlanningId) REFERENCES Plannings(PlanningId) ON DELETE CASCADE
);
```
Query menampilkan run terbaru menggunakan Window Function (`ROW_NUMBER() OVER (PARTITION BY PlanningId ORDER BY RunVersion DESC)`).

---

## 9. Index Proposal & Trade-off Analysis
```sql
-- 1. Index untuk Idempotency dan Sorting History
CREATE INDEX IX_Plannings_Search ON Plannings (RequestCode, CreatedAt DESC, Status);

-- 2. Index untuk Foreign Key Join & Urutan Slot
CREATE INDEX IX_PlanningSlots_PlanningId_SlotOrder ON PlanningSlots (PlanningId, SlotOrder);
```
### Analisis Manfaat vs Biaya Write:
* **Manfaat**:
  - Pengecekan idempotency `RequestCode` menjadi instan $O(\log N)$ atau $O(1)$ Hash Lookup.
  - Query riwayat `ORDER BY CreatedAt DESC` tidak memerlukan operasi *in-memory sort* (*Index Scan* langsung).
  - Eliminasi *Full Table Scan* saat melakukan `JOIN` antara `Plannings` dan `PlanningSlots`.
* **Biaya Write (*Write Cost / Overhead*)**:
  - Sedikit penambahan waktu pada operasi `INSERT` dan `DELETE` karena engine database harus memperbarui struktur pohon B-Tree indeks.
  - Tambahan ruang penyimpanan di disk/memori untuk menyimpan struktur indeks. Karena tabel ini didominasi *Read Heavy* (riwayat dan lookup), trade-off ini sangat optimal.

---

## 10. Safe Migration Strategy (Kolom Flat $\rightarrow$ Relasional Row-per-Slot)
Strategi migrasi zero-data-loss dari skema lama (`Senin_Qty, Selasa_Qty, ...`) ke model relasional normal:

1. **Pre-Migration Validation**: Hitung total record lama dan sum agregat seluruh kolom produksi.
2. **DDL Creation**: Siapkan tabel `Plannings` dan `PlanningSlots` baru.
3. **Data Unpivot & Migration**: Eksekusi batch `INSERT INTO ... SELECT ... UNION ALL` untuk mentransformasi 7 kolom menjadi 7 baris.
4. **Post-Migration Validation**:
   - Pastikan `Jumlah Baris Baru = 7 × Jumlah Baris Lama`.
   - Pastikan `Sum(OriginalQuantity) Baru = Sum(Total Seluruh Kolom Lama)`.
5. **Cut-over & Cleanup**: Ubah referensi aplikasi ke skema baru, lalu *deprecate/drop* tabel lama setelah masa transisi.
