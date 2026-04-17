namespace PolishIdentifiers;

/// <summary>
/// Provides methods for generating valid and deliberately invalid NIP numbers,
/// primarily intended for use in tests and tooling.
/// </summary>
public static class NipGenerator
{
    // RNG strategy is split by target framework — this is intentional. Do not consolidate.
    // net10.0:          Random.Shared is the platform's purpose-built concurrent RNG:
    //                   thread-safe without locks, OS-entropy seeded, no per-thread state.
    // netstandard2.0:   Random.Shared is not part of the netstandard2.0 API surface.
    //                   ThreadLocal<Random> gives each thread an independent instance,
    //                   eliminating data races on a shared Random field.
    //                   Seeded via Guid.NewGuid().GetHashCode() because the netstandard2.0
    //                   contract does not guarantee OS-entropy seeding across all conforming
    //                   runtimes — older .NET Framework derives seeds from Environment.TickCount,
    //                   where threads initialized within the same millisecond receive identical
    //                   seeds and produce identical sequences.
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

    // --- Valid generators ---

    /// <summary>Generates a valid NIP.</summary>
    /// <returns>A valid <see cref="Nip"/> instance.</returns>
    /// <remarks>This method is thread-safe.</remarks>
    public static Nip Generate()
    {
        Span<int> digits = stackalloc int[10]; // Declared outside the loop; indices [0..8] are always written before read.
        int checkDigit;

        do
        {
            for (var i = 0; i < 9; i++)
                digits[i] = NextDigit();

            var sum = 0;
            for (var i = 0; i < 9; i++)
                sum += digits[i] * NipChecksumWeights.Weights[i];

            checkDigit = sum % 11;
        }
        while (checkDigit == 10); // Retry if no valid check digit exists for this combination.

        digits[9] = checkDigit;

        ulong value = 0;
        foreach (var d in digits)
            value = value * 10 + (ulong)d;

        return new Nip(value);
    }

    /// <summary>Generates <paramref name="count"/> valid NIPs.</summary>
    /// <param name="count">The number of NIP values to generate. Must be zero or greater.</param>
    /// <returns>A read-only list of <paramref name="count"/> valid <see cref="Nip"/> instances.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count"/> is negative.</exception>
    /// <remarks>This method is thread-safe.</remarks>
    public static IReadOnlyList<Nip> Generate(int count)
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), count, "Count must be zero or greater.");
        var result = new List<Nip>(count);
        for (var i = 0; i < count; i++)
            result.Add(Generate());
        return result;
    }


    // --- Invalid generators (return string — Nip.Parse would throw) ---

    /// <summary>
    /// Provides methods for generating deliberately invalid NIP strings.
    /// Each method violates exactly one validation rule while keeping all others correct,
    /// enabling precise unit testing of individual error codes.
    /// </summary>
    public static class Invalid
    {
        private const int MaxLengthDelta = 3;

        /// <summary>
        /// Generates a NIP string with a wrong check digit.
        /// Triggers <see cref="NipValidationError.InvalidChecksum"/>.
        /// </summary>
        /// <returns>A 10-digit string that fails checksum validation.</returns>
        public static string WrongChecksum()
        {
            var chars = NipGenerator.Generate().ToString().ToCharArray();
            chars[9] = (char)('0' + (chars[9] - '0' + 1) % 10);
            return new string(chars);
        }

        /// <summary>
        /// Returns a digit-only NIP string with an invalid length.
        /// Triggers <see cref="NipValidationError.InvalidLength"/>.
        /// </summary>
        /// <returns>A digit-only string whose length differs from a valid NIP by 1 to 3 characters.</returns>
        public static string WrongLength()
        {
            var value = NipGenerator.Generate().ToString();
            var delta = NextInt(MaxLengthDelta) + 1;

            return NextInt(2) == 0
                ? value.Substring(0, value.Length - delta)
                : AppendRandomDigits(value, delta);
        }

        /// <summary>
        /// Generates a NIP string that is 10 characters long but contains a non-digit character.
        /// Triggers <see cref="NipValidationError.InvalidCharacters"/>.
        /// </summary>
        /// <returns>A 10-character string that fails character validation.</returns>
        public static string NonNumeric()
        {
            var chars = NipGenerator.Generate().ToString().ToCharArray();
            chars[5] = 'X';
            return new string(chars);
        }

        /// <summary>
        /// Returns a NIP string that uses only valid NIP characters but does not match
        /// any of the documented public NIP text representations.
        /// Triggers <see cref="NipValidationError.UnrecognizedFormat"/>.
        /// </summary>
        /// <returns>A string that fails format recognition only.</returns>
        public static string UnrecognizedFormat()
        {
            var nip = NipGenerator.Generate().ToString();
            return "PL-" + nip;
        }

        /// <summary>
        /// Generates a sequence of <paramref name="count"/> NIP strings, each with a wrong check digit.
        /// Every element triggers <see cref="NipValidationError.InvalidChecksum"/>.
        /// </summary>
        /// <param name="count">The number of strings to generate. Must be zero or greater.</param>
        /// <returns>A read-only list of <paramref name="count"/> strings, each failing checksum validation.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count"/> is negative.</exception>
        public static IReadOnlyList<string> WrongChecksum(int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), count, "Count must be zero or greater.");
            var result = new List<string>(count);
            for (var i = 0; i < count; i++)
                result.Add(WrongChecksum());
            return result;
        }

        /// <summary>
        /// Generates a sequence of <paramref name="count"/> digit-only NIP strings, each with an invalid length.
        /// Every element triggers <see cref="NipValidationError.InvalidLength"/>.
        /// </summary>
        /// <param name="count">The number of strings to generate. Must be zero or greater.</param>
        /// <returns>A read-only list of <paramref name="count"/> strings, each failing length validation.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count"/> is negative.</exception>
        public static IReadOnlyList<string> WrongLength(int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), count, "Count must be zero or greater.");
            var result = new List<string>(count);
            for (var i = 0; i < count; i++)
                result.Add(WrongLength());
            return result;
        }

        /// <summary>
        /// Generates a sequence of <paramref name="count"/> NIP strings, each containing a non-digit character.
        /// Every element triggers <see cref="NipValidationError.InvalidCharacters"/>.
        /// </summary>
        /// <param name="count">The number of strings to generate. Must be zero or greater.</param>
        /// <returns>A read-only list of <paramref name="count"/> strings, each failing character validation.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count"/> is negative.</exception>
        public static IReadOnlyList<string> NonNumeric(int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), count, "Count must be zero or greater.");
            var result = new List<string>(count);
            for (var i = 0; i < count; i++)
                result.Add(NonNumeric());
            return result;
        }

        /// <summary>
        /// Generates a sequence of <paramref name="count"/> NIP strings, each using only valid NIP characters
        /// but not matching any documented NIP text representation.
        /// Every element triggers <see cref="NipValidationError.UnrecognizedFormat"/>.
        /// </summary>
        /// <param name="count">The number of strings to generate. Must be zero or greater.</param>
        /// <returns>A read-only list of <paramref name="count"/> strings, each failing format recognition only.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count"/> is negative.</exception>
        public static IReadOnlyList<string> UnrecognizedFormat(int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), count, "Count must be zero or greater.");
            var result = new List<string>(count);
            for (var i = 0; i < count; i++)
                result.Add(UnrecognizedFormat());
            return result;
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
