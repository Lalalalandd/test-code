function BalanceCarProduction(planningData) {
    Validate all input values
    
    Find all active slot. Plan must be > 0

    if there is no active slot:
        return all zero

    totalPlan = sum of all plan

    baseQty = totalPlan / number of active slots
    remainder = totalPlan % number of active slots

    Set balanced quantity of every active slot
        to baseQty

    Sort active slots by:
        original quantity descending
        slot order ascending when original quantity is equal

    Give +1 to the first "remainder" active slots

    Keep inactive slots as 0

    Return balanced quantities
}

### Asumsi

- Input harus berupa arraay integer non-negatif. Tipe data selain ini akan throw error validation dikarenakan tidak sesuai dengan tipe data yang diminta yaitu angka bulat/integer(tidak negatif)
- Nilai 0 menandakan hari yang tidak aktif dan nilai nya harus tetap 0
- Slot yang aktif akan mendapatkan pembagian produksi sesuai dengan sisa produksi yang tersisa
- Jika semua slot bernilai 0, maka output nya juga semua 0
- Pseudocode ini dibuat untuk mengetahui panjang slot N secara dinamis
- Urutan slot pada output harus mengikuti urutan slot pada input


### Kompleksitas

**Time Complexity: O(N log N)**

Algoritma ini bekerja dalam beberapa langkah:
- Scan seluruh input dari awal sampai akhir untuk validasi dan hitung total → dilakukan 1x per slot
- Filter slot aktif → dilakukan 1x per slot
- **Sort (pengurutan)** slot aktif berdasarkan prioritas → inilah bagian yang paling "berat", karena untuk mengurutkan N data dibutuhkan sekitar N × log N langkah perbandingan
- Distribusi sisa dan rekonstruksi output → dilakukan 1x per slot

Karena langkah sorting mendominasi, total waktunya adalah **O(N log N)**.

**Space Complexity: O(N)**

Algoritma ini menyimpan beberapa data tambahan di memori:
- Array baru untuk hasil output (N slot)
- Daftar slot aktif beserta informasi indeks aslinya

Total memori tambahan yang dipakai sebanding dengan jumlah slot N, sehingga **O(N)**.


## 3 Edge Cases Tambahan

### Edge Case 1: Array Satu Elemen
**Input**: `[10]` atau `[0]`
**Bug yang dicegah**: IndexOutOfBounds atau error pada kondisi minimal.
Ketika hanya ada 1 slot (aktif/nonaktif), implementasi harus tetap bekerja
tanpa mengakses indeks yang tidak ada saat sorting dan rekonstruksi output.

### Edge Case 2: Nilai Sangat Besar
**Input**: `[1000000, 999999, 0, 1000001, 0, 0, 999998]`
**Bug yang dicegah**: Integer overflow pada kalkulasi `Sum()`.
Jika tipe data tidak cukup besar, total bisa overflow secara diam-diam
sehingga Invariant "total tidak berubah" dilanggar tanpa exception.

### Edge Case 3: Remainder Mendekati Jumlah Slot Aktif
**Input**: `[2, 5, 4, 0, 2, 0, 0]` (total=13, active=4, remainder=1)
**Bug yang dicegah**: Off-by-one error pada loop distribusi sisa.
Jika kondisi loop ditulis `<=` alih-alih `<`, satu slot ekstra
mendapat +1 sehingga total output berbeda dari total input.
