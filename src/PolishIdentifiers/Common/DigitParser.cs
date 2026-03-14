namespace PolishIdentifiers;

/// <summary>
/// Provides fast, allocation-free parsing of digit spans into unsigned integers.
/// </summary>
internal static class DigitParser
{
    /// <summary>
    /// Converts a span of validated digit characters into a <see cref="ulong"/> value.
    /// Assumes the input contains only numeric characters ('0'-'9').
    /// </summary>
    internal static ulong ParseUInt64(ReadOnlySpan<char> value)
    {
        ulong result = 0;
        foreach (var c in value)
            result = result * 10 + (ulong)(c - '0');
        return result;
    }
}
