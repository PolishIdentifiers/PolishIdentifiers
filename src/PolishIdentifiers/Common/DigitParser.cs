namespace PolishIdentifiers;

internal static class DigitParser
{
    internal static ulong ParseUInt64(ReadOnlySpan<char> value)
    {
        ulong result = 0;
        foreach (var c in value)
            result = result * 10 + (ulong)(c - '0');
        return result;
    }
}
