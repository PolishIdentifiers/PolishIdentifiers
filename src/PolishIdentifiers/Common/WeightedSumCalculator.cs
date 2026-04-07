namespace PolishIdentifiers;

/// <summary>
/// Provides weighted-sum computation for Polish identifier validation.
/// </summary>
internal static class WeightedSumCalculator
{
    /// <summary>
    /// Computes a weighted sum for purely numeric identifiers (PESEL, NIP, REGON).
    /// </summary>
    internal static int WeightedSum(ReadOnlySpan<char> digits, ReadOnlySpan<int> weights)
    {
        int sum = 0;
        for (int i = 0; i < weights.Length; i++)
            sum += (digits[i] - '0') * weights[i];
        return sum;
    }

}
