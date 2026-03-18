namespace PolishIdentifiers;

/// <summary>
/// Checksum weights for PESEL. Shared between <see cref="PeselValidator"/> and
/// <see cref="PeselGenerator"/> to keep validation and generation consistent.
/// </summary>
internal static class PeselChecksumWeights
{
    // Weights for d0-d9; d10 is the check digit.
    internal static ReadOnlySpan<int> Weights => [1, 3, 7, 9, 1, 3, 7, 9, 1, 3];
}