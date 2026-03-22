namespace PolishIdentifiers;

/// <summary>
/// Checksum weights for NIP. Shared between <see cref="NipValidator"/> and
/// <see cref="NipGenerator"/> to keep validation and generation consistent.
/// </summary>
internal static class NipChecksumWeights
{
    // Weights for d0-d8; d9 is the check digit.
    internal static ReadOnlySpan<int> Weights => [6, 5, 7, 2, 3, 4, 5, 6, 7];
}