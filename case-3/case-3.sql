-- Case 3: Database, integrity, and advanced reasoning
-- Token: VEH-AGIT-001

-- ------------------------------------------------------------------------------
-- Task 1: Schema and constraints (DDL)
-- Bikin tabel header Planning dan detail PlanningSlot.
-- Constraint: PK, FK cascade, unique RequestCode, NOT NULL, dan check quantity >= 0.
-- ------------------------------------------------------------------------------

DROP TABLE IF EXISTS PlanningSlots;
DROP TABLE IF EXISTS Plannings;

CREATE TABLE Plannings (
    PlanningId TEXT PRIMARY KEY NOT NULL,
    RequestCode VARCHAR(100) NOT NULL,
    CandidateToken VARCHAR(100) NOT NULL,
    CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Status VARCHAR(20) NOT NULL DEFAULT 'Success',
    OriginalTotal INTEGER NOT NULL DEFAULT 0 CHECK (OriginalTotal >= 0),
    BalancedTotal INTEGER NOT NULL DEFAULT 0 CHECK (BalancedTotal >= 0),
    CONSTRAINT UQ_Plannings_RequestCode UNIQUE (RequestCode)
);

CREATE TABLE PlanningSlots (
    PlanningSlotId TEXT PRIMARY KEY NOT NULL,
    PlanningId TEXT NOT NULL,
    SlotOrder INTEGER NOT NULL CHECK (SlotOrder >= 1 AND SlotOrder <= 7),
    SlotName VARCHAR(50) NOT NULL,
    OriginalQuantity INTEGER NOT NULL CHECK (OriginalQuantity >= 0),
    BalancedQuantity INTEGER NOT NULL CHECK (BalancedQuantity >= 0),
    IsActive INTEGER NOT NULL CHECK (IsActive IN (0, 1)),
    CONSTRAINT FK_PlanningSlots_Planning FOREIGN KEY (PlanningId) 
        REFERENCES Plannings(PlanningId) ON DELETE CASCADE,
    CONSTRAINT UQ_PlanningSlots_Order UNIQUE (PlanningId, SlotOrder)
);


-- ------------------------------------------------------------------------------
-- Task 2: Seed data (minimal 10 planning)
-- Token: VEH-AGIT-001
-- Nyediain variasi data: normal, semua 0, 1 slot aktif, tie, sisa bagi, nilai besar, dll.
-- ------------------------------------------------------------------------------

-- 1. Normal case (sample soal: [4, 5, 1, 7, 6, 4, 0] -> [4, 5, 4, 5, 5, 4, 0], total 27)
INSERT INTO Plannings (PlanningId, RequestCode, CandidateToken, CreatedAt, Status, OriginalTotal, BalancedTotal)
VALUES ('P-01', 'REQ-VEH-001', 'VEH-AGIT-001', '2026-08-31 08:00:00', 'Success', 27, 27);

INSERT INTO PlanningSlots (PlanningSlotId, PlanningId, SlotOrder, SlotName, OriginalQuantity, BalancedQuantity, IsActive) VALUES
('PS-01-1', 'P-01', 1, 'Senin', 4, 4, 1),
('PS-01-2', 'P-01', 2, 'Selasa', 5, 5, 1),
('PS-01-3', 'P-01', 3, 'Rabu', 1, 4, 1),
('PS-01-4', 'P-01', 4, 'Kamis', 7, 5, 1),
('PS-01-5', 'P-01', 5, 'Jumat', 6, 5, 1),
('PS-01-6', 'P-01', 6, 'Sabtu', 4, 4, 1),
('PS-01-7', 'P-01', 7, 'Minggu', 0, 0, 0);

-- 2. Semua 0 (hari libur semua, total 0)
INSERT INTO Plannings (PlanningId, RequestCode, CandidateToken, CreatedAt, Status, OriginalTotal, BalancedTotal)
VALUES ('P-02', 'REQ-VEH-002', 'VEH-AGIT-001', '2026-08-31 08:05:00', 'Success', 0, 0);

