namespace PolishIdentifiers;

/// <summary>
/// Provides methods for generating valid and deliberately invalid PESEL numbers,
/// primarily intended for use in tests and tooling.
/// </summary>
public static class PeselGenerator
{
#if NET10_0_OR_GREATER
    private static int NextDigit() => System.Random.Shared.Next(10);
    private static int NextInt(int maxValue) => System.Random.Shared.Next(maxValue);
    private static int NextInt(int minValue, int maxValue) => System.Random.Shared.Next(minValue, maxValue);
#else
    private static readonly ThreadLocal<Random> Rng = new(() => new Random(Guid.NewGuid().GetHashCode()));

    private static Random CurrentRng
        => Rng.Value ?? throw new InvalidOperationException("Random generator is not available.");

    private static int NextDigit() => CurrentRng.Next(10);
    private static int NextInt(int maxValue) => CurrentRng.Next(maxValue);
    private static int NextInt(int minValue, int maxValue) => CurrentRng.Next(minValue, maxValue);
#endif

    private static readonly int[] ChecksumWeights = [1, 3, 7, 9, 1, 3, 7, 9, 1, 3];

    // --- Valid generators ---

    /// <summary>Generates a random valid PESEL with a random birth date and gender.</summary>
    /// <returns>A valid <see cref="Pesel"/> instance.</returns>
    /// <remarks>This method is thread-safe.</remarks>
    public static Pesel Random()
    {
        var date   = RandomDate();
        var gender = NextInt(2) == 0 ? Gender.Male : Gender.Female;
        return BuildPesel(date, gender);
    }

    /// <summary>
    /// Returns a <see cref="PeselDateBuilder"/> for generating a valid PESEL
    /// for a person born on <paramref name="date"/>.
    /// Only the date part (year, month, day) is used; any time component is ignored.
    /// </summary>
    /// <param name="date">The birth date to encode. Only the date part is used.</param>
    /// <returns>A <see cref="PeselDateBuilder"/> for choosing the gender and building the PESEL.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the year is outside the PESEL-supported range of 1800–2299.
    /// </exception>
    public static PeselDateBuilder ForBirthDate(DateTime date)
    {
        if (date.Year is < 1800 or > 2299)
            throw new ArgumentOutOfRangeException(nameof(date), "PESEL supports birth years in range 1800-2299.");

        return new PeselDateBuilder(date.Date);
    }

#if NET10_0_OR_GREATER
    /// <summary>
    /// Returns a <see cref="PeselDateBuilder"/> for generating a valid PESEL
    /// for a person born on <paramref name="date"/>.
    /// </summary>
    /// <param name="date">The birth date to encode.</param>
    /// <returns>A <see cref="PeselDateBuilder"/> for choosing the gender and building the PESEL.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the year is outside the PESEL-supported range of 1800–2299.
    /// </exception>
    public static PeselDateBuilder ForBirthDate(DateOnly date)
    {
        if (date.Year is < 1800 or > 2299)
            throw new ArgumentOutOfRangeException(nameof(date), "PESEL supports birth years in range 1800-2299.");

        return new PeselDateBuilder(date.ToDateTime(TimeOnly.MinValue));
    }
#endif

    // --- Invalid generators (return string — Pesel.Parse would throw) ---

    /// <summary>
    /// Provides methods for generating deliberately invalid PESEL strings.
    /// Each method violates exactly one validation rule while keeping all others correct,
    /// enabling precise unit testing of individual error codes.
    /// </summary>
    public static class Invalid
    {
        /// <summary>
        /// Generates a PESEL string with a valid birth date but a wrong check digit.
        /// Triggers <see cref="PeselValidationError.InvalidChecksum"/>.
        /// </summary>
        /// <returns>An 11-digit string that fails checksum validation.</returns>
        public static string WrongChecksum()
        {
            var chars = PeselGenerator.Random().ToString().ToCharArray();
            chars[10] = (char)('0' + (chars[10] - '0' + 1) % 10);
            return new string(chars);
        }

        /// <summary>
        /// Generates a PESEL string with an impossible date (encoded month 13, outside every
        /// century-encoding range) but a correct check digit.
        /// Triggers <see cref="PeselValidationError.InvalidDate"/>.
        /// </summary>
        /// <returns>An 11-digit string that fails date validation.</returns>
        public static string WrongDate()
        {
            var digits = new int[11];
            digits[0] = 0; digits[1] = 0;  // yy = 00
            digits[2] = 1; digits[3] = 3;  // encodedMonth = 13 → outside every valid range
            digits[4] = 0; digits[5] = 1;  // day = 01
            digits[6] = NextDigit();
            digits[7] = NextDigit();
            digits[8] = NextDigit();
            digits[9] = NextDigit();
            digits[10] = ComputeChecksum(digits);

            var chars = new char[11];
            for (var i = 0; i < 11; i++) chars[i] = (char)('0' + digits[i]);
            return new string(chars);
        }

        /// <summary>
        /// Returns a fixed 10-digit string that is one character too short.
        /// Triggers <see cref="PeselValidationError.InvalidLength"/>.
        /// </summary>
        /// <returns>A 10-character string that fails length validation.</returns>
        public static string WrongLength() => "4405140145";

        /// <summary>
        /// Generates a PESEL string that is 11 characters long but contains a non-digit character.
        /// Triggers <see cref="PeselValidationError.InvalidCharacters"/>.
        /// </summary>
        /// <returns>An 11-character string that fails character validation.</returns>
        public static string NonNumeric()
        {
            var chars = PeselGenerator.Random().ToString().ToCharArray();
            chars[5] = 'X';
            return new string(chars);
        }
    }

    // --- Internal helpers (used by PeselDateBuilder) ---

    internal static Pesel BuildPesel(DateTime date, Gender gender)
    {
        var yy           = date.Year % 100;
        var encodedMonth = EncodeMonth(date.Year, date.Month);
        var day          = date.Day;

        var digits = new int[11];
        digits[0] = yy / 10;
        digits[1] = yy % 10;
        digits[2] = encodedMonth / 10;
        digits[3] = encodedMonth % 10;
        digits[4] = day / 10;
        digits[5] = day % 10;
        digits[6] = NextDigit();
        digits[7] = NextDigit();
        digits[8] = NextDigit();
        digits[9] = gender == Gender.Male
            ? NextInt(5) * 2 + 1   // 1, 3, 5, 7, 9
            : NextInt(5) * 2;      // 0, 2, 4, 6, 8
        digits[10] = ComputeChecksum(digits);

        ulong value = 0;
        foreach (var d in digits)
            value = value * 10 + (ulong)d;

        return new Pesel(value);
    }

    private static DateTime RandomDate()
    {
        var year  = NextInt(1800, 2300);
        var month = NextInt(1, 13);
        var day   = NextInt(1, DateTime.DaysInMonth(year, month) + 1);
        return new DateTime(year, month, day);
    }

    private static int EncodeMonth(int year, int month)
    {
        if      (year <= 1899) return month + 80;
        else if (year <= 1999) return month;
        else if (year <= 2099) return month + 20;
        else if (year <= 2199) return month + 40;
        else                   return month + 60;
    }

    private static int ComputeChecksum(int[] digits)
    {
        var sum = 0;
        for (var i = 0; i < 10; i++)
            sum += digits[i] * ChecksumWeights[i];
        return (10 - sum % 10) % 10;
    }
}
