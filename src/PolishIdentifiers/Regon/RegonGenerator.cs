namespace PolishIdentifiers;

/// <summary>
/// Provides methods for generating valid and deliberately invalid REGON numbers,
/// primarily intended for use in tests and tooling.
/// </summary>
public static class RegonGenerator
{
#if NET10_0_OR_GREATER
    private static int NextDigit() => System.Random.Shared.Next(10);
    private static int NextInt(int maxValue) => System.Random.Shared.Next(maxValue);
#else
    private static readonly ThreadLocal<Random> Rng = new(() => new Random(Guid.NewGuid().GetHashCode()));

    private static Random CurrentRng
        => Rng.Value ?? throw new InvalidOperationException("Random generator is not available.");

    private static int NextDigit() => CurrentRng.Next(10);
    private static int NextInt(int maxValue) => CurrentRng.Next(maxValue);
#endif

    // REGON-9 weights for d0–d7; d8 is the check digit.
    private static readonly int[] Weights9 = [8, 9, 2, 3, 4, 5, 6, 7];

    // REGON-14 weights for d0–d12; d13 is the check digit.
    private static readonly int[] Weights14 = [2, 4, 8, 5, 0, 9, 7, 3, 6, 1, 2, 4, 8];

    // --- Valid generators ---

    /// <summary>Generates a random valid 9-digit REGON (REGON-9, primary entity).</summary>
    /// <returns>A valid <see cref="Regon"/> instance with <see cref="Regon.Kind"/> == <see cref="RegonKind.Main"/>.</returns>
    /// <remarks>This method is thread-safe.</remarks>
    public static Regon Random()
    {
        var digits = new int[9];

        for (var i = 0; i < 8; i++)
            digits[i] = NextDigit();

        var sum = 0;
        for (var i = 0; i < 8; i++)
            sum += digits[i] * Weights9[i];

        var r = sum % 11;
        digits[8] = r == 10 ? 0 : r;

        ulong value = 0;
        foreach (var d in digits)
            value = value * 10 + (ulong)d;

        return new Regon(value, isLocal: false);
    }

    /// <summary>Generates a random valid 14-digit REGON (REGON-14, local unit).</summary>
    /// <returns>A valid <see cref="Regon"/> instance with <see cref="Regon.Kind"/> == <see cref="RegonKind.Local"/>.</returns>
    /// <remarks>This method is thread-safe. The embedded 9-digit base is always a valid REGON-9.</remarks>
    public static Regon RandomLocal()
    {
        var base9 = Random();
        var base9Str = base9.ToString(); // "D9" — 9 digits

        var digits = new int[14];
        for (var i = 0; i < 9; i++)
            digits[i] = base9Str[i] - '0';

        for (var i = 9; i < 13; i++)
            digits[i] = NextDigit();

        var sum = 0;
        for (var i = 0; i < 13; i++)
            sum += digits[i] * Weights14[i];

        var r = sum % 11;
        digits[13] = r == 10 ? 0 : r;

        ulong value = 0;
        foreach (var d in digits)
            value = value * 10 + (ulong)d;

        return new Regon(value, isLocal: true);
    }

    // --- Invalid generators (return string — Regon.Parse would throw) ---

    /// <summary>
    /// Provides methods for generating deliberately invalid REGON strings.
    /// Each method violates exactly one validation rule while keeping all others correct,
    /// enabling precise unit testing of individual error codes.
    /// </summary>
    public static class Invalid
    {
        private const int MaxLengthDelta = 3;

        /// <summary>
        /// Generates a REGON-9 string with a wrong check digit.
        /// Triggers <see cref="RegonValidationError.InvalidChecksum"/>.
        /// </summary>
        /// <returns>A 9-digit string that fails checksum validation.</returns>
        public static string WrongChecksum()
        {
            var chars = RegonGenerator.Random().ToString().ToCharArray();
            chars[8] = (char)('0' + (chars[8] - '0' + 1) % 10);
            return new string(chars);
        }

        /// <summary>
        /// Returns a digit-only REGON string with an invalid length (neither 9 nor 14 digits).
        /// Triggers <see cref="RegonValidationError.InvalidLength"/>.
        /// </summary>
        /// <returns>A digit-only string whose length is between 6–8 or 10–12 characters.</returns>
        public static string WrongLength()
        {
            var value = RegonGenerator.Random().ToString(); // 9 digits
            var delta = NextInt(MaxLengthDelta) + 1;        // 1–3

            // Shorten → 6–8 chars, or lengthen → 10–12 chars. Neither is 9 or 14.
            return NextInt(2) == 0
                ? value.Substring(0, value.Length - delta)
                : AppendRandomDigits(value, delta);
        }

        /// <summary>
        /// Generates a REGON-9 string that contains a non-digit character at a random position.
        /// Triggers <see cref="RegonValidationError.InvalidCharacters"/>.
        /// </summary>
        /// <returns>A 9-character string that fails character validation.</returns>
        public static string NonNumeric()
        {
            var chars = RegonGenerator.Random().ToString().ToCharArray();
            var pos = NextInt(chars.Length);
            chars[pos] = 'X';
            return new string(chars);
        }

        private static string AppendRandomDigits(string value, int count)
        {
            var chars = new char[value.Length + count];
            value.AsSpan().CopyTo(chars);

            for (var i = 0; i < count; i++)
                chars[value.Length + i] = (char)('0' + NextDigit());

            return new string(chars);
        }
    }
}
