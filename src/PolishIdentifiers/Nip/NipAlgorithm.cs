namespace PolishIdentifiers;

/// <summary>
/// Shared constants for the NIP checksum algorithm.
/// Both <see cref="NipValidator"/> and <see cref="NipGenerator"/> use these
/// weights to keep generation and validation consistent.
/// </summary>
internal static class NipAlgorithm
{
    // Weights for d0-d8; d9 is the check digit.
    internal static ReadOnlySpan<int> Weights => [6, 5, 7, 2, 3, 4, 5, 6, 7];
}