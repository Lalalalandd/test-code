using CarProductionBalancer.Core;

namespace CarProductionBalancer.Tests;

public class ProductionBalancerTests
{
    // Helper buat cek 3 invarian aturan bisnis
    private static void AssertTotalUnchanged(int[] input, int[] output)
    {
        Assert.Equal(input.Sum(), output.Sum());
    }

    private static void AssertInactiveSlotsRemainZero(int[] input, int[] output)
    {
        for (int i = 0; i < input.Length; i++)
            if (input[i] == 0)
                Assert.Equal(0, output[i]);
    }

    private static void AssertMaxDiffOne(int[] input, int[] output)
    {
        var activeOutputValues = output
            .Where((val, idx) => input[idx] > 0)
            .ToList();

        if (activeOutputValues.Count <= 1) return;

        int diff = activeOutputValues.Max() - activeOutputValues.Min();
        Assert.True(diff <= 1, $"Selisih slot aktif harus <= 1, tapi hasilnya {diff}");
    }

    private static void AssertAllInvariants(int[] input, int[] output)
    {
        AssertTotalUnchanged(input, output);
        AssertInactiveSlotsRemainZero(input, output);
        AssertMaxDiffOne(input, output);
    }

    // 1. Sample case dari soal
    [Fact]
    public void Balance_SampleCase_ReturnsExpectedOutput()
    {
        int[] input    = { 5, 3, 7, 0, 8, 0, 4 };
        int[] expected = { 5, 5, 6, 0, 6, 0, 5 };

        int[] result = ProductionBalancer.Balance(input);

        Assert.Equal(expected, result);
        AssertAllInvariants(input, result);
    }

    // 2. Total habis dibagi (sisa 0)
    [Fact]
    public void Balance_TotalDivisibleEvenly_AllActiveSlotsEqual()
    {
        int[] input    = { 3, 3, 3, 0, 3, 0, 0 };
        int[] expected = { 3, 3, 3, 0, 3, 0, 0 };

        int[] result = ProductionBalancer.Balance(input);

        Assert.Equal(expected, result);
        AssertAllInvariants(input, result);
    }

    // 3. Total bersisa (sisa > 0)
    [Fact]
    public void Balance_TotalWithRemainder_DistributesRemainderCorrectly()
    {
        int[] input    = { 3, 1, 6, 0, 0, 0, 0 };
        int[] expected = { 3, 3, 4, 0, 0, 0, 0 };

        int[] result = ProductionBalancer.Balance(input);

        Assert.Equal(expected, result);
        AssertAllInvariants(input, result);
    }

    // 4. Semua slot 0 (libur semua)
    [Fact]
    public void Balance_AllZeros_ReturnsAllZeros()
    {
        int[] input    = { 0, 0, 0, 0, 0, 0, 0 };
        int[] expected = { 0, 0, 0, 0, 0, 0, 0 };

        int[] result = ProductionBalancer.Balance(input);

        Assert.Equal(expected, result);
    }

    // 5. Cuma 1 slot yang aktif
    [Fact]
    public void Balance_SingleActiveSlot_GetsEntireTotal()
    {
        int[] input    = { 0, 0, 15, 0, 0, 0, 0 };
        int[] expected = { 0, 0, 15, 0, 0, 0, 0 };

        int[] result = ProductionBalancer.Balance(input);

        Assert.Equal(expected, result);
        AssertAllInvariants(input, result);
    }

    // 6. Tie-breaker kalau nilai awalnya sama
    [Fact]
    public void Balance_TieInOriginalQuantity_EarlierSlotGetsRemainder()
    {
        int[] input    = { 2, 2, 3, 0, 0, 0, 0 };
        int[] expected = { 2, 2, 3, 0, 0, 0, 0 };

        int[] result = ProductionBalancer.Balance(input);

        Assert.Equal(expected, result);
        AssertAllInvariants(input, result);
    }

    [Fact]
    public void Balance_TieBreaker_SlotWithSameQty_EarlierIndexWins()
    {
        int[] input    = { 5, 5, 5, 0, 0, 0, 0 };
        int[] expected = { 5, 5, 5, 0, 0, 0, 0 };

        int[] result = ProductionBalancer.Balance(input);

        Assert.Equal(expected, result);
        AssertAllInvariants(input, result);
    }

    [Fact]
    public void Balance_TieBreaker_RemainderGoesToEarlierIndex()
    {
        int[] input    = { 2, 2, 2, 0, 0, 0, 2 };
        int[] expected = { 2, 2, 2, 0, 0, 0, 2 };

        int[] result = ProductionBalancer.Balance(input);

        Assert.Equal(expected, result);
        AssertAllInvariants(input, result);
    }

    // 7. Input tidak valid (minus atau null)
    [Fact]
    public void Balance_NegativeValue_ThrowsArgumentException()
    {
        int[] input = { 5, -3, 7, 0, 8, 0, 4 };
        Assert.Throws<ArgumentException>(() => ProductionBalancer.Balance(input));
    }

    [Fact]
    public void Balance_NullInput_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => ProductionBalancer.Balance(null!));
    }

    // Edge case 1: Array cuma 1 elemen
    [Fact]
    public void Balance_SingleElement_NonZero_ReturnsSameValue()
    {
        int[] input    = { 10 };
        int[] expected = { 10 };

        int[] result = ProductionBalancer.Balance(input);

        Assert.Equal(expected, result);
        AssertAllInvariants(input, result);
    }

    [Fact]
    public void Balance_SingleElement_Zero_ReturnsZero()
    {
        int[] input    = { 0 };
        int[] expected = { 0 };

        int[] result = ProductionBalancer.Balance(input);

        Assert.Equal(expected, result);
    }

    // Edge case 2: Angka jutaan / besar
    [Fact]
    public void Balance_LargeValues_DoesNotOverflow()
    {
        int[] input = { 1000000, 999999, 0, 1000001, 0, 0, 999998 };

        int[] result = ProductionBalancer.Balance(input);

        AssertAllInvariants(input, result);
        Assert.Equal(input.Sum(), result.Sum());
    }

    // Edge case 3: Sisa bagi hampir sama kayak jumlah slot aktif
    [Fact]
    public void Balance_RemainderAlmostEqualToActiveCount_DistributesCorrectly()
    {
        int[] input    = { 2, 5, 4, 0, 2, 0, 0 };
        int[] expected = { 3, 4, 3, 0, 3, 0, 0 };

        int[] result = ProductionBalancer.Balance(input);

        Assert.Equal(expected, result);
        AssertAllInvariants(input, result);
    }
}
