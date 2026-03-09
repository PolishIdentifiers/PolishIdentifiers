namespace PolishIdentifiers;

/// <summary>
/// Internal validation logic for NIP (Numer Identyfikacji Podatkowej).
/// Checksum algorithm uses mod 11. If the weighted sum mod 11 equals 10,
/// no valid check digit exists for that combination — the NIP is invalid.
/// </summary>
internal static class NipValidator
{
    private static ReadOnlySpan<int> Weights => [6, 5, 7, 2, 3, 4, 5, 6, 7];

    public static ValidationResult<NipValidationError> Validate(ReadOnlySpan<char> value)
    {
        foreach (var c in value)
        {
            if (c < '0' || c > '9')
                return ValidationResult<NipValidationError>.Failure(NipValidationError.InvalidCharacters);
        }

        if (value.Length != 10)
            return ValidationResult<NipValidationError>.Failure(NipValidationError.InvalidLength);

        if (!IsChecksumValid(value))
            return ValidationResult<NipValidationError>.Failure(NipValidationError.InvalidChecksum);

        return ValidationResult<NipValidationError>.Valid();
    }

    public static ValidationResult<NipValidationError> Validate(string? value)
    {
        if (value is null)
            return ValidationResult<NipValidationError>.Failure(NipValidationError.InvalidLength);
        return Validate(value.AsSpan());
    }

    private static bool IsChecksumValid(ReadOnlySpan<char> value)
    {
        var sum = ChecksumCalculator.WeightedSum(value, Weights);
        var checksum = sum % 11;

        // If checksum == 10, no valid check digit exists for this combination.
        if (checksum == 10)
            return false;

        return checksum == (value[9] - '0');
    }

    internal static ulong SpanToUlong(ReadOnlySpan<char> value)
    {
        ulong result = 0;
        foreach (var c in value)
            result = result * 10 + (ulong)(c - '0');
        return result;
    }

    /// <summary>
    /// Validates and converts in a single pass. Used by <c>Parse</c> and <c>TryParse</c>
    /// to avoid iterating the span twice (once for <see cref="Validate(ReadOnlySpan{char})"/>, once for <see cref="SpanToUlong"/>).
    /// Error order matches <see cref="Validate(ReadOnlySpan{char})"/>: characters → length → checksum.
    /// </summary>
    internal static bool TryParseCore(ReadOnlySpan<char> value, out ulong result, out NipValidationError error)
    {
        result = 0;

        // Pass 1: scan chars, accumulate ulong, accumulate weighted sum — all in one loop.
        // We still need the full char scan before we can confirm InvalidLength,
        // because InvalidCharacters must be reported before InvalidLength per spec.
        int weightedSum = 0;
        ulong acc = 0;
        ReadOnlySpan<int> weights = [6, 5, 7, 2, 3, 4, 5, 6, 7];

        for (int i = 0; i < value.Length; i++)
        {
            var c = value[i];
            if (c < '0' || c > '9')
            {
                error = NipValidationError.InvalidCharacters;
                return false;
            }

            var d = (ulong)(c - '0');
            acc = acc * 10 + d;

            if (i < 9)
                weightedSum += (int)d * weights[i];
        }

        if (value.Length != 10)
        {
            error = NipValidationError.InvalidLength;
            return false;
        }

        var checksum = weightedSum % 11;
        if (checksum == 10 || checksum != (value[9] - '0'))
        {
            error = NipValidationError.InvalidChecksum;
            return false;
        }

        result = acc;
        error = default;
        return true;
    }
}
