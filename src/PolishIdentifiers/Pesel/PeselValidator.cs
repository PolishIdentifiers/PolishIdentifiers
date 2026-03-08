namespace PolishIdentifiers;

internal static class PeselValidator
{
    public static ValidationResult<PeselValidationError> Validate(ReadOnlySpan<char> value)
    {
        foreach (var c in value)
        {
            if (c < '0' || c > '9')
                return ValidationResult<PeselValidationError>.Failure(PeselValidationError.InvalidCharacters);
        }

        if (value.Length != 11)
            return ValidationResult<PeselValidationError>.Failure(PeselValidationError.InvalidLength);

        if (!IsDateValid(value))
            return ValidationResult<PeselValidationError>.Failure(PeselValidationError.InvalidDate);

        if (!IsChecksumValid(value))
            return ValidationResult<PeselValidationError>.Failure(PeselValidationError.InvalidChecksum);

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
        // Weights [1,3,7,9,1,3,7,9,1,3] inlined; check digit folded in so only one % 10 is needed.
        var sum = (value[0] - '0')
                + (value[1] - '0') * 3
                + (value[2] - '0') * 7
                + (value[3] - '0') * 9
                + (value[4] - '0')
                + (value[5] - '0') * 3
                + (value[6] - '0') * 7
                + (value[7] - '0') * 9
                + (value[8] - '0')
                + (value[9] - '0') * 3
                + (value[10] - '0');

        return sum % 10 == 0;
    }

    internal static ulong SpanToUlong(ReadOnlySpan<char> value)
    {
        ulong result = 0;
        foreach (var c in value)
            result = result * 10 + (ulong)(c - '0');
        return result;
    }
}
