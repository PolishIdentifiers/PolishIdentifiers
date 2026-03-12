namespace PolishIdentifiers;

/// <summary>
/// Internal validation logic for REGON (Rejestr Gospodarki Narodowej).
/// Supports both 9-digit (REGON-9, primary entity) and 14-digit (REGON-14, local unit) variants.
/// Both checksum algorithms use mod 11; when the remainder equals 10, the check digit is 0.
/// REGON-14 validation is two-step: the embedded 9-digit base is validated first.
/// </summary>
internal static class RegonValidator
{
    // REGON-9 weights for digits d0–d7; check digit is d8.
    private static ReadOnlySpan<int> Weights9 => [8, 9, 2, 3, 4, 5, 6, 7];

    // REGON-14 weights for digits d0–d12; check digit is d13.
    private static ReadOnlySpan<int> Weights14 => [2, 4, 8, 5, 0, 9, 7, 3, 6, 1, 2, 4, 8];

    public static ValidationResult<RegonValidationError> Validate(ReadOnlySpan<char> value)
    {
        if (!TryValidate(value, out var error, out _))
            return ValidationResult<RegonValidationError>.Failure(error);

        return ValidationResult<RegonValidationError>.Valid();
    }

    public static ValidationResult<RegonValidationError> Validate(string? value)
    {
        if (value is null)
            return ValidationResult<RegonValidationError>.Failure(RegonValidationError.InvalidLength);
        return Validate(value.AsSpan());
    }

    /// <summary>
    /// Validates and converts the canonical REGON representation.
    /// Used by <c>Parse</c> and <c>TryParse</c> to keep validation and construction in one place.
    /// Error order matches <see cref="Validate(ReadOnlySpan{char})"/>: characters → length → checksum.
    /// </summary>
    internal static bool TryParseCore(
        ReadOnlySpan<char> value,
        out ulong result,
        out bool isLocal,
        out RegonValidationError error)
    {
        result = 0;
        isLocal = false;

        if (!TryValidate(value, out error, out isLocal))
            return false;

        result = DigitParser.ParseUInt64(value);
        return true;
    }

    private static bool TryValidate(ReadOnlySpan<char> value, out RegonValidationError error, out bool isLocal)
    {
        isLocal = false;

        foreach (var c in value)
        {
            if (c < '0' || c > '9')
            {
                error = RegonValidationError.InvalidCharacters;
                return false;
            }
        }

        if (value.Length != 9 && value.Length != 14)
        {
            error = RegonValidationError.InvalidLength;
            return false;
        }

        if (value.Length == 9)
        {
            if (!IsChecksum9Valid(value))
            {
                error = RegonValidationError.InvalidChecksum;
                return false;
            }
        }
        else
        {
            // Two-step validation for REGON-14: base REGON-9 first, then full 14-digit checksum.
            if (!IsChecksum9Valid(value.Slice(0, 9)))
            {
                error = RegonValidationError.InvalidChecksum;
                return false;
            }

            if (!IsChecksum14Valid(value))
            {
                error = RegonValidationError.InvalidChecksum;
                return false;
            }

            isLocal = true;
        }

        error = default;
        return true;
    }

    private static bool IsChecksum9Valid(ReadOnlySpan<char> value)
    {
        var sum = ChecksumCalculator.WeightedSum(value.Slice(0, 8), Weights9);
        var r = sum % 11;
        var checkDigit = r == 10 ? 0 : r;
        return checkDigit == (value[8] - '0');
    }

    private static bool IsChecksum14Valid(ReadOnlySpan<char> value)
    {
        var sum = ChecksumCalculator.WeightedSum(value.Slice(0, 13), Weights14);
        var r = sum % 11;
        var checkDigit = r == 10 ? 0 : r;
        return checkDigit == (value[13] - '0');
    }
}
