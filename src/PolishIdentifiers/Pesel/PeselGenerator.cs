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

    // --- Valid generators ---

    /// <summary>Generates a valid PESEL with a random birth date and gender.</summary>
    /// <returns>A valid <see cref="Pesel"/> instance.</returns>
    /// <remarks>This method is thread-safe.</remarks>
    public static Pesel Generate()
    {
        var date   = RandomDate();
        var gender = NextInt(2) == 0 ? Gender.Male : Gender.Female;
        return BuildPesel(date, gender);
    }

    /// <summary>Generates a valid PESEL with a random birth date and the specified gender.</summary>
    /// <param name="gender">The gender to encode in the generated PESEL.</param>
    /// <returns>A valid <see cref="Pesel"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="gender"/> is not a supported <see cref="Gender"/> value.</exception>
    /// <remarks>This method is thread-safe.</remarks>
    public static Pesel Generate(Gender gender)
        => BuildPesel(RandomDate(), gender);

    /// <summary>
    /// Generates a valid PESEL for a person born on <paramref name="birthDate"/>, with a random gender.
    /// Only the date part (year, month, day) is used; any time component is ignored.
    /// </summary>
    /// <param name="birthDate">The birth date to encode. Only the date part is used.</param>
    /// <returns>A valid <see cref="Pesel"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the year is outside the PESEL-supported range of 1800–2299.
    /// </exception>
    public static Pesel Generate(DateTime birthDate)
    {
        ValidateBirthYear(birthDate.Year, nameof(birthDate));
        var gender = NextInt(2) == 0 ? Gender.Male : Gender.Female;
        return BuildPesel(birthDate, gender);
    }

    /// <summary>
    /// Generates a valid PESEL for a person born on <paramref name="birthDate"/> with the specified gender.
    /// Only the date part (year, month, day) is used; any time component is ignored.
    /// </summary>
    /// <param name="gender">The gender to encode in the generated PESEL.</param>
    /// <param name="birthDate">The birth date to encode. Only the date part is used.</param>
    /// <returns>A valid <see cref="Pesel"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the year is outside the PESEL-supported range of 1800–2299.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="gender"/> is not a supported <see cref="Gender"/> value.</exception>
    public static Pesel Generate(Gender gender, DateTime birthDate)
    {
        ValidateBirthYear(birthDate.Year, nameof(birthDate));
        return BuildPesel(birthDate, gender);
    }

#if NET10_0_OR_GREATER
    /// <summary>
    /// Generates a valid PESEL for a person born on <paramref name="birthDate"/>, with a random gender.
    /// </summary>
    /// <param name="birthDate">The birth date to encode.</param>
    /// <returns>A valid <see cref="Pesel"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the year is outside the PESEL-supported range of 1800–2299.
    /// </exception>
    public static Pesel Generate(DateOnly birthDate)
    {
        ValidateBirthYear(birthDate.Year, nameof(birthDate));
        var gender = NextInt(2) == 0 ? Gender.Male : Gender.Female;
        return BuildPesel(birthDate.ToDateTime(TimeOnly.MinValue), gender);
    }

    /// <summary>
    /// Generates a valid PESEL for a person born on <paramref name="birthDate"/> with the specified gender.
    /// </summary>
    /// <param name="gender">The gender to encode in the generated PESEL.</param>
    /// <param name="birthDate">The birth date to encode.</param>
    /// <returns>A valid <see cref="Pesel"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the year is outside the PESEL-supported range of 1800–2299.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="gender"/> is not a supported <see cref="Gender"/> value.</exception>
    public static Pesel Generate(Gender gender, DateOnly birthDate)
    {
        ValidateBirthYear(birthDate.Year, nameof(birthDate));
        return BuildPesel(birthDate.ToDateTime(TimeOnly.MinValue), gender);
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
        private const int MaxLengthDelta = 3;

        /// <summary>
        /// Generates a PESEL string with a valid birth date but a wrong check digit.
        /// Triggers <see cref="PeselValidationError.InvalidChecksum"/>.
        /// </summary>
        /// <returns>An 11-digit string that fails checksum validation.</returns>
        public static string WrongChecksum()
        {
            var chars = PeselGenerator.Generate().ToString().ToCharArray();
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
        /// Returns a digit-only PESEL string with an invalid length.
        /// Triggers <see cref="PeselValidationError.InvalidLength"/>.
        /// </summary>
        /// <returns>A digit-only string whose length differs from a valid PESEL by 1 to 3 characters.</returns>
        public static string WrongLength()
        {
            var value = PeselGenerator.Generate().ToString();
            var delta = NextInt(MaxLengthDelta) + 1;

            return NextInt(2) == 0
                ? value.Substring(0, value.Length - delta)
                : AppendRandomDigits(value, delta);
        }

        /// <summary>
        /// Generates a PESEL string that is 11 characters long but contains a non-digit character.
        /// Triggers <see cref="PeselValidationError.InvalidCharacters"/>.
        /// </summary>
        /// <returns>An 11-character string that fails character validation.</returns>
        public static string NonNumeric()
        {
            var chars = PeselGenerator.Generate().ToString().ToCharArray();
            chars[5] = 'X';
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

    // --- Internal helpers ---

    private static void ValidateBirthYear(int year, string paramName)
    {
        if (year is < 1800 or > 2299)
            throw new ArgumentOutOfRangeException(paramName, "PESEL supports birth years in range 1800-2299.");
    }

    private static Pesel BuildPesel(DateTime date, Gender gender)
    {
        ValidateGender(gender, nameof(gender));

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

    private static void ValidateGender(Gender gender, string paramName)
    {
        if (gender is not Gender.Female and not Gender.Male)
            throw new ArgumentOutOfRangeException(paramName, gender, "Unsupported gender value.");
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
            sum += digits[i] * PeselChecksumWeights.Weights[i];
        return (10 - sum % 10) % 10;
    }
}