INSERT INTO PlanningSlots (PlanningSlotId, PlanningId, SlotOrder, SlotName, OriginalQuantity, BalancedQuantity, IsActive) VALUES
('PS-02-1', 'P-02', 1, 'Senin', 0, 0, 0),
('PS-02-2', 'P-02', 2, 'Selasa', 0, 0, 0),
('PS-02-3', 'P-02', 3, 'Rabu', 0, 0, 0),
('PS-02-4', 'P-02', 4, 'Kamis', 0, 0, 0),
('PS-02-5', 'P-02', 5, 'Jumat', 0, 0, 0),
('PS-02-6', 'P-02', 6, 'Sabtu', 0, 0, 0),
('PS-02-7', 'P-02', 7, 'Minggu', 0, 0, 0);

-- 3. Cuma 1 slot aktif ([0, 0, 15, 0, 0, 0, 0] -> tetap 15 di hari Rabu)
INSERT INTO Plannings (PlanningId, RequestCode, CandidateToken, CreatedAt, Status, OriginalTotal, BalancedTotal)
VALUES ('P-03', 'REQ-VEH-003', 'VEH-AGIT-001', '2026-08-31 08:10:00', 'Success', 15, 15);

INSERT INTO PlanningSlots (PlanningSlotId, PlanningId, SlotOrder, SlotName, OriginalQuantity, BalancedQuantity, IsActive) VALUES
('PS-03-1', 'P-03', 1, 'Senin', 0, 0, 0),
('PS-03-2', 'P-03', 2, 'Selasa', 0, 0, 0),
('PS-03-3', 'P-03', 3, 'Rabu', 15, 15, 1),
('PS-03-4', 'P-03', 4, 'Kamis', 0, 0, 0),
('PS-03-5', 'P-03', 5, 'Jumat', 0, 0, 0),
('PS-03-6', 'P-03', 6, 'Sabtu', 0, 0, 0),
('PS-03-7', 'P-03', 7, 'Minggu', 0, 0, 0);

-- 4. Kondisi tie-breaker (nilai awal sama, prioritas hari yang lebih awal)
INSERT INTO Plannings (PlanningId, RequestCode, CandidateToken, CreatedAt, Status, OriginalTotal, BalancedTotal)
VALUES ('P-04', 'REQ-VEH-004', 'VEH-AGIT-001', '2026-08-31 08:15:00', 'Success', 7, 7);

INSERT INTO PlanningSlots (PlanningSlotId, PlanningId, SlotOrder, SlotName, OriginalQuantity, BalancedQuantity, IsActive) VALUES
('PS-04-1', 'P-04', 1, 'Senin', 2, 2, 1),
('PS-04-2', 'P-04', 2, 'Selasa', 2, 2, 1),
('PS-04-3', 'P-04', 3, 'Rabu', 3, 3, 1),
('PS-04-4', 'P-04', 4, 'Kamis', 0, 0, 0),
('PS-04-5', 'P-04', 5, 'Jumat', 0, 0, 0),
('PS-04-6', 'P-04', 6, 'Sabtu', 0, 0, 0),
('PS-04-7', 'P-04', 7, 'Minggu', 0, 0, 0);

-- 5. Total habis dibagi (12 dibagi 4 slot aktif = masing-masing 3)
INSERT INTO Plannings (PlanningId, RequestCode, CandidateToken, CreatedAt, Status, OriginalTotal, BalancedTotal)
VALUES ('P-05', 'REQ-VEH-005', 'VEH-AGIT-001', '2026-08-31 08:20:00', 'Success', 12, 12);

INSERT INTO PlanningSlots (PlanningSlotId, PlanningId, SlotOrder, SlotName, OriginalQuantity, BalancedQuantity, IsActive) VALUES
('PS-05-1', 'P-05', 1, 'Senin', 3, 3, 1),
('PS-05-2', 'P-05', 2, 'Selasa', 3, 3, 1),
('PS-05-3', 'P-05', 3, 'Rabu', 3, 3, 1),
('PS-05-4', 'P-05', 4, 'Kamis', 0, 0, 0),
('PS-05-5', 'P-05', 5, 'Jumat', 3, 3, 1),
('PS-05-6', 'P-05', 6, 'Sabtu', 0, 0, 0),
('PS-05-7', 'P-05', 7, 'Minggu', 0, 0, 0);

-- 6. Total bersisa (total 23 dibagi 5 slot aktif, sisa 3 disebar ke slot terbesar)
INSERT INTO Plannings (PlanningId, RequestCode, CandidateToken, CreatedAt, Status, OriginalTotal, BalancedTotal)
VALUES ('P-06', 'REQ-VEH-006', 'VEH-AGIT-001', '2026-08-31 08:25:00', 'Success', 23, 23);

