namespace PolishIdentifiers;

/// <summary>
/// Formats a NIP value according to the requested <see cref="NipFormat"/>.
/// </summary>
internal static class NipFormatter
{
    internal static string Format(ulong value, NipFormat format)
    {
        var canonical = value.ToString("D10", System.Globalization.CultureInfo.InvariantCulture);

        return format switch
        {
            NipFormat.DigitsOnly => canonical,
            NipFormat.Hyphenated => $"{canonical.Substring(0, 3)}-{canonical.Substring(3, 3)}-{canonical.Substring(6, 2)}-{canonical.Substring(8, 2)}",
            NipFormat.VatEu => $"PL{canonical}",
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unknown NipFormat value.")
        };
    }
}
