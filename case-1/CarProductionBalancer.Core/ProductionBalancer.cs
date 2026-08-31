namespace CarProductionBalancer.Core;

public static class ProductionBalancer
{
    public static int[] Balance(int[] planningData)
    {
        // Validasi input dulu, ga boleh null atau minus
        if (planningData == null)
            throw new ArgumentException("Input cannot be null.");

        foreach (var value in planningData)
        {
            if (value < 0)
                throw new ArgumentException($"Input contains invalid value: {value}. All values must be non-negative integers.");
        }

        int n = planningData.Length;
        int[] result = new int[n];

        // Ambil slot yang aktif (> 0) dan simpen index aslinya
        var activeSlots = new List<(int index, int originalQty)>();
        for (int i = 0; i < n; i++)
        {
            if (planningData[i] > 0)
                activeSlots.Add((i, planningData[i]));
        }

        // Kalau libur semua (semua 0), langsung balikin 0 semua
        if (activeSlots.Count == 0)
            return result;

        // Hitung kuota rata-rata sama sisa pembagiannya
        int totalPlan = planningData.Sum();
        int activeCount = activeSlots.Count;
        int baseQty = totalPlan / activeCount;
        int remainder = totalPlan % activeCount;

        // Kasih kuota dasar ke semua slot yang aktif
        foreach (var slot in activeSlots)
            result[slot.index] = baseQty;

        // Urutkan slot aktif: rencana awal terbesar dulu, kalau sama urutkan dari hari/index awal
        var sortedSlots = activeSlots
            .OrderByDescending(s => s.originalQty)
            .ThenBy(s => s.index)
            .ToList();

        // Bagiin sisa kuota (+1) ke slot prioritas teratas
        for (int i = 0; i < remainder; i++)
            result[sortedSlots[i].index] += 1;

        // Slot nonaktif otomatis tetap 0 karena default int array
        return result;
    }
}