INSERT INTO PlanningSlots (PlanningSlotId, PlanningId, SlotOrder, SlotName, OriginalQuantity, BalancedQuantity, IsActive) VALUES
('PS-06-1', 'P-06', 1, 'Senin', 6, 5, 1),
('PS-06-2', 'P-06', 2, 'Selasa', 2, 4, 1),
('PS-06-3', 'P-06', 3, 'Rabu', 7, 5, 1),
('PS-06-4', 'P-06', 4, 'Kamis', 5, 5, 1),
('PS-06-5', 'P-06', 5, 'Jumat', 3, 4, 1),
('PS-06-6', 'P-06', 6, 'Sabtu', 0, 0, 0),
('PS-06-7', 'P-06', 7, 'Minggu', 0, 0, 0);

-- 7. Nilai besar (jutaan, tes biar aman dari overflow)
INSERT INTO Plannings (PlanningId, RequestCode, CandidateToken, CreatedAt, Status, OriginalTotal, BalancedTotal)
VALUES ('P-07', 'REQ-VEH-007', 'VEH-AGIT-001', '2026-08-31 08:30:00', 'Success', 3999998, 3999998);

INSERT INTO PlanningSlots (PlanningSlotId, PlanningId, SlotOrder, SlotName, OriginalQuantity, BalancedQuantity, IsActive) VALUES
('PS-07-1', 'P-07', 1, 'Senin', 1000000, 1000000, 1),
('PS-07-2', 'P-07', 2, 'Selasa', 999999, 999999, 1),
('PS-07-3', 'P-07', 3, 'Rabu', 0, 0, 0),
('PS-07-4', 'P-07', 4, 'Kamis', 1000001, 1000000, 1),
('PS-07-5', 'P-07', 5, 'Jumat', 0, 0, 0),
('PS-07-6', 'P-07', 6, 'Sabtu', 0, 0, 0),
('PS-07-7', 'P-07', 7, 'Minggu', 999998, 999999, 1);

-- 8. Semua hari aktif (Senin sampai Minggu masuk semua)
INSERT INTO Plannings (PlanningId, RequestCode, CandidateToken, CreatedAt, Status, OriginalTotal, BalancedTotal)
VALUES ('P-08', 'REQ-VEH-008', 'VEH-AGIT-001', '2026-08-31 08:35:00', 'Success', 71, 71);

INSERT INTO PlanningSlots (PlanningSlotId, PlanningId, SlotOrder, SlotName, OriginalQuantity, BalancedQuantity, IsActive) VALUES
('PS-08-1', 'P-08', 1, 'Senin', 10, 10, 1),
('PS-08-2', 'P-08', 2, 'Selasa', 10, 10, 1),
('PS-08-3', 'P-08', 3, 'Rabu', 10, 10, 1),
('PS-08-4', 'P-08', 4, 'Kamis', 10, 10, 1),
('PS-08-5', 'P-08', 5, 'Jumat', 10, 10, 1),
('PS-08-6', 'P-08', 6, 'Sabtu', 10, 10, 1),
('PS-08-7', 'P-08', 7, 'Minggu', 11, 11, 1);

-- 9. Slot cuma 1 tapi nilainya lumayan gede
INSERT INTO Plannings (PlanningId, RequestCode, CandidateToken, CreatedAt, Status, OriginalTotal, BalancedTotal)
VALUES ('P-09', 'REQ-VEH-009', 'VEH-AGIT-001', '2026-08-31 08:40:00', 'Success', 60, 60);

INSERT INTO PlanningSlots (PlanningSlotId, PlanningId, SlotOrder, SlotName, OriginalQuantity, BalancedQuantity, IsActive) VALUES
('PS-09-1', 'P-09', 1, 'Senin', 60, 60, 1),
('PS-09-2', 'P-09', 2, 'Selasa', 0, 0, 0),
('PS-09-3', 'P-09', 3, 'Rabu', 0, 0, 0),
('PS-09-4', 'P-09', 4, 'Kamis', 0, 0, 0),
('PS-09-5', 'P-09', 5, 'Jumat', 0, 0, 0),
('PS-09-6', 'P-09', 6, 'Sabtu', 0, 0, 0),
('PS-09-7', 'P-09', 7, 'Minggu', 0, 0, 0);

-- 10. Dua slot aktif dengan selisih awal yang jauh (100 vs 1)
INSERT INTO Plannings (PlanningId, RequestCode, CandidateToken, CreatedAt, Status, OriginalTotal, BalancedTotal)
VALUES ('P-10', 'REQ-VEH-010', 'VEH-AGIT-001', '2026-08-31 08:45:00', 'Success', 101, 101);

INSERT INTO PlanningSlots (PlanningSlotId, PlanningId, SlotOrder, SlotName, OriginalQuantity, BalancedQuantity, IsActive) VALUES
('PS-10-1', 'P-10', 1, 'Senin', 100, 51, 1),
('PS-10-2', 'P-10', 2, 'Selasa', 1, 50, 1),
('PS-10-3', 'P-10', 3, 'Rabu', 0, 0, 0),
('PS-10-4', 'P-10', 4, 'Kamis', 0, 0, 0),
('PS-10-5', 'P-10', 5, 'Jumat', 0, 0, 0),
('PS-10-6', 'P-10', 6, 'Sabtu', 0, 0, 0),
('PS-10-7', 'P-10', 7, 'Minggu', 0, 0, 0);


-- ------------------------------------------------------------------------------
-- Task 3: Total validation query
-- Cek apakah total awal == total hasil, dan cocok sama sum slot detailnya.
-- ------------------------------------------------------------------------------
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


-- ------------------------------------------------------------------------------
-- Task 4: History query
-- Ambil riwayat planning, hitung berapa slot yang aktif, urutkan dari yang paling baru.
-- ------------------------------------------------------------------------------
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


-- ------------------------------------------------------------------------------
-- Task 5: Anomaly query
-- Cek keanehan data:
-- 1. Slot libur/mati tapi BalancedQuantity > 0
-- 2. Total awal beda sama total hasil
-- 3. Slot detailnya kurang dari 7 hari
-- 4. Ada RequestCode yang kembar
-- ------------------------------------------------------------------------------
SELECT 
    p.PlanningId,
    p.RequestCode,
    'Slot nonaktif nilainya > 0' AS AnomalyType
FROM Plannings p
JOIN PlanningSlots ps ON p.PlanningId = ps.PlanningId
WHERE ps.IsActive = 0 AND ps.BalancedQuantity > 0

UNION ALL

SELECT 
    p.PlanningId,
    p.RequestCode,
    'Total awal tidak sama dengan total hasil' AS AnomalyType
FROM Plannings p
WHERE p.OriginalTotal <> p.BalancedTotal

UNION ALL

SELECT 
    p.PlanningId,
    p.RequestCode,
    'Slot tidak lengkap (kurang dari 7)' AS AnomalyType
FROM Plannings p
JOIN PlanningSlots ps ON p.PlanningId = ps.PlanningId
GROUP BY p.PlanningId, p.RequestCode
HAVING COUNT(ps.PlanningSlotId) < 7

UNION ALL

SELECT 
    p.PlanningId,
    p.RequestCode,
    'RequestCode duplikat' AS AnomalyType
FROM Plannings p
WHERE p.RequestCode IN (
    SELECT RequestCode 
    FROM Plannings 
    GROUP BY RequestCode 
    HAVING COUNT(*) > 1
);


-- ------------------------------------------------------------------------------
-- Task 6: Largest adjustments
-- Cari 3 slot yang perubahannya paling drastis (selisih absolut terbesar).
-- Kalau selisihnya sama, pilih yang harinya lebih awal (SlotOrder lebih kecil).
-- ------------------------------------------------------------------------------
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


-- ------------------------------------------------------------------------------
-- Task 7: Atomic save
-- Pakai database transaction biar header dan semua 7 slot detail tersimpan barengan.
-- Kalau salah satu insert gagal, rollback semua biar ga nyisa data setengah-setengah.
-- ------------------------------------------------------------------------------

BEGIN TRANSACTION;

-- 1. Insert header planning
INSERT INTO Plannings (PlanningId, RequestCode, CandidateToken, CreatedAt, Status, OriginalTotal, BalancedTotal)
VALUES (@PlanningId, @RequestCode, @CandidateToken, CURRENT_TIMESTAMP, 'Success', @OriginalTotal, @BalancedTotal);

-- 2. Insert 7 slot detailnya
INSERT INTO PlanningSlots (PlanningSlotId, PlanningId, SlotOrder, SlotName, OriginalQuantity, BalancedQuantity, IsActive) VALUES
(@Id1, @PlanningId, 1, 'Senin', @Orig1, @Bal1, @IsActive1),
(@Id2, @PlanningId, 2, 'Selasa', @Orig2, @Bal2, @IsActive2),
(@Id3, @PlanningId, 3, 'Rabu', @Orig3, @Bal3, @IsActive3),
(@Id4, @PlanningId, 4, 'Kamis', @Orig4, @Bal4, @IsActive4),
(@Id5, @PlanningId, 5, 'Jumat', @Orig5, @Bal5, @IsActive5),
(@Id6, @PlanningId, 6, 'Sabtu', @Orig6, @Bal6, @IsActive6),
(@Id7, @PlanningId, 7, 'Minggu', @Orig7, @Bal7, @IsActive7);

-- Kalau lancar semua:
COMMIT;

-- Kalau ada error/gagal:
-- ROLLBACK;



-- ------------------------------------------------------------------------------
-- Task 8: Latest processing version
-- Tabel buat nyatet riwayat run/versi balancing (RebalanceRun).
-- Query-nya cuma nampilin run paling baru untuk tiap Planning.
-- ------------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS RebalanceRuns (
    RunId TEXT PRIMARY KEY NOT NULL,
    PlanningId TEXT NOT NULL,
    RunVersion INTEGER NOT NULL,
    RunAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    AlgorithmVersion VARCHAR(50) NOT NULL,
    CONSTRAINT FK_RebalanceRuns_Planning FOREIGN KEY (PlanningId) REFERENCES Plannings(PlanningId) ON DELETE CASCADE
);

-- Query ambil run versi terbaru per Planning pakai ROW_NUMBER():

WITH RankedRuns AS (
    SELECT 
        r.RunId,
        r.PlanningId,
        r.RunVersion,
        r.RunAt,
        r.AlgorithmVersion,
        p.RequestCode,
        p.OriginalTotal,
        p.BalancedTotal,
        ROW_NUMBER() OVER (PARTITION BY r.PlanningId ORDER BY r.RunVersion DESC, r.RunAt DESC) AS rn
    FROM RebalanceRuns r
    JOIN Plannings p ON r.PlanningId = p.PlanningId
)
SELECT 
    PlanningId,
    RequestCode,
    RunId,
    RunVersion AS LatestVersion,
    RunAt AS LatestRunAt,
    AlgorithmVersion,
    OriginalTotal,
    BalancedTotal
FROM RankedRuns
WHERE rn = 1;



-- ------------------------------------------------------------------------------
-- Task 9: Index proposal
-- 1. Index di Plannings (RequestCode, CreatedAt DESC, Status)
--    - Manfaat: Bikin cek duplikasi RequestCode jadi instan, dan query history ga perlu sort manual di memori.
--    - Biaya write: Ada sedikit beban saat INSERT/UPDATE karena database perlu update struktur B-Tree index.
-- 2. Index di PlanningSlots (PlanningId, SlotOrder)
--    - Manfaat: JOIN antara Planning dan detailnya jadi cepat tanpa scan seluruh tabel.
--    - Biaya write: Tambahan waktu kecil saat insert 7 baris slot.
-- ------------------------------------------------------------------------------
CREATE INDEX IF NOT EXISTS IX_Plannings_Search ON Plannings (RequestCode, CreatedAt DESC, Status);
CREATE INDEX IF NOT EXISTS IX_PlanningSlots_PlanningId_SlotOrder ON PlanningSlots (PlanningId, SlotOrder);


-- ------------------------------------------------------------------------------
-- Task 10: Safe migration
-- Cara aman migrasi dari tabel lama yang slotnya masih berupa kolom (Senin_Qty, Selasa_Qty, ...)
-- ke tabel baru yang per-baris (PlanningSlots).
-- ------------------------------------------------------------------------------

-- 1. Cek dulu data lama sebelum migrasi
SELECT COUNT(*) AS TotalBarisLama, SUM(TotalQty) AS TotalQtyLama FROM OldPlanningTable;

