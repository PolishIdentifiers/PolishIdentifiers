namespace PolishIdentifiers;

/// <summary>
/// Internal validation logic for PESEL (Powszechny Elektroniczny System Ewidencji Ludności).
/// Checksum algorithm uses modulo 10. Also performs logical validation of the encoded birth date.
/// </summary>
internal static class PeselValidator
{
    public static ValidationResult<PeselValidationError> Validate(ReadOnlySpan<char> value)
    {
        if (!TryValidate(value, out var error))
            return ValidationResult<PeselValidationError>.Failure(error);

        return ValidationResult<PeselValidationError>.Valid();
    }

    public static ValidationResult<PeselValidationError> Validate(string? value)
    {
        if (value is null)
            return ValidationResult<PeselValidationError>.Failure(PeselValidationError.InvalidLength);
        return Validate(value.AsSpan());
    }

    private static bool IsDateValid(ReadOnlySpan<char> value)
    {
        var yy    = (value[0] - '0') * 10 + (value[1] - '0');
        var month = (value[2] - '0') * 10 + (value[3] - '0');
        var day   = (value[4] - '0') * 10 + (value[5] - '0');

        if (!PeselParser.TryDecodeYearMonth(month, yy, out var fullYear, out var actualMonth))
            return false;

        if (day < 1) return false;
        return day <= DateTime.DaysInMonth(fullYear, actualMonth);
    }

    private static bool IsChecksumValid(ReadOnlySpan<char> value)
    {
        var sum = ChecksumCalculator.WeightedSum(value, PeselAlgorithm.Weights);
        var checksum = (10 - (sum % 10)) % 10;

        return checksum == (value[10] - '0');
    }

    private static bool TryValidate(ReadOnlySpan<char> value, out PeselValidationError error)
    {
        foreach (var c in value)
        {
            if (c < '0' || c > '9')
            {
                error = PeselValidationError.InvalidCharacters;
                return false;
            }
        }

        if (value.Length != 11)
        {
            error = PeselValidationError.InvalidLength;
            return false;
        }

        if (!IsDateValid(value))
        {
            error = PeselValidationError.InvalidDate;
            return false;
        }

        if (!IsChecksumValid(value))
        {
            error = PeselValidationError.InvalidChecksum;
            return false;
        }

        error = default;
        return true;
    }

    /// <summary>
    /// Validates and converts the strict 11-digit PESEL representation.
    /// Used by <c>Parse</c> and <c>TryParse</c> to keep validation and construction in one place.
    /// Error order matches <see cref="Validate(ReadOnlySpan{char})"/>: characters → length → date → checksum.
    /// </summary>
    internal static bool TryParseCore(ReadOnlySpan<char> value, out ulong result, out PeselValidationError error)
    {
        result = 0;

        if (!TryValidate(value, out error))
            return false;

        result = DigitParser.ParseUInt64(value);
        return true;
    }
}
