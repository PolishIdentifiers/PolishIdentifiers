namespace PolishIdentifiers;

/// <summary>
/// Provides weighted-sum checksum calculation for Polish identifiers.
/// </summary>
internal static class ChecksumCalculator
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

    /// <summary>
    /// Computes a weighted sum for alphanumeric identifiers (PolishIdCardNumber, PolishPassportNumber).
    /// Letters are mapped A=10, B=11, ..., Z=35.
    /// </summary>
    internal static int WeightedSumAlphanumeric(ReadOnlySpan<char> chars, ReadOnlySpan<int> weights)
    {
        int sum = 0;
        for (int i = 0; i < weights.Length; i++)
            sum += CharToValue(chars[i]) * weights[i];
        return sum;
    }

    private static int CharToValue(char c)
        => c >= 'A' && c <= 'Z' ? c - 'A' + 10 : c - '0';
}