-- 2. Buat tabel baru
DROP TABLE IF EXISTS PlanningSlots;
DROP TABLE IF EXISTS Plannings;

CREATE TABLE Plannings (
    PlanningId TEXT PRIMARY KEY NOT NULL,
    RequestCode VARCHAR(100) NOT NULL,
    CandidateToken VARCHAR(100) NOT NULL,
    CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Status VARCHAR(20) NOT NULL DEFAULT 'Success',
    OriginalTotal INTEGER NOT NULL DEFAULT 0 CHECK (OriginalTotal >= 0),
    BalancedTotal INTEGER NOT NULL DEFAULT 0 CHECK (BalancedTotal >= 0),
    CONSTRAINT UQ_Plannings_RequestCode UNIQUE (RequestCode)
);

CREATE TABLE PlanningSlots (
    PlanningSlotId TEXT PRIMARY KEY NOT NULL,
    PlanningId TEXT NOT NULL,
    SlotOrder INTEGER NOT NULL CHECK (SlotOrder >= 1 AND SlotOrder <= 7),
    SlotName VARCHAR(50) NOT NULL,
    OriginalQuantity INTEGER NOT NULL CHECK (OriginalQuantity >= 0),
    BalancedQuantity INTEGER NOT NULL CHECK (BalancedQuantity >= 0),
    IsActive INTEGER NOT NULL CHECK (IsActive IN (0, 1)),
    CONSTRAINT FK_PlanningSlots_Planning FOREIGN KEY (PlanningId) 
        REFERENCES Plannings(PlanningId) ON DELETE CASCADE,
    CONSTRAINT UQ_PlanningSlots_Order UNIQUE (PlanningId, SlotOrder)
);

-- 3. Pindahkan datanya dengan unpivot 7 kolom jadi 7 baris
INSERT INTO PlanningSlots (PlanningSlotId, PlanningId, SlotOrder, SlotName, OriginalQuantity, BalancedQuantity, IsActive)
SELECT lower(hex(randomblob(16))), Id, 1, 'Senin', Senin_Orig, Senin_Bal, CASE WHEN Senin_Orig > 0 THEN 1 ELSE 0 END FROM OldPlanningTable
UNION ALL
SELECT lower(hex(randomblob(16))), Id, 2, 'Selasa', Selasa_Orig, Selasa_Bal, CASE WHEN Selasa_Orig > 0 THEN 1 ELSE 0 END FROM OldPlanningTable
UNION ALL
SELECT lower(hex(randomblob(16))), Id, 3, 'Rabu', Rabu_Orig, Rabu_Bal, CASE WHEN Rabu_Orig > 0 THEN 1 ELSE 0 END FROM OldPlanningTable
UNION ALL
SELECT lower(hex(randomblob(16))), Id, 4, 'Kamis', Kamis_Orig, Kamis_Bal, CASE WHEN Kamis_Orig > 0 THEN 1 ELSE 0 END FROM OldPlanningTable
UNION ALL
SELECT lower(hex(randomblob(16))), Id, 5, 'Jumat', Jumat_Orig, Jumat_Bal, CASE WHEN Jumat_Orig > 0 THEN 1 ELSE 0 END FROM OldPlanningTable
UNION ALL
SELECT lower(hex(randomblob(16))), Id, 6, 'Sabtu', Sabtu_Orig, Sabtu_Bal, CASE WHEN Sabtu_Orig > 0 THEN 1 ELSE 0 END FROM OldPlanningTable
UNION ALL
SELECT lower(hex(randomblob(16))), Id, 7, 'Minggu', Minggu_Orig, Minggu_Bal, CASE WHEN Minggu_Orig > 0 THEN 1 ELSE 0 END FROM OldPlanningTable;

-- 4. Validasi setelah migrasi (pastikan baris baru = 7x baris lama & total kuantitasnya sama persis)
SELECT 
    (SELECT COUNT(*) * 7 FROM OldPlanningTable) AS ExpectedSlots,
    (SELECT COUNT(*) FROM PlanningSlots) AS ActualSlots,
    (SELECT SUM(TotalQty) FROM OldPlanningTable) AS OldTotal,
    (SELECT SUM(OriginalQuantity) FROM PlanningSlots) AS NewTotal;
