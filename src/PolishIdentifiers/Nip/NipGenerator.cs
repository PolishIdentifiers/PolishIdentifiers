namespace PolishIdentifiers;

/// <summary>
/// Provides methods for generating valid and deliberately invalid NIP numbers,
/// primarily intended for use in tests and tooling.
/// </summary>
public static class NipGenerator
{
#if NET10_0_OR_GREATER
    private static int NextDigit() => System.Random.Shared.Next(10);
#else
    private static readonly ThreadLocal<Random> Rng = new(() => new Random(Guid.NewGuid().GetHashCode()));

    private static Random CurrentRng
        => Rng.Value ?? throw new InvalidOperationException("Random generator is not available.");

    private static int NextDigit() => CurrentRng.Next(10);
#endif

    private static readonly int[] ChecksumWeights = [6, 5, 7, 2, 3, 4, 5, 6, 7];

    // --- Valid generators ---

    /// <summary>Generates a random valid NIP.</summary>
    /// <returns>A valid <see cref="Nip"/> instance.</returns>
    /// <remarks>This method is thread-safe.</remarks>
    public static Nip Random()
    {
        int[] digits;
        int checksum;

        do
        {
            digits = new int[10];
            for (var i = 0; i < 9; i++)
                digits[i] = NextDigit();

            var sum = 0;
            for (var i = 0; i < 9; i++)
                sum += digits[i] * ChecksumWeights[i];

            checksum = sum % 11;
        }
        while (checksum == 10); // Retry if no valid check digit exists for this combination.

        digits[9] = checksum;

        ulong value = 0;
        foreach (var d in digits)
            value = value * 10 + (ulong)d;

        return new Nip(value);
    }

    // --- Invalid generators (return string — Nip.Parse would throw) ---

    /// <summary>
    /// Provides methods for generating deliberately invalid NIP strings.
    /// Each method violates exactly one validation rule while keeping all others correct,
    /// enabling precise unit testing of individual error codes.
    /// </summary>
    public static class Invalid
    {
        /// <summary>
        /// Generates a NIP string with a wrong check digit.
        /// Triggers <see cref="NipValidationError.InvalidChecksum"/>.
        /// </summary>
        /// <returns>A 10-digit string that fails checksum validation.</returns>
        public static string WrongChecksum()
        {
            var chars = NipGenerator.Random().ToString().ToCharArray();
            chars[9] = (char)('0' + (chars[9] - '0' + 1) % 10);
            return new string(chars);
        }

        /// <summary>
        /// Returns a fixed 9-digit string that is one character too short.
        /// Triggers <see cref="NipValidationError.InvalidLength"/>.
        /// </summary>
        /// <returns>A 9-character string that fails length validation.</returns>
        public static string WrongLength() => "123456321";

        /// <summary>
        /// Generates a NIP string that is 10 characters long but contains a non-digit character.
        /// Triggers <see cref="NipValidationError.InvalidCharacters"/>.
        /// </summary>
        /// <returns>A 10-character string that fails character validation.</returns>
        public static string NonNumeric()
        {
            var chars = NipGenerator.Random().ToString().ToCharArray();
            chars[5] = 'X';
            return new string(chars);
        }
    }
}
