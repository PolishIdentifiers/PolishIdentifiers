using PolishIdentifiers;
using Shouldly;

namespace PolishIdentifiers.Tests;

public class PeselGeneratorTests
{
    // --- Generate() ---

    [Fact]
    public void Generate_ReturnsParsablePesel()
    {
        var pesel = PeselGenerator.Generate();

        Pesel.Validate(pesel.ToString()).IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Generate_CalledMultipleTimes_ReturnsOnlyValidValues()
    {
        var results = Enumerable.Range(0, 100).Select(_ => PeselGenerator.Generate().ToString()).ToList();

        results.ShouldAllBe(value => Pesel.Validate(value).IsValid);
    }

    // --- Generate(Gender) ---

    [Fact]
    public void Generate_WithMaleGender_HasMaleGender()
    {
        var pesel = PeselGenerator.Generate(Gender.Male);

        pesel.Gender.ShouldBe(Gender.Male);
    }

    [Fact]
    public void Generate_WithUnsupportedGender_ThrowsArgumentOutOfRangeException()
    {
        var invalidGender = (Gender)999;

        Should.Throw<ArgumentOutOfRangeException>(() => PeselGenerator.Generate(invalidGender));
    }

    [Fact]
    public void Generate_WithFemaleGender_HasFemaleGender()
    {
        var pesel = PeselGenerator.Generate(Gender.Female);

        pesel.Gender.ShouldBe(Gender.Female);
    }

    // --- Generate(DateTime) ---

    [Fact]
    public void Generate_WithBirthDate_HasCorrectDate()
    {
        var date = new DateTime(1990, 5, 14);
        var pesel = PeselGenerator.Generate(date);

        pesel.BirthDate.ShouldBe(date);
    }

    [Fact]
    public void Generate_WithBirthDate_ProducesValidPesel()
    {
        var pesel = PeselGenerator.Generate(new DateTime(2001, 3, 21));

        Pesel.Validate(pesel.ToString()).IsValid.ShouldBeTrue();
    }

    // --- Generate(Gender, DateTime) ---

    [Fact]
    public void Generate_WithMaleGenderAndBirthDate_HasCorrectDate()
    {
        var date = new DateTime(1990, 5, 14);
        var pesel = PeselGenerator.Generate(Gender.Male, date);

        pesel.BirthDate.ShouldBe(date);
    }

    [Fact]
    public void Generate_WithUnsupportedGenderAndBirthDate_ThrowsArgumentOutOfRangeException()
    {
        var invalidGender = (Gender)999;
        var birthDate = new DateTime(1990, 5, 14);

        Should.Throw<ArgumentOutOfRangeException>(() => PeselGenerator.Generate(invalidGender, birthDate));
    }

    [Fact]
    public void Generate_WithMaleGenderAndBirthDate_HasMaleGender()
    {
        var pesel = PeselGenerator.Generate(Gender.Male, new DateTime(1990, 5, 14));

        pesel.Gender.ShouldBe(Gender.Male);
    }

    [Fact]
    public void Generate_WithFemaleGenderAndBirthDate_HasCorrectDate()
    {
        var date = new DateTime(1985, 11, 3);
        var pesel = PeselGenerator.Generate(Gender.Female, date);

        pesel.BirthDate.ShouldBe(date);
    }

    [Fact]
    public void Generate_WithFemaleGenderAndBirthDate_HasFemaleGender()
    {
        var pesel = PeselGenerator.Generate(Gender.Female, new DateTime(1985, 11, 3));

        pesel.Gender.ShouldBe(Gender.Female);
    }

    // --- Century encoding ---

    [Theory]
    [InlineData(1800, 1, 1)]   // 1800s century: PESEL encoded month = 81
    [InlineData(1850, 6, 15)]
    [InlineData(1899, 12, 31)]
    public void Generate_Century1800_RoundTrips(int year, int month, int day)
    {
        var date  = new DateTime(year, month, day);
        var pesel = PeselGenerator.Generate(Gender.Male, date);

        pesel.BirthDate.ShouldBe(date);
    }

    [Theory]
    [InlineData(2000, 1, 1)]   // 2000s century: PESEL encoded month = 21
    [InlineData(2050, 8, 20)]
    [InlineData(2099, 12, 31)]
    public void Generate_Century2000_RoundTrips(int year, int month, int day)
    {
        var date  = new DateTime(year, month, day);
        var pesel = PeselGenerator.Generate(Gender.Female, date);

        pesel.BirthDate.ShouldBe(date);
    }

    [Theory]
    [InlineData(2100, 3, 5)]   // 2100s century: PESEL encoded month = 43
    [InlineData(2200, 7, 1)]   // 2200s century: PESEL encoded month = 67
    public void Generate_Century2100And2200_RoundTrips(int year, int month, int day)
    {
        var date  = new DateTime(year, month, day);
        var pesel = PeselGenerator.Generate(Gender.Male, date);

        pesel.BirthDate.ShouldBe(date);
    }

    [Theory]
    [InlineData(1799, 12, 31)]
    [InlineData(2300, 1, 1)]
    public void Generate_WithBirthDate_YearOutOfRange_ThrowsArgumentOutOfRangeException(int year, int month, int day)
    {
        Should.Throw<ArgumentOutOfRangeException>(() => PeselGenerator.Generate(new DateTime(year, month, day)));
    }

    [Theory]
    [InlineData(1800, 1, 1)]
    [InlineData(2299, 12, 31)]
    public void Generate_WithBirthDate_BoundaryYears_DoNotThrow(int year, int month, int day)
    {
        var exception = Record.Exception(() => PeselGenerator.Generate(new DateTime(year, month, day)));

        exception.ShouldBeNull();
    }

    // --- Invalid generators ---

    [Fact]
    public void Invalid_WrongChecksum_YieldsExactlyInvalidChecksum()
    {
        var s = PeselGenerator.Invalid.WrongChecksum();

        Pesel.Validate(s).Error.ShouldBe(PeselValidationError.InvalidChecksum);
    }

    [Fact]
    public void Invalid_WrongDate_YieldsExactlyInvalidDate()
    {
        var s = PeselGenerator.Invalid.WrongDate();

        Pesel.Validate(s).Error.ShouldBe(PeselValidationError.InvalidDate);
    }

    [Fact]
    public void Invalid_WrongDate_CalledMultipleTimes_AlwaysYieldsInvalidDate()
    {
        var results = Enumerable.Range(0, 50).Select(_ => PeselGenerator.Invalid.WrongDate()).ToList();

        results.ShouldAllBe(s => Pesel.Validate(s).Error == PeselValidationError.InvalidDate);
    }

    [Fact]
    public void Invalid_WrongLength_YieldsExactlyInvalidLength()
    {
        var s = PeselGenerator.Invalid.WrongLength();

        Pesel.Validate(s).Error.ShouldBe(PeselValidationError.InvalidLength);
    }

    [Fact]
    public void Invalid_WrongLength_CalledMultipleTimes_AlwaysYieldsInvalidLength()
    {
        var results = Enumerable.Range(0, 50).Select(_ => PeselGenerator.Invalid.WrongLength()).ToList();

        results.ShouldAllBe(s => Pesel.Validate(s).Error == PeselValidationError.InvalidLength);
    }

    [Fact]
    public void Invalid_WrongLength_HasInvalidLength()
    {
        var value = PeselGenerator.Invalid.WrongLength();

        value.Length.ShouldNotBe(11);
    }

    [Fact]
    public void Invalid_WrongLength_ReturnsOnlyDigits()
    {
        var value = PeselGenerator.Invalid.WrongLength();

        value.ShouldAllBe(c => char.IsDigit(c));
    }

    [Fact]
    public void Invalid_NonNumeric_YieldsExactlyInvalidCharacters()
    {
        var s = PeselGenerator.Invalid.NonNumeric();

        Pesel.Validate(s).Error.ShouldBe(PeselValidationError.InvalidCharacters);
    }

    [Fact]
    public void Invalid_NonNumeric_CalledMultipleTimes_AlwaysYieldsInvalidCharacters()
    {
        var results = Enumerable.Range(0, 50).Select(_ => PeselGenerator.Invalid.NonNumeric()).ToList();

        results.ShouldAllBe(s => Pesel.Validate(s).Error == PeselValidationError.InvalidCharacters);
    }

    // --- WrongChecksum: structural verification (independent of the validator) ---

    [Fact]
    public void Invalid_WrongChecksum_HasCorrectLengthAndDigits()
    {
        var s = PeselGenerator.Invalid.WrongChecksum();

        s.Length.ShouldBe(11);
        s.ShouldAllBe(c => c >= '0' && c <= '9');
    }

    // --- NonNumeric: structural verification ---

    [Fact]
    public void Invalid_NonNumeric_HasLength11()
    {
        var value = PeselGenerator.Invalid.NonNumeric();

        value.Length.ShouldBe(11);
    }

    [Fact]
    public void Invalid_NonNumeric_ContainsAtLeastOneNonDigit()
    {
        var value = PeselGenerator.Invalid.NonNumeric();

        value.Any(c => c < '0' || c > '9').ShouldBeTrue();
    }

    // --- WrongDate: verifies the date is invalid while the checksum is valid ---

    [Fact]
    public void Invalid_WrongDate_HasCorrectChecksumButInvalidMonth()
    {
        var s = PeselGenerator.Invalid.WrongDate();

        // Checksum must be valid (to isolate the InvalidDate failure from InvalidChecksum)
        int[] weights = [1, 3, 7, 9, 1, 3, 7, 9, 1, 3];
        var sum = 0;
        for (var i = 0; i < 10; i++) sum += (s[i] - '0') * weights[i];
        var expectedChecksum = (10 - sum % 10) % 10;
        (s[10] - '0').ShouldBe(expectedChecksum);

        // Encoded month (positions 2-3) must be outside all valid century ranges
        var month = (s[2] - '0') * 10 + (s[3] - '0');
        var isValidRange = (month >= 1 && month <= 12) || (month >= 21 && month <= 32) ||
                           (month >= 41 && month <= 52) || (month >= 61 && month <= 72) ||
                           (month >= 81 && month <= 92);
        isValidRange.ShouldBeFalse();
    }

    // --- Generate(DateOnly) — net10 only ---

#if NET10_0_OR_GREATER
    [Fact]
    public void Generate_WithDateOnly_ProducesValidPesel()
    {
        var date = new DateOnly(2001, 3, 21);
        var pesel = PeselGenerator.Generate(date);

        Pesel.Validate(pesel.ToString()).IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Generate_WithMaleGenderAndDateOnly_HasCorrectDate()
    {
        var date = new DateOnly(1990, 5, 14);

        var pesel = PeselGenerator.Generate(Gender.Male, date);

        DateOnly.FromDateTime(pesel.BirthDate).ShouldBe(date);
    }

    [Fact]
    public void Generate_WithFemaleGenderAndDateOnly_HasCorrectDate()
    {
        var date = new DateOnly(1985, 11, 3);

        var pesel = PeselGenerator.Generate(Gender.Female, date);

        DateOnly.FromDateTime(pesel.BirthDate).ShouldBe(date);
    }

    [Fact]
    public void Generate_WithUnsupportedGenderAndDateOnly_ThrowsArgumentOutOfRangeException()
    {
        var invalidGender = (Gender)999;
        var birthDate = new DateOnly(1990, 5, 14);

        Should.Throw<ArgumentOutOfRangeException>(() => PeselGenerator.Generate(invalidGender, birthDate));
    }

    [Theory]
    [InlineData(1799, 12, 31)]
    [InlineData(2300, 1, 1)]
    public void Generate_WithDateOnly_YearOutOfRange_ThrowsArgumentOutOfRangeException(int year, int month, int day)
    {
        Should.Throw<ArgumentOutOfRangeException>(() => PeselGenerator.Generate(new DateOnly(year, month, day)));
    }

    [Fact]
    public void Generate_DateOnlyAndDateTimeOverloads_ProduceSameBirthDate()
    {
        var dateOnly = new DateOnly(1990, 5, 14);
        var dateTime = dateOnly.ToDateTime(TimeOnly.MinValue);

        var fromDateOnly = PeselGenerator.Generate(Gender.Male, dateOnly);
        var fromDateTime = PeselGenerator.Generate(Gender.Male, dateTime);

        DateOnly.FromDateTime(fromDateOnly.BirthDate).ShouldBe(DateOnly.FromDateTime(fromDateTime.BirthDate));
    }
#endif
}
