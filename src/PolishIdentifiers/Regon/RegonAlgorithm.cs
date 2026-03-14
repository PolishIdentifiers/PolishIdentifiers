namespace PolishIdentifiers;

/// <summary>
/// Shared constants for the REGON checksum algorithm.
/// Both <see cref="RegonValidator"/> and <see cref="RegonGenerator"/> use these
/// weights to keep generation and validation consistent.
/// </summary>
internal static class RegonAlgorithm
{
    // Weights for d0-d7; d8 is the check digit.
    internal static ReadOnlySpan<int> Weights9 => [8, 9, 2, 3, 4, 5, 6, 7];

    // Weights for d0-d12; d13 is the check digit.
    internal static ReadOnlySpan<int> Weights14 => [2, 4, 8, 5, 0, 9, 7, 3, 6, 1, 2, 4, 8];
}