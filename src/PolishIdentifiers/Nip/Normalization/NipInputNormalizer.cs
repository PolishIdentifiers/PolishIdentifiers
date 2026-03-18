namespace PolishIdentifiers;

/// <summary>
/// Recognizes and normalizes the five supported NIP input formats
/// into a canonical 10-digit span. Returns <c>false</c> when the
/// input does not match any recognized pattern.
/// </summary>
/// <remarks>
/// Supported formats:
/// <list type="number">
///   <item><c>1234563218</c> — Canonical (10 digits)</item>
///   <item><c>123-456-32-18</c> — Hyphenated (13 chars)</item>
///   <item><c>PL1234563218</c> — PL prefix without space (12 chars)</item>
///   <item><c>PL 1234563218</c> — PL prefix with space (13 chars)</item>
///   <item><c>PL 123-456-32-18</c> — PL prefix + space + hyphens (16 chars)</item>
/// </list>
/// Everything else → <see cref="NipValidationError.UnrecognizedFormat"/>.
/// </remarks>
internal static class NipInputNormalizer
{
    /// <summary>
    /// Attempts to recognize the input format and extract 10 digits into <paramref name="digits"/>.
    /// </summary>
    /// <param name="value">Raw input.</param>
    /// <param name="digits">A span of at least 10 chars to receive the extracted digits.</param>
    /// <returns><c>true</c> if the format was recognized and digits extracted; <c>false</c> otherwise.</returns>
    internal static bool TryNormalize(ReadOnlySpan<char> value, Span<char> digits)
    {
        // Format 1: Canonical — 10 digits
        if (value.Length == 10 && AllDigits(value))
        {
            value.CopyTo(digits);
            return true;
        }

        // Format 2: Hyphenated — 123-456-32-18 (13 chars)
        if (value.Length == 13 && IsHyphenated(value))
        {
            ExtractHyphenated(value, digits);
            return true;
        }

        // Format 3: PL prefix no space — PL1234563218 (12 chars)
        if (value.Length == 12 && HasPlPrefix(value) && AllDigits(value.Slice(2)))
        {
            value.Slice(2).CopyTo(digits);
            return true;
        }

        // Format 4: PL prefix with space — PL 1234563218 (13 chars)
        if (value.Length == 13 && HasPlPrefix(value) && value[2] == ' ' && AllDigits(value.Slice(3)))
        {
            value.Slice(3).CopyTo(digits);
            return true;
        }

        // Format 5: PL prefix + space + hyphens — PL 123-456-32-18 (16 chars)
        if (value.Length == 16 && HasPlPrefix(value) && value[2] == ' ' && IsHyphenated(value.Slice(3)))
        {
            ExtractHyphenated(value.Slice(3), digits);
            return true;
        }

        return false;
    }

    private static bool AllDigits(ReadOnlySpan<char> span)
    {
        foreach (var c in span)
        {
            if (c < '0' || c > '9') return false;
        }
        return true;
    }

    private static bool HasPlPrefix(ReadOnlySpan<char> span)
        => span.Length >= 2 && span[0] == 'P' && span[1] == 'L';

    /// <summary>
    /// Checks whether a 13-char span matches the hyphenated pattern: ddd-ddd-dd-dd.
    /// </summary>
    private static bool IsHyphenated(ReadOnlySpan<char> span)
    {
        if (span.Length != 13) return false;
        if (span[3] != '-' || span[7] != '-' || span[10] != '-') return false;

        // Check all non-hyphen positions are digits.
        for (int i = 0; i < 13; i++)
        {
            if (i == 3 || i == 7 || i == 10) continue;
            if (span[i] < '0' || span[i] > '9') return false;
        }

        return true;
    }

    /// <summary>
    /// Extracts 10 digits from hyphenated format ddd-ddd-dd-dd.
    /// </summary>
    private static void ExtractHyphenated(ReadOnlySpan<char> hyphenated, Span<char> digits)
    {
        // 123-456-32-18 → positions: 0,1,2, skip 3, 4,5,6, skip 7, 8,9, skip 10, 11,12
        int di = 0;
        for (int i = 0; i < 13; i++)
        {
            if (i == 3 || i == 7 || i == 10) continue;
            digits[di++] = hyphenated[i];
        }
    }
}
