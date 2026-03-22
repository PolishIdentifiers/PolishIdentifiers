namespace PolishIdentifiers;

/// <summary>
/// Provides methods for generating valid and deliberately invalid REGON numbers,
/// primarily intended for use in tests and tooling.
/// </summary>
public static class RegonGenerator
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

    /// <summary>Generates a valid REGON of the specified kind.</summary>
    /// <param name="kind">The kind of REGON to generate: <see cref="RegonKind.Regon9"/> (9 digits) or <see cref="RegonKind.Regon14"/> (14 digits).</param>
    /// <returns>A valid <see cref="Regon"/> instance with the requested <see cref="Regon.Kind"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="kind"/> is not a supported <see cref="RegonKind"/> value.</exception>
    /// <remarks>This method is thread-safe.</remarks>
    public static Regon Generate(RegonKind kind)
        => kind switch
        {
            RegonKind.Regon9 => GenerateRegon9(),
            RegonKind.Regon14 => GenerateRegon14(),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unsupported REGON kind.")
        };

    private static Regon GenerateRegon9() => GenerateRegon9Core(out _);

    private static Regon GenerateRegon9Core(out int[] digits)
    {
        digits = new int[9];

        for (var i = 0; i < 8; i++)
            digits[i] = NextDigit();

        var sum = 0;
        for (var i = 0; i < 8; i++)
            sum += digits[i] * RegonChecksumWeights.Weights9[i];

        var r = sum % 11;
        digits[8] = r == 10 ? 0 : r;

        ulong value = 0;
        foreach (var d in digits)
            value = value * 10 + (ulong)d;

        return new Regon(value, isRegon14: false);
    }

    private static Regon GenerateRegon14()
    {
        GenerateRegon9Core(out var base9Digits);

        var digits = new int[14];
        Array.Copy(base9Digits, digits, 9);

        for (var i = 9; i < 13; i++)
            digits[i] = NextDigit();

        var sum = 0;
        for (var i = 0; i < 13; i++)
            sum += digits[i] * RegonChecksumWeights.Weights14[i];

        var r = sum % 11;
        digits[13] = r == 10 ? 0 : r;

        ulong value = 0;
        foreach (var d in digits)
            value = value * 10 + (ulong)d;

        return new Regon(value, isRegon14: true);
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
        /// <returns>A 9-digit REGON string that fails checksum validation.</returns>
        public static string WrongChecksumRegon9()
        {
            var chars = RegonGenerator.GenerateRegon9().ToString().ToCharArray();
            chars[8] = (char)('0' + (chars[8] - '0' + 1) % 10);
            return new string(chars);
        }

        /// <summary>
        /// Generates a REGON-14 string with a valid embedded REGON-9 base but a wrong final check digit.
        /// Triggers <see cref="RegonValidationError.InvalidChecksum"/>.
        /// </summary>
        /// <returns>
        /// A 14-digit REGON string whose embedded REGON-9 base (first 9 digits) is valid,
        /// but the REGON-14 check digit (position 13) is wrong.
        /// </returns>
        public static string WrongChecksumRegon14()
        {
            var chars = RegonGenerator.GenerateRegon14().ToString().ToCharArray();
            chars[13] = (char)('0' + (chars[13] - '0' + 1) % 10);
            return new string(chars);
        }

        /// <summary>
        /// Returns a digit-only REGON string with an invalid length (neither 9 nor 14 digits).
        /// Triggers <see cref="RegonValidationError.InvalidLength"/>.
        /// </summary>
        /// <returns>A digit-only string whose length is between 6–8 or 10–12 characters.</returns>
        public static string WrongLength()
        {
            var value = RegonGenerator.GenerateRegon9().ToString(); // 9 digits
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
            var chars = RegonGenerator.GenerateRegon9().ToString().ToCharArray();
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
