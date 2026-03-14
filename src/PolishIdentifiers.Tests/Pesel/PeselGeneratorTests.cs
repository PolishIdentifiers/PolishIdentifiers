using PolishIdentifiers;
using Shouldly;

namespace PolishIdentifiers.Tests;

public class PeselGeneratorTests
{
    // --- Random() ---

    [Fact]
    public void Random_ReturnsParsablePesel()
    {
        var pesel = PeselGenerator.Random();

        Pesel.Validate(pesel.ToString()).IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Random_CalledMultipleTimes_ReturnsOnlyValidValues()
    {
        var results = Enumerable.Range(0, 100).Select(_ => PeselGenerator.Random().ToString()).ToList();

        results.ShouldAllBe(value => Pesel.Validate(value).IsValid);
    }

    // --- ForBirthDate().Male() / .Female() ---

    [Fact]
    public void ForBirthDate_Male_HasCorrectDate()
    {
        var date = new DateTime(1990, 5, 14);
        var pesel = PeselGenerator.ForBirthDate(date).Male();

        pesel.BirthDateTime.ShouldBe(date);
    }

    [Fact]
    public void ForBirthDate_Male_HasMaleGender()
    {
        var pesel = PeselGenerator.ForBirthDate(new DateTime(1990, 5, 14)).Male();

        pesel.Gender.ShouldBe(Gender.Male);
    }

    [Fact]
    public void ForBirthDate_Female_HasCorrectDate()
    {
        var date = new DateTime(1985, 11, 3);
        var pesel = PeselGenerator.ForBirthDate(date).Female();

        pesel.BirthDateTime.ShouldBe(date);
    }

    [Fact]
    public void ForBirthDate_Female_HasFemaleGender()
    {
        var pesel = PeselGenerator.ForBirthDate(new DateTime(1985, 11, 3)).Female();

        pesel.Gender.ShouldBe(Gender.Female);
    }

    [Fact]
    public void ForBirthDate_ProducesValidPesel()
    {
        var pesel = PeselGenerator.ForBirthDate(new DateTime(2001, 3, 21)).Female();

        Pesel.Validate(pesel.ToString()).IsValid.ShouldBeTrue();
    }

    // --- Century encoding ---

    [Theory]
    [InlineData(1800, 1, 1)]   // 1800s century: PESEL encoded month = 81
    [InlineData(1850, 6, 15)]
    [InlineData(1899, 12, 31)]
    public void ForBirthDate_Century1800_RoundTrips(int year, int month, int day)
    {
        var date  = new DateTime(year, month, day);
        var pesel = PeselGenerator.ForBirthDate(date).Male();

        pesel.BirthDateTime.ShouldBe(date);
    }

    [Theory]
    [InlineData(2000, 1, 1)]   // 2000s century: PESEL encoded month = 21
    [InlineData(2050, 8, 20)]
    [InlineData(2099, 12, 31)]
    public void ForBirthDate_Century2000_RoundTrips(int year, int month, int day)
    {
        var date  = new DateTime(year, month, day);
        var pesel = PeselGenerator.ForBirthDate(date).Female();

        pesel.BirthDateTime.ShouldBe(date);
    }

    [Theory]
    [InlineData(2100, 3, 5)]   // 2100s century: PESEL encoded month = 43
    [InlineData(2200, 7, 1)]   // 2200s century: PESEL encoded month = 67
    public void ForBirthDate_Century2100And2200_RoundTrips(int year, int month, int day)
    {
        var date  = new DateTime(year, month, day);
        var pesel = PeselGenerator.ForBirthDate(date).Male();

        pesel.BirthDateTime.ShouldBe(date);
    }

    [Theory]
    [InlineData(1799, 12, 31)]
    [InlineData(2300, 1, 1)]
    public void ForBirthDate_YearOutOfSupportedRange_ThrowsArgumentOutOfRangeException(int year, int month, int day)
    {
        Should.Throw<ArgumentOutOfRangeException>(() => PeselGenerator.ForBirthDate(new DateTime(year, month, day)));
    }

    [Theory]
    [InlineData(1800, 1, 1)]
    [InlineData(2299, 12, 31)]
    public void ForBirthDate_BoundaryYears_DoNotThrow(int year, int month, int day)
    {
        var exception = Record.Exception(() => PeselGenerator.ForBirthDate(new DateTime(year, month, day)));

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
    public void Invalid_WrongLength_YieldsExactlyInvalidLength()
    {
        var s = PeselGenerator.Invalid.WrongLength();

        Pesel.Validate(s).Error.ShouldBe(PeselValidationError.InvalidLength);
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

    // --- WrongChecksum: structural verification (independent of the validator) ---

    [Fact]
    public void Invalid_WrongChecksum_HasCorrectLengthAndDigits()
    {
        var s = PeselGenerator.Invalid.WrongChecksum();

        s.Length.ShouldBe(11);
        s.ShouldAllBe(c => c >= '0' && c <= '9');
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

    // --- WithGender ---

    [Theory]
    [InlineData(Gender.Male)]
    [InlineData(Gender.Female)]
    public void ForBirthDate_WithGender_HasCorrectGender(Gender gender)
    {
        var date = new DateTime(1990, 5, 14);

        var pesel = PeselGenerator.ForBirthDate(date).WithGender(gender);

        pesel.Gender.ShouldBe(gender);
    }

    [Theory]
    [InlineData(Gender.Male)]
    [InlineData(Gender.Female)]
    public void ForBirthDate_WithGender_HasCorrectDate(Gender gender)
    {
        var date = new DateTime(1990, 5, 14);

        var pesel = PeselGenerator.ForBirthDate(date).WithGender(gender);

        pesel.BirthDateTime.ShouldBe(date);
    }

    [Theory]
    [InlineData(Gender.Male)]
    [InlineData(Gender.Female)]
    public void ForBirthDate_WithGender_ProducesValidPesel(Gender gender)
    {
        var pesel = PeselGenerator.ForBirthDate(new DateTime(2001, 3, 21)).WithGender(gender);

        Pesel.Validate(pesel.ToString()).IsValid.ShouldBeTrue();
    }

    [Fact]
    public void ForBirthDate_WithGender_Male_ProducesSameResultAsMale()
    {
        // WithGender(Male) is semantically equivalent to .Male() — verify both produce valid male PESELs
        var date = new DateTime(1985, 11, 3);

        var pesel = PeselGenerator.ForBirthDate(date).WithGender(Gender.Male);

        pesel.Gender.ShouldBe(Gender.Male);
        pesel.BirthDateTime.ShouldBe(date);
    }

    [Fact]
    public void ForBirthDate_WithGender_Female_ProducesSameResultAsFemale()
    {
        var date = new DateTime(1985, 11, 3);

        var pesel = PeselGenerator.ForBirthDate(date).WithGender(Gender.Female);

        pesel.Gender.ShouldBe(Gender.Female);
        pesel.BirthDateTime.ShouldBe(date);
    }

    // --- ForBirthDate(DateOnly) — net10 only ---

#if NET10_0_OR_GREATER
    [Fact]
    public void ForBirthDate_DateOnly_Male_HasCorrectDate()
    {
        var date = new DateOnly(1990, 5, 14);

        var pesel = PeselGenerator.ForBirthDate(date).Male();

        DateOnly.FromDateTime(pesel.BirthDateTime).ShouldBe(date);
    }

    [Fact]
    public void ForBirthDate_DateOnly_Female_HasCorrectDate()
    {
        var date = new DateOnly(1985, 11, 3);

        var pesel = PeselGenerator.ForBirthDate(date).Female();

        DateOnly.FromDateTime(pesel.BirthDateTime).ShouldBe(date);
    }

    [Fact]
    public void ForBirthDate_DateOnly_WithGender_HasCorrectGenderAndDate()
    {
        var date = new DateOnly(2000, 2, 29);

        var pesel = PeselGenerator.ForBirthDate(date).WithGender(Gender.Male);

        pesel.Gender.ShouldBe(Gender.Male);
        DateOnly.FromDateTime(pesel.BirthDateTime).ShouldBe(date);
    }

    [Fact]
    public void ForBirthDate_DateOnly_ProducesValidPesel()
    {
        var date = new DateOnly(2001, 3, 21);

        var pesel = PeselGenerator.ForBirthDate(date).Female();

        Pesel.Validate(pesel.ToString()).IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData(1799, 12, 31)]
    [InlineData(2300, 1, 1)]
    public void ForBirthDate_DateOnly_YearOutOfRange_ThrowsArgumentOutOfRangeException(int year, int month, int day)
    {
        Should.Throw<ArgumentOutOfRangeException>(() => PeselGenerator.ForBirthDate(new DateOnly(year, month, day)));
    }

    [Fact]
    public void ForBirthDate_DateOnly_IsConsistentWithDateTimeOverload()
    {
        var dateOnly = new DateOnly(1990, 5, 14);
        var dateTime = dateOnly.ToDateTime(TimeOnly.MinValue);

        var fromDateOnly = PeselGenerator.ForBirthDate(dateOnly).Male();
        var fromDateTime = PeselGenerator.ForBirthDate(dateTime).Male();

        DateOnly.FromDateTime(fromDateOnly.BirthDateTime).ShouldBe(DateOnly.FromDateTime(fromDateTime.BirthDateTime));
    }
#endif
}
