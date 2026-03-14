namespace PolishIdentifiers;

/// <summary>
/// Shared constants for the PESEL checksum algorithm.
/// Both <see cref="PeselValidator"/> and <see cref="PeselGenerator"/> use these
/// weights to keep generation and validation consistent.
/// </summary>
internal static class PeselAlgorithm
{
    // Weights for d0-d9; d10 is the check digit.
    internal static ReadOnlySpan<int> Weights => [1, 3, 7, 9, 1, 3, 7, 9, 1, 3];
}