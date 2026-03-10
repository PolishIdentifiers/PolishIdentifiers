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
        if (!TryValidate(value, out var error))
            return ValidationResult<NipValidationError>.Failure(error);

        return ValidationResult<NipValidationError>.Valid();
    }

    private static bool TryValidate(ReadOnlySpan<char> value, out NipValidationError error)
    {
        if (!TryScanDigits(value, out var weightedSum))
        {
            error = NipValidationError.InvalidCharacters;
            return false;
        }

        if (value.Length != 10)
        {
            error = NipValidationError.InvalidLength;
            return false;
        }

        if (!IsChecksumValid(value, weightedSum))
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

    private static bool IsChecksumValid(ReadOnlySpan<char> value, int sum)
    {
        var checksum = sum % 11;

        // If checksum == 10, no valid check digit exists for this combination.
        if (checksum == 10)
            return false;

        return checksum == (value[9] - '0');
    }

    private static bool TryScanDigits(ReadOnlySpan<char> value, out int weightedSum)
    {
        weightedSum = 0;

        for (int i = 0; i < value.Length; i++)
        {
            var c = value[i];
            if (c < '0' || c > '9')
                return false;

            if (i < Weights.Length)
                weightedSum += (c - '0') * Weights[i];
        }

        return true;
    }

    /// <summary>
    /// Validates and converts the strict 10-digit NIP representation.
    /// Used by <c>Parse</c> and <c>TryParse</c> to keep validation and construction in one place.
    /// Error order matches <see cref="Validate(ReadOnlySpan{char})"/>: characters → length → checksum.
    /// </summary>
    internal static bool TryParseCore(ReadOnlySpan<char> value, out ulong result, out NipValidationError error)
    {
        result = 0;

        if (!TryValidate(value, out error))
            return false;

        result = DigitParser.ParseUInt64(value);
        return true;
    }
}
