namespace PolishIdentifiers;

/// <summary>
/// Internal validation logic for NIP (Numer Identyfikacji Podatkowej).
/// Checksum algorithm uses mod 11. If the weighted sum mod 11 equals 10,
/// no valid check digit exists for that combination — the NIP is invalid.
/// </summary>
internal static class NipValidator
{
    public static ValidationResult<NipValidationError> Validate(ReadOnlySpan<char> value)
    {
        Span<char> digits = stackalloc char[10];
        if (!TryNormalizeAndValidate(value, digits, out var error))
            return ValidationResult<NipValidationError>.Failure(error);

        return ValidationResult<NipValidationError>.Valid();
    }

    private static bool TryNormalizeAndValidate(ReadOnlySpan<char> value, Span<char> digits, out NipValidationError error)
    {
        foreach (var c in value)
        {
            if ((c < '0' || c > '9') && c != 'P' && c != 'L' && c != ' ' && c != '-')
            {
                error = NipValidationError.InvalidCharacters;
                return false;
            }
        }

        if (IsAllDigits(value))
        {
            if (value.Length != 10)
            {
                error = NipValidationError.InvalidLength;
                return false;
            }

            value.CopyTo(digits);
        }
        else if (!NipInputNormalizer.TryNormalize(value, digits))
        {
            error = NipValidationError.UnrecognizedFormat;
            return false;
        }

        if (!IsChecksumValid(digits))
        {
            error = NipValidationError.InvalidChecksum;
            return false;
        }

        error = default;
        return true;
    }

    public static ValidationResult<NipValidationError> Validate(string? value)
    {
        if (value is null)
            return ValidationResult<NipValidationError>.Failure(NipValidationError.InvalidLength);
        return Validate(value.AsSpan());
    }

    private static bool IsAllDigits(ReadOnlySpan<char> value)
    {
        foreach (var c in value)
        {
            if (c < '0' || c > '9')
                return false;
        }

        return true;
    }

    private static bool IsChecksumValid(ReadOnlySpan<char> value)
    {
        var sum = WeightedSumCalculator.WeightedSum(value.Slice(0, 9), NipChecksumWeights.Weights);
        var checkDigit = sum % 11;

        // If check digit == 10, no valid check digit exists for this combination.
        if (checkDigit == 10)
            return false;

        return checkDigit == (value[9] - '0');
    }

    /// <summary>
    /// Validates and converts any supported public NIP text representation.
    /// Used by <c>Parse</c> and <c>TryParse</c> to keep validation and construction in one place.
    /// Error order matches <see cref="Validate(ReadOnlySpan{char})"/>: characters → length → format → checksum.
    /// </summary>
    internal static bool TryParseCore(ReadOnlySpan<char> value, out ulong result, out NipValidationError error)
    {
        result = 0;
        Span<char> digits = stackalloc char[10];

        if (!TryNormalizeAndValidate(value, digits, out error))
            return false;

        result = DigitParser.ParseUInt64(digits);
        return true;
    }
}
