namespace PolishIdentifiers;

internal static class PeselParser
{
    // Decodes the PESEL-encoded month into a full year and the actual calendar month.
    // Returns false when encodedMonth does not belong to any valid range.
    internal static bool TryDecodeYearMonth(
        int encodedMonth, int yy, out int fullYear, out int actualMonth)
    {
        // 1900s checked first — the common case for the vast majority of living persons.
        if (encodedMonth is >= 1 and <= 12)  { fullYear = 1900 + yy; actualMonth = encodedMonth;      return true; }
        if (encodedMonth is >= 21 and <= 32) { fullYear = 2000 + yy; actualMonth = encodedMonth - 20; return true; }
        if (encodedMonth is >= 41 and <= 52) { fullYear = 2100 + yy; actualMonth = encodedMonth - 40; return true; }
        if (encodedMonth is >= 61 and <= 72) { fullYear = 2200 + yy; actualMonth = encodedMonth - 60; return true; }
        if (encodedMonth is >= 81 and <= 92) { fullYear = 1800 + yy; actualMonth = encodedMonth - 80; return true; }
        fullYear = 0; actualMonth = 0; return false;
    }

    // Assumes value is already a valid (validated) PESEL.
    internal static DateTime DecodeDate(ReadOnlySpan<char> value)
    {
        var yy    = (value[0] - '0') * 10 + (value[1] - '0');
        var month = (value[2] - '0') * 10 + (value[3] - '0');
        var day   = (value[4] - '0') * 10 + (value[5] - '0');
        TryDecodeYearMonth(month, yy, out var fullYear, out var actualMonth);
        return new DateTime(fullYear, actualMonth, day);
    }

    // Numeric fast path — safe because the gender digit is the 2nd from the right,
    // so it is unaffected by any leading zeros that were lost when storing as ulong.
    internal static Gender DecodeGender(ulong pesel)
        => ((pesel / 10) % 2) == 0 ? Gender.Female : Gender.Male;
}
