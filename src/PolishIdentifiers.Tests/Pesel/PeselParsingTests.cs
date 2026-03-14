using System.Globalization;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using PolishIdentifiers;
using Shouldly;

namespace PolishIdentifiers.Tests;

public class PeselParsingTests
{
    private const string ValidPesel = "44051401458";
    private const string AnotherValidPesel = "90061502867";
    private const string ValidPeselWithLeadingZero = "02070803628";
    private const string InvalidChecksumPesel = "44051401457";

    private const string TooShortPesel = "4405140145";
    private const string TooLongPesel = "440514014580";
    private const string VeryLongNumericPesel = "440514014584405140145844051401458";
    private const string InvalidCharactersPesel = "4405140145A";
    private const string InvalidDatePesel = "99223112345";
    private const string EmptyPesel = "";
    private const string LeadingWhitespacePesel = " 44051401458";
    private const string TrailingWhitespacePesel = "44051401458 ";
    private const string MiddleTabPesel = "4405140\t458";
    private const string MultipleValidationIssuesPesel = "12X";

    private const string InvalidLeap1900Pesel = "00022912345";
    private const string InvalidLeap2100Pesel = "00422912345";

    private const string InvalidEncodedMonth20Pesel = "00200112345";
    private const string InvalidEncodedMonth33Pesel = "00330112345";
    private const string InvalidEncodedMonth40Pesel = "00400112345";
    private const string InvalidEncodedMonth53Pesel = "00530112345";
    private const string InvalidEncodedMonth60Pesel = "00600112345";
    private const string InvalidEncodedMonth73Pesel = "00730112345";
    private const string InvalidEncodedMonth80Pesel = "00800112345";
    private const string InvalidEncodedMonth93Pesel = "00930112345";

    private const string InvalidApril31Pesel = "00043112345";
    private const string InvalidJune31Pesel = "00063112345";
    private const string InvalidSeptember31Pesel = "00093112345";

    private const string Eighteen00sPesel = "00810100019";
    private const string Twenty00sPesel = "00210100028";
    private const string Twenty100sPesel = "00410100017";
    private const string Twenty200sPesel = "00610100013";

    private const string MalePesel = "44051401434";
    private const string FemalePesel = "44051401403";

    // Gender digit coverage — every remaining odd/even digit explicitly tested
    // Base "440514014" → partial sum 87; checksum = (10 - (87 + digit*3) % 10) % 10
    private const string MaleGenderDigit1Pesel   = "44051401410"; // digit 1, sum 90
    private const string MaleGenderDigit7Pesel   = "44051401472"; // digit 7, sum 110
    private const string MaleGenderDigit9Pesel   = "44051401496"; // digit 9, sum 120
    private const string FemaleGenderDigit2Pesel = "44051401427"; // digit 2, sum 100
    private const string FemaleGenderDigit4Pesel = "44051401441"; // digit 4, sum 100
    private const string FemaleGenderDigit8Pesel = "44051401489"; // digit 8, sum 120

    // Feb 29 in non-Gregorian-leap centuries (divisible by 100 but not 400)
    private const string InvalidLeap1800Pesel = "00822912345"; // 1800-02-29; encodedMonth 82 → 1800s
    private const string InvalidLeap2200Pesel = "00622912345"; // 2200-02-29; encodedMonth 62 → 2200s

    // All-same-digit strings: valid characters and length, but encodedMonth falls outside every valid range
    private const string AllZerosPesel = "00000000000"; // encodedMonth 00 → InvalidDate
    private const string AllNinesPesel = "99999999999"; // encodedMonth 99 → InvalidDate

    public static TheoryData<string, PeselValidationError> InvalidInputData => new()
    {
        { TooShortPesel, PeselValidationError.InvalidLength },
        { TooLongPesel, PeselValidationError.InvalidLength },
        { InvalidCharactersPesel, PeselValidationError.InvalidCharacters },
        { InvalidDatePesel, PeselValidationError.InvalidDate }
    };

    public static TheoryData<string> InvalidInputStringsData => new()
    {
        TooShortPesel,
        TooLongPesel,
        InvalidCharactersPesel,
        InvalidDatePesel
    };

    public static TheoryData<string, PeselValidationError> WhitespaceInputData => new()
    {
        { LeadingWhitespacePesel, PeselValidationError.InvalidCharacters },
        { TrailingWhitespacePesel, PeselValidationError.InvalidCharacters },
        { MiddleTabPesel, PeselValidationError.InvalidCharacters }
    };

    public static TheoryData<string> WhitespaceInputStringsData => new()
    {
        LeadingWhitespacePesel,
        TrailingWhitespacePesel,
        MiddleTabPesel
    };

    public static TheoryData<string> InvalidEncodedMonthData => new()
    {
        InvalidEncodedMonth20Pesel,
        InvalidEncodedMonth33Pesel,
        InvalidEncodedMonth40Pesel,
        InvalidEncodedMonth53Pesel,
        InvalidEncodedMonth60Pesel,
        InvalidEncodedMonth73Pesel,
        InvalidEncodedMonth80Pesel,
        InvalidEncodedMonth93Pesel
    };

    public static TheoryData<string> Invalid31stDayFor30DayMonthData => new()
    {
        InvalidApril31Pesel,
        InvalidJune31Pesel,
        InvalidSeptember31Pesel
    };

    public static TheoryData<string, int, int, int> BirthDateTimeData => new()
    {
        { ValidPesel, 1944, 5, 14 },
        { ValidPeselWithLeadingZero, 1902, 7, 8 },
        { Eighteen00sPesel, 1800, 1, 1 },
        { Twenty00sPesel, 2000, 1, 1 },
        { Twenty100sPesel, 2100, 1, 1 },
        { Twenty200sPesel, 2200, 1, 1 }
    };

    public static TheoryData<string, Gender> GenderData => new()
    {
        { ValidPesel,               Gender.Male   },  // digit 5
        { MalePesel,                Gender.Male   },  // digit 3
        { MaleGenderDigit1Pesel,    Gender.Male   },  // digit 1
        { MaleGenderDigit7Pesel,    Gender.Male   },  // digit 7
        { MaleGenderDigit9Pesel,    Gender.Male   },  // digit 9
        { AnotherValidPesel,        Gender.Female },  // digit 6
        { FemalePesel,              Gender.Female },  // digit 0
        { FemaleGenderDigit2Pesel,  Gender.Female },  // digit 2
        { FemaleGenderDigit4Pesel,  Gender.Female },  // digit 4
        { FemaleGenderDigit8Pesel,  Gender.Female },  // digit 8
    };

    // --- Parse (throws on invalid input) ---

    [Fact]
    public void Parse_ValidPesel_ReturnsPeselStruct()
    {
        var input = ValidPesel;
        var pesel = Pesel.Parse(input);

        pesel.ToString().ShouldBe(input);
    }

    [Fact]
    public void Parse_PeselWithLeadingZero_ReturnsCanonicalDigits()
    {
        var input = ValidPeselWithLeadingZero;

        var pesel = Pesel.Parse(input);

        pesel.ToString().ShouldBe(input);
    }

    [Fact]
    public void Parse_InvalidPesel_ThrowsPeselValidationException()
    {
        var ex = Should.Throw<PeselValidationException>(() => Pesel.Parse(InvalidChecksumPesel));

        ex.Error.ShouldBe(PeselValidationError.InvalidChecksum);
    }

    [Theory]
    [MemberData(nameof(InvalidInputData))]
    public void Parse_InvalidInput_ThrowsPeselValidationExceptionWithExpectedError(string input, PeselValidationError expectedError)
    {
        var ex = Should.Throw<PeselValidationException>(() => Pesel.Parse(input));

        ex.Error.ShouldBe(expectedError);
    }

    [Fact]
    public void Parse_NullString_ThrowsArgumentNullException()
    {
        var ex = Should.Throw<ArgumentNullException>(() => Pesel.Parse((string)null!));

        ex.ParamName.ShouldBe("value");
    }

    [Fact]
    public void Parse_EmptyString_ThrowsPeselValidationExceptionWithInvalidLength()
    {
        var ex = Should.Throw<PeselValidationException>(() => Pesel.Parse(EmptyPesel));

        ex.Error.ShouldBe(PeselValidationError.InvalidLength);
    }

    [Fact]
    public void Parse_VeryLongNumericInput_ThrowsPeselValidationExceptionWithInvalidLength()
    {
        var ex = Should.Throw<PeselValidationException>(() => Pesel.Parse(VeryLongNumericPesel));

        ex.Error.ShouldBe(PeselValidationError.InvalidLength);
    }

    [Theory]
    [MemberData(nameof(WhitespaceInputData))]
    public void Parse_WhitespaceInput_ThrowsPeselValidationExceptionWithExpectedError(string input, PeselValidationError expectedError)
    {
        var ex = Should.Throw<PeselValidationException>(() => Pesel.Parse(input));

        ex.Error.ShouldBe(expectedError);
    }

    [Fact]
    public void Parse_InputWithMultipleValidationIssues_ThrowsFirstExpectedValidationError()
    {
        var ex = Should.Throw<PeselValidationException>(() => Pesel.Parse(MultipleValidationIssuesPesel));

        ex.Error.ShouldBe(PeselValidationError.InvalidCharacters);
    }

    // --- TryParse ---

    [Fact]
    public void TryParse_ValidPesel_ReturnsTrue()
    {
        Pesel.TryParse(ValidPesel, out _).ShouldBeTrue();
    }

    [Fact]
    public void TryParse_ValidPesel_SetsOutParam()
    {
        Pesel.TryParse(ValidPesel, out var pesel);

        pesel.ToString().ShouldBe(ValidPesel);
    }

    [Fact]
    public void TryParse_InvalidChecksumPesel_ReturnsFalse()
    {
        Pesel.TryParse(InvalidChecksumPesel, out _).ShouldBeFalse();
    }

    [Fact]
    public void TryParse_InvalidChecksumPesel_SetsOutParamToDefault()
    {
        Pesel.TryParse(InvalidChecksumPesel, out var pesel);

        pesel.ShouldBe(default);
    }

    [Theory]
    [MemberData(nameof(InvalidInputStringsData))]
    public void TryParse_InvalidInput_ReturnsFalse(string input)
    {
        Pesel.TryParse(input, out _).ShouldBeFalse();
    }

    [Theory]
    [MemberData(nameof(InvalidInputStringsData))]
    public void TryParse_InvalidInput_SetsOutParamToDefault(string input)
    {
        Pesel.TryParse(input, out var pesel);

        pesel.ShouldBe(default);
    }

    [Fact]
    public void TryParse_Null_ReturnsFalse()
    {
        Pesel.TryParse(null, out _).ShouldBeFalse();
    }

    [Fact]
    public void TryParse_Null_SetsOutParamToDefault()
    {
        Pesel.TryParse(null, out var pesel);

        pesel.ShouldBe(default);
    }

    [Fact]
    public void TryParse_VeryLongNumericInput_ReturnsFalse()
    {
        Pesel.TryParse(VeryLongNumericPesel, out _).ShouldBeFalse();
    }

    [Theory]
    [MemberData(nameof(WhitespaceInputStringsData))]
    public void TryParse_WhitespaceInput_ReturnsFalse(string input)
    {
        Pesel.TryParse(input, out _).ShouldBeFalse();
    }

    [Theory]
    [MemberData(nameof(WhitespaceInputStringsData))]
    public void TryParse_WhitespaceInput_SetsOutParamToDefault(string input)
    {
        Pesel.TryParse(input, out var pesel);

        pesel.ShouldBe(default);
    }

    // --- Span overloads ---

    [Fact]
    public void Parse_SpanOverload_ValidPesel_ReturnsPesel()
    {
        var input = ValidPesel;
        var pesel = Pesel.Parse(input.AsSpan());

        pesel.ToString().ShouldBe(input);
    }

    [Fact]
    public void Parse_SpanOverload_InvalidPesel_ThrowsPeselValidationException()
    {
        var ex = Should.Throw<PeselValidationException>(() => Pesel.Parse(InvalidChecksumPesel.AsSpan()));

        ex.Error.ShouldBe(PeselValidationError.InvalidChecksum);
    }

    [Fact]
    public void TryParse_SpanOverload_ValidPesel_ReturnsTrue()
    {
        Pesel.TryParse(ValidPesel.AsSpan(), out _).ShouldBeTrue();
    }

    [Fact]
    public void TryParse_SpanOverload_ValidPesel_SetsOutParam()
    {
        Pesel.TryParse(ValidPesel.AsSpan(), out var pesel);

        pesel.ToString().ShouldBe(ValidPesel);
    }

    [Fact]
    public void TryParse_SpanOverload_InvalidPesel_ReturnsFalse()
    {
        Pesel.TryParse(InvalidChecksumPesel.AsSpan(), out _).ShouldBeFalse();
    }

    [Fact]
    public void TryParse_SpanOverload_InvalidPesel_SetsOutParamToDefault()
    {
        Pesel.TryParse(InvalidChecksumPesel.AsSpan(), out var pesel);

        pesel.ShouldBe(default);
    }

    [Fact]
    public void TryParse_SpanOverload_EmptySpan_ReturnsFalse()
    {
        Pesel.TryParse(ReadOnlySpan<char>.Empty, out _).ShouldBeFalse();
    }

    [Fact]
    public void TryParse_SpanOverload_EmptySpan_SetsOutParamToDefault()
    {
        Pesel.TryParse(ReadOnlySpan<char>.Empty, out var pesel);

        pesel.ShouldBe(default);
    }

    [Theory]
    [MemberData(nameof(InvalidInputStringsData))]
    public void TryParse_SpanOverload_InvalidInput_ReturnsFalse(string input)
    {
        Pesel.TryParse(input.AsSpan(), out _).ShouldBeFalse();
    }

    [Theory]
    [MemberData(nameof(InvalidInputStringsData))]
    public void TryParse_SpanOverload_InvalidInput_SetsOutParamToDefault(string input)
    {
        Pesel.TryParse(input.AsSpan(), out var pesel);

        pesel.ShouldBe(default);
    }

    [Theory]
    [MemberData(nameof(InvalidInputData))]
    public void Parse_SpanOverload_InvalidInput_ThrowsPeselValidationExceptionWithExpectedError(string input, PeselValidationError expectedError)
    {
        var ex = Should.Throw<PeselValidationException>(() => Pesel.Parse(input.AsSpan()));

        ex.Error.ShouldBe(expectedError);
    }

    [Fact]
    public void Parse_SpanOverload_EmptySpan_ThrowsWithInvalidLength()
    {
        var ex = Should.Throw<PeselValidationException>(() => Pesel.Parse(ReadOnlySpan<char>.Empty));

        ex.Error.ShouldBe(PeselValidationError.InvalidLength);
    }

    // --- BirthDateTime ---

    [Theory]
    [MemberData(nameof(BirthDateTimeData))]
    public void BirthDateTime_ReturnsCorrectDate(string input, int year, int month, int day)
    {
        var pesel = Pesel.Parse(input);

        pesel.BirthDateTime.ShouldBe(new DateTime(year, month, day));
    }

    [Theory]
    [InlineData(2000, 2, 29)]
    [InlineData(2004, 2, 29)]
    public void BirthDateTime_LeapDayInLeapYear_ReturnsCorrectDate(int year, int month, int day)
    {
        var generated = PeselGenerator.ForBirthDate(new DateTime(year, month, day)).Male();
        var pesel = Pesel.Parse(generated.ToString());

        pesel.BirthDateTime.ShouldBe(new DateTime(year, month, day));
    }

    [Theory]
    [InlineData(InvalidLeap1800Pesel)]
    [InlineData(InvalidLeap1900Pesel)]
    [InlineData(InvalidLeap2100Pesel)]
    [InlineData(InvalidLeap2200Pesel)]
    public void Parse_LeapDayInNonLeapYear_ThrowsPeselValidationExceptionWithInvalidDate(string input)
    {
        var ex = Should.Throw<PeselValidationException>(() => Pesel.Parse(input));

        ex.Error.ShouldBe(PeselValidationError.InvalidDate);
    }

    [Theory]
    [MemberData(nameof(InvalidEncodedMonthData))]
    public void Parse_InvalidEncodedMonth_ThrowsPeselValidationExceptionWithInvalidDate(string input)
    {
        var ex = Should.Throw<PeselValidationException>(() => Pesel.Parse(input));

        ex.Error.ShouldBe(PeselValidationError.InvalidDate);
    }

    [Theory]
    [MemberData(nameof(Invalid31stDayFor30DayMonthData))]
    public void Parse_ThirtyFirstDayInThirtyDayMonth_ThrowsPeselValidationExceptionWithInvalidDate(string input)
    {
        var ex = Should.Throw<PeselValidationException>(() => Pesel.Parse(input));

        ex.Error.ShouldBe(PeselValidationError.InvalidDate);
    }

    [Theory]
    [InlineData(1804, 2, 29)]
    [InlineData(2104, 2, 29)]
    public void BirthDateTime_LeapDayInNonStandardCenturyLeapYear_ReturnsCorrectDate(int year, int month, int day)
    {
        var generated = PeselGenerator.ForBirthDate(new DateTime(year, month, day)).Male();
        var pesel = Pesel.Parse(generated.ToString());

        pesel.BirthDateTime.ShouldBe(new DateTime(year, month, day));
    }

    [Theory]
    [InlineData(1899, 12, 31)]
    [InlineData(1999, 12, 31)]
    [InlineData(2099, 12, 31)]
    [InlineData(2199, 12, 31)]
    [InlineData(2299, 12, 31)]
    public void BirthDateTime_LastDayOfCenturyRange_ReturnsCorrectDate(int year, int month, int day)
    {
        var generated = PeselGenerator.ForBirthDate(new DateTime(year, month, day)).Male();
        var pesel = Pesel.Parse(generated.ToString());

        pesel.BirthDateTime.ShouldBe(new DateTime(year, month, day));
    }

    [Fact]
    public void BirthDateTime_Kind_IsUnspecified()
    {
        var pesel = Pesel.Parse(ValidPesel);

        pesel.BirthDateTime.Kind.ShouldBe(DateTimeKind.Unspecified);
    }

    [Fact]
    public void BirthDateTime_TimeOfDay_IsZero()
    {
        var pesel = Pesel.Parse(ValidPesel);

        pesel.BirthDateTime.TimeOfDay.ShouldBe(TimeSpan.Zero);
    }

    // --- Gender ---

    [Theory]
    [MemberData(nameof(GenderData))]
    public void Gender_ReturnsCorrectGender(string input, Gender expected)
    {
        var pesel = Pesel.Parse(input);

        pesel.Gender.ShouldBe(expected);
    }

    // --- Default struct ---

    [Fact]
    public void DefaultPesel_BirthDateTime_ThrowsInvalidOperationException()
    {
        var p = default(Pesel);

        Should.Throw<InvalidOperationException>(() => { var _ = p.BirthDateTime; });
    }

    [Fact]
    public void DefaultPesel_Gender_ThrowsInvalidOperationException()
    {
        var p = default(Pesel);

        Should.Throw<InvalidOperationException>(() => { var _ = p.Gender; });
    }

    [Fact]
    public void DefaultPesel_ToString_ThrowsInvalidOperationException()
    {
        var p = default(Pesel);

        Should.Throw<InvalidOperationException>(() => p.ToString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("G")]
    [InlineData("g")]
    [InlineData("D11")]
    [InlineData("d11")]
    public void DefaultPesel_ToString_WithSupportedFormat_ThrowsInvalidOperationException(string? format)
    {
        var p = default(Pesel);

        Should.Throw<InvalidOperationException>(() => p.ToString(format, null));
    }

    [Fact]
    public void DefaultPesel_IsDefault_ReturnsTrue()
    {
        var p = default(Pesel);

        p.IsDefault.ShouldBeTrue();
    }

    [Fact]
    public void ParsedPesel_IsDefault_ReturnsFalse()
    {
        var pesel = Pesel.Parse(ValidPesel);

        pesel.IsDefault.ShouldBeFalse();
    }

    [Fact]
    public void TryParse_InvalidPesel_OutParam_IsDefault_ReturnsTrue()
    {
        Pesel.TryParse(InvalidChecksumPesel, out var pesel);

        pesel.IsDefault.ShouldBeTrue();
    }

    // --- Equality ---

    [Fact]
    public void Equals_SamePeselValue_ReturnsTrue()
    {
        var a = Pesel.Parse(ValidPesel);
        var b = Pesel.Parse(ValidPesel);

        a.ShouldBe(b);
    }

    [Fact]
    public void EqualityOperator_SamePeselValue_ReturnsTrue()
    {
        var a = Pesel.Parse(ValidPesel);
        var b = Pesel.Parse(ValidPesel);

        (a == b).ShouldBeTrue();
    }

    [Fact]
    public void InequalityOperator_SamePeselValue_ReturnsFalse()
    {
        var a = Pesel.Parse(ValidPesel);
        var b = Pesel.Parse(ValidPesel);

        (a != b).ShouldBeFalse();
    }

    [Fact]
    public void Equals_DifferentPeselValues_ReturnsFalse()
    {
        var a = Pesel.Parse(ValidPesel);
        var b = Pesel.Parse(AnotherValidPesel);

        a.ShouldNotBe(b);
    }

    [Fact]
    public void EqualityOperator_DifferentPeselValues_ReturnsFalse()
    {
        var a = Pesel.Parse(ValidPesel);
        var b = Pesel.Parse(AnotherValidPesel);

        (a == b).ShouldBeFalse();
    }

    [Fact]
    public void InequalityOperator_DifferentPeselValues_ReturnsTrue()
    {
        var a = Pesel.Parse(ValidPesel);
        var b = Pesel.Parse(AnotherValidPesel);

        (a != b).ShouldBeTrue();
    }

    [Fact]
    public void Equals_BoxedSamePesel_ReturnsTrue()
    {
        var pesel = Pesel.Parse(ValidPesel);

        pesel.Equals((object)pesel).ShouldBeTrue();
    }

    [Fact]
    public void Equals_BoxedNonPeselObject_ReturnsFalse()
    {
        var pesel = Pesel.Parse(ValidPesel);

        pesel.Equals((object)"not a pesel").ShouldBeFalse();
    }

    [Fact]
    public void GetHashCode_EqualPesels_HaveSameHashCode()
    {
        var a = Pesel.Parse(ValidPesel);
        var b = Pesel.Parse(ValidPesel);

        a.GetHashCode().ShouldBe(b.GetHashCode());
    }

    [Fact]
    public void EqualityOperator_TwoDefaults_ReturnsTrue()
    {
        var a = default(Pesel);
        var b = default(Pesel);

        (a == b).ShouldBeTrue();
    }

    [Fact]
    public void InequalityOperator_DefaultAndValidPesel_ReturnsTrue()
    {
        var valid = Pesel.Parse(ValidPesel);
        var def   = default(Pesel);

        (valid != def).ShouldBeTrue();
    }

    [Fact]
    public void Equals_NullObject_ReturnsFalse()
    {
        var pesel = Pesel.Parse(ValidPesel);

        pesel.Equals((object?)null).ShouldBeFalse();
    }

    [Fact]
    public void HashSet_EqualPesels_CountsAsOneEntry()
    {
        var a = Pesel.Parse(ValidPesel);
        var b = Pesel.Parse(ValidPesel);

        var set = new HashSet<Pesel> { a, b };

        set.Count.ShouldBe(1);
    }

    [Fact]
    public void HashSet_DifferentPesels_CountsAsTwoEntries()
    {
        var a = Pesel.Parse(ValidPesel);
        var b = Pesel.Parse(AnotherValidPesel);

        var set = new HashSet<Pesel> { a, b };

        set.Count.ShouldBe(2);
    }

    [Fact]
    public void Dictionary_PeselAsKey_RetrievesValueCorrectly()
    {
        var pesel = Pesel.Parse(ValidPesel);
        var dict  = new Dictionary<Pesel, string> { [pesel] = "Jan Kowalski" };

        dict[Pesel.Parse(ValidPesel)].ShouldBe("Jan Kowalski");
    }

    // --- IComparable ---

    [Fact]
    public void CompareTo_SmallerPesel_ReturnsNegative()
    {
        var smaller = Pesel.Parse(ValidPesel);
        var larger  = Pesel.Parse(AnotherValidPesel);

        (smaller.CompareTo(larger) < 0).ShouldBeTrue();
    }

    [Fact]
    public void CompareTo_EqualPesel_ReturnsZero()
    {
        var a = Pesel.Parse(ValidPesel);
        var b = Pesel.Parse(ValidPesel);

        a.CompareTo(b).ShouldBe(0);
    }

    [Fact]
    public void CompareTo_LargerPesel_ReturnsPositive()
    {
        var smaller = Pesel.Parse(ValidPesel);
        var larger  = Pesel.Parse(AnotherValidPesel);

        (larger.CompareTo(smaller) > 0).ShouldBeTrue();
    }

    [Fact]
    public void CompareTo_IsAntisymmetric()
    {
        var a = Pesel.Parse(ValidPesel);
        var b = Pesel.Parse(AnotherValidPesel);

        a.CompareTo(b).ShouldBe(-b.CompareTo(a));
    }

    [Fact]
    public void CompareTo_DefaultPesel_ReturnsPositiveForValidPesel()
    {
        var pesel = Pesel.Parse(ValidPesel);
        var defaultPesel = default(Pesel);

        (pesel.CompareTo(defaultPesel) > 0).ShouldBeTrue();
    }

    [Fact]
    public void CompareTo_ValidPesel_ReturnsNegativeForDefaultPesel()
    {
        var pesel        = Pesel.Parse(ValidPesel);
        var defaultPesel = default(Pesel);

        (defaultPesel.CompareTo(pesel) < 0).ShouldBeTrue();
    }

    [Fact]
    public void CompareTo_TwoDefaults_ReturnsZero()
    {
        var a = default(Pesel);
        var b = default(Pesel);

        a.CompareTo(b).ShouldBe(0);
    }

    [Fact]
    public void CompareTo_IsTransitive()
    {
        var a = Pesel.Parse(ValidPeselWithLeadingZero); // 02070803628 — smallest
        var b = Pesel.Parse(ValidPesel);                // 44051401458 — middle
        var c = Pesel.Parse(AnotherValidPesel);         // 90061502867 — largest

        (a.CompareTo(b) < 0).ShouldBeTrue();
        (b.CompareTo(c) < 0).ShouldBeTrue();
        (a.CompareTo(c) < 0).ShouldBeTrue();
    }

    [Fact]
    public void Sort_ListOfPesels_ProducesAscendingOrder()
    {
        var a = Pesel.Parse(ValidPeselWithLeadingZero); // 02070803628
        var b = Pesel.Parse(ValidPesel);                // 44051401458
        var c = Pesel.Parse(AnotherValidPesel);         // 90061502867

        var list = new List<Pesel> { c, b, a };
        list.Sort();

        list.ShouldBe([a, b, c]);
    }

    // --- ToString ---

    [Fact]
    public void ToString_PreservesLeadingZeros()
    {
        var input = ValidPeselWithLeadingZero;
        var pesel = Pesel.Parse(input);

        pesel.ToString().ShouldBe(input);
    }

    [Fact]
    public void Parse_ToString_Parse_RoundtripPreservesValue()
    {
        var parsed = Pesel.Parse(ValidPeselWithLeadingZero);
        var serialized = parsed.ToString();
        var reparsed = Pesel.Parse(serialized);

        reparsed.ShouldBe(parsed);
    }

    // --- IFormattable ---

    [Theory]
    [InlineData(null)]
    [InlineData("G")]
    [InlineData("g")]
    [InlineData("D11")]
    [InlineData("d11")]
    public void ToString_SupportedFormat_ReturnsCanonicalPesel(string? format)
    {
        var input = ValidPeselWithLeadingZero;
        var pesel = Pesel.Parse(input);

        pesel.ToString(format, null).ShouldBe(input);
    }

    [Fact]
    public void ToString_SupportedFormat_WithCulture_ReturnsCanonicalPesel()
    {
        var input = ValidPeselWithLeadingZero;
        var pesel = Pesel.Parse(input);

        pesel.ToString("D11", CultureInfo.GetCultureInfo("pl-PL")).ShouldBe(input);
    }

    [Theory]
    [InlineData("pl-PL")]
    [InlineData("en-US")]
    [InlineData("fr-FR")]
    public void ToString_SupportedFormat_WithMultipleCultures_ReturnsCanonicalPesel(string cultureName)
    {
        var input = ValidPeselWithLeadingZero;
        var pesel = Pesel.Parse(input);

        pesel.ToString("D11", CultureInfo.GetCultureInfo(cultureName)).ShouldBe(input);
    }

    [Theory]
    [InlineData("X")]
    [InlineData("F")]
    [InlineData("N")]
    [InlineData("E")]
    [InlineData("D")]
    [InlineData("d")]
    [InlineData("D10")]
    [InlineData("G11")]
    public void ToString_UnsupportedFormat_ThrowsFormatException(string format)
    {
        var pesel = Pesel.Parse(ValidPeselWithLeadingZero);

        var ex = Should.Throw<FormatException>(() => pesel.ToString(format, null));

        ex.Message.ShouldContain(format);
    }

    // --- PeselValidationException ---

    [Fact]
    public void PeselValidationException_Message_ContainsErrorName()
    {
        var ex = Should.Throw<PeselValidationException>(() => Pesel.Parse(InvalidChecksumPesel));

        ex.Message.ShouldContain(nameof(PeselValidationError.InvalidChecksum));
    }

    [Fact]
    public void PeselValidationException_InnerException_IsNull()
    {
        var ex = Should.Throw<PeselValidationException>(() => Pesel.Parse(InvalidChecksumPesel));

        ex.InnerException.ShouldBeNull();
    }

    // --- Validate API ---

    [Fact]
    public void Validate_Null_ReturnsInvalidLength_UnlikeParseWhichThrowsArgumentNullException()
    {
        var result = Pesel.Validate(null);

        result.IsValid.ShouldBeFalse();
        result.Error.ShouldBe(PeselValidationError.InvalidLength);
    }

    [Fact]
    public void Validate_ValidPesel_IsConsistentWithTryParse()
    {
        var canParse = Pesel.TryParse(ValidPesel, out _);
        var result   = Pesel.Validate(ValidPesel);

        result.IsValid.ShouldBe(canParse);
    }

    [Fact]
    public void Validate_InvalidPesel_IsConsistentWithTryParse()
    {
        var canParse = Pesel.TryParse(InvalidChecksumPesel, out _);
        var result   = Pesel.Validate(InvalidChecksumPesel);

        result.IsValid.ShouldBe(canParse);
    }

    // --- Pathological values ---

    [Theory]
    [InlineData(AllZerosPesel)]
    [InlineData(AllNinesPesel)]
    public void Parse_AllSameDigits_ThrowsPeselValidationExceptionWithInvalidDate(string input)
    {
        var ex = Should.Throw<PeselValidationException>(() => Pesel.Parse(input));

        ex.Error.ShouldBe(PeselValidationError.InvalidDate);
    }

    // --- Struct semantics ---

    [Fact]
    public void StructAssignment_ProducesEqualCopy()
    {
        var original = Pesel.Parse(ValidPesel);
        var copy     = original;

        copy.ShouldBe(original);
    }

    [Fact]
    public void TryParse_MixedDataset_EachInputProducesExpectedOutcome()
    {
        var validA = PeselGenerator.ForBirthDate(new DateTime(1990, 6, 15)).Female().ToString();
        var validB = PeselGenerator.ForBirthDate(new DateTime(2000, 2, 29)).Male().ToString();
        var validC = PeselGenerator.ForBirthDate(new DateTime(2200, 1, 1)).Female().ToString();

        Pesel.TryParse(validA, out _).ShouldBeTrue();
        Pesel.TryParse(validB, out _).ShouldBeTrue();
        Pesel.TryParse(validC, out _).ShouldBeTrue();
        Pesel.TryParse(InvalidChecksumPesel, out _).ShouldBeFalse();
        Pesel.TryParse(TooShortPesel, out _).ShouldBeFalse();
        Pesel.TryParse(InvalidCharactersPesel, out _).ShouldBeFalse();
        Pesel.TryParse(InvalidDatePesel, out _).ShouldBeFalse();
        Pesel.TryParse(LeadingWhitespacePesel, out _).ShouldBeFalse();
    }

    [Fact]
    public void Parse_ParallelLoad_ProducesNoFailures()
    {
        var inputs = new[]
        {
            ValidPesel,
            AnotherValidPesel,
            ValidPeselWithLeadingZero,
            Eighteen00sPesel,
            Twenty00sPesel,
            Twenty100sPesel,
            Twenty200sPesel
        };

        var failures = new ConcurrentBag<Exception>();

        Parallel.For(0, 500, i =>
        {
            try
            {
                var input = inputs[i % inputs.Length];
                var parsed = Pesel.Parse(input);
                _ = parsed.ToString();
                _ = parsed.BirthDateTime;
                _ = parsed.Gender;
            }
            catch (Exception ex)
            {
                failures.Add(ex);
            }
        });

        failures.ShouldBeEmpty();
    }

    // --- net10 only ---

#if NET10_0_OR_GREATER
    [Fact]
    public void BirthDateOnly_ReturnsCorrectDate()
    {
        var pesel = Pesel.Parse(ValidPesel);

        pesel.BirthDateOnly.ShouldBe(new DateOnly(1944, 5, 14));
    }

    [Fact]
    public void BirthDateOnly_IsConsistentWithBirthDateTime()
    {
        var pesel = Pesel.Parse(ValidPesel);

        pesel.BirthDateOnly.ShouldBe(DateOnly.FromDateTime(pesel.BirthDateTime));
    }

    [Fact]
    public void DefaultPesel_BirthDateOnly_ThrowsInvalidOperationException()
    {
        var p = default(Pesel);

        Should.Throw<InvalidOperationException>(() => { var _ = p.BirthDateOnly; });
    }

    [Fact]
    public void IParsable_Parse_ValidPesel_ReturnsPesel()
    {
        static T CallParse<T>(string s) where T : IParsable<T> => T.Parse(s, null);

        var pesel = CallParse<Pesel>(ValidPesel);

        pesel.ToString().ShouldBe(ValidPesel);
    }

    [Fact]
    public void IParsable_TryParse_ValidPesel_ReturnsTrue()
    {
        static bool CallTryParse<T>(string? s, out T result) where T : struct, IParsable<T>
            => T.TryParse(s, null, out result);

        CallTryParse<Pesel>(ValidPesel, out _).ShouldBeTrue();
    }

    [Fact]
    public void IParsable_Parse_InvalidPesel_ThrowsPeselValidationException()
    {
        static T CallParse<T>(string s) where T : IParsable<T> => T.Parse(s, null);

        Should.Throw<PeselValidationException>(() => CallParse<Pesel>(InvalidChecksumPesel));
    }

    [Fact]
    public void IParsable_Parse_Null_ThrowsArgumentNullException()
    {
        static T CallParse<T>(string? s) where T : IParsable<T> => T.Parse(s!, null);

        Should.Throw<ArgumentNullException>(() => CallParse<Pesel>(null));
    }

    [Fact]
    public void IParsable_Parse_WithProvider_ReturnsCanonicalPesel()
    {
        static T CallParse<T>(string s, IFormatProvider? provider) where T : IParsable<T>
            => T.Parse(s, provider);

        var pesel = CallParse<Pesel>(ValidPeselWithLeadingZero, CultureInfo.GetCultureInfo("pl-PL"));

        pesel.ToString().ShouldBe(ValidPeselWithLeadingZero);
    }

    [Fact]
    public void IParsable_TryParse_InvalidPesel_ReturnsFalse()
    {
        static bool CallTryParse<T>(string? s, out T result) where T : struct, IParsable<T>
            => T.TryParse(s, null, out result);

        CallTryParse<Pesel>(InvalidChecksumPesel, out _).ShouldBeFalse();
    }

    [Fact]
    public void IParsable_TryParse_InvalidPesel_SetsOutParamToDefault()
    {
        static bool CallTryParse<T>(string? s, out T result) where T : struct, IParsable<T>
            => T.TryParse(s, null, out result);

        CallTryParse<Pesel>(InvalidChecksumPesel, out var pesel);

        pesel.ShouldBe(default);
    }

    [Fact]
    public void ISpanParsable_Parse_ValidPesel_ReturnsPesel()
    {
        static T CallParse<T>(ReadOnlySpan<char> s) where T : ISpanParsable<T> => T.Parse(s, null);

        var pesel = CallParse<Pesel>(ValidPesel.AsSpan());

        pesel.ToString().ShouldBe(ValidPesel);
    }

    [Fact]
    public void ISpanParsable_TryParse_ValidPesel_ReturnsTrue()
    {
        static bool CallTryParse<T>(ReadOnlySpan<char> s, out T result) where T : struct, ISpanParsable<T>
            => T.TryParse(s, null, out result);

        CallTryParse<Pesel>(ValidPesel.AsSpan(), out _).ShouldBeTrue();
    }

    [Fact]
    public void ISpanParsable_Parse_InvalidPesel_ThrowsPeselValidationException()
    {
        static T CallParse<T>(ReadOnlySpan<char> s) where T : ISpanParsable<T> => T.Parse(s, null);

        Should.Throw<PeselValidationException>(() => CallParse<Pesel>(InvalidChecksumPesel.AsSpan()));
    }

    [Fact]
    public void ISpanParsable_Parse_EmptySpan_ThrowsPeselValidationExceptionWithInvalidLength()
    {
        static T CallParse<T>(ReadOnlySpan<char> s) where T : ISpanParsable<T> => T.Parse(s, null);

        var ex = Should.Throw<PeselValidationException>(() => CallParse<Pesel>(ReadOnlySpan<char>.Empty));

        ex.Error.ShouldBe(PeselValidationError.InvalidLength);
    }

    [Fact]
    public void ISpanParsable_TryParse_InvalidPesel_ReturnsFalse()
    {
        static bool CallTryParse<T>(ReadOnlySpan<char> s, out T result) where T : struct, ISpanParsable<T>
            => T.TryParse(s, null, out result);

        CallTryParse<Pesel>(InvalidChecksumPesel.AsSpan(), out _).ShouldBeFalse();
    }

    [Fact]
    public void ISpanParsable_TryParse_InvalidPesel_SetsOutParamToDefault()
    {
        static bool CallTryParse<T>(ReadOnlySpan<char> s, out T result) where T : struct, ISpanParsable<T>
            => T.TryParse(s, null, out result);

        CallTryParse<Pesel>(InvalidChecksumPesel.AsSpan(), out var pesel);

        pesel.ShouldBe(default);
    }

    [Fact]
    public void IParsable_TryParse_Null_ReturnsFalse()
    {
        static bool CallTryParse<T>(string? s, out T result) where T : struct, IParsable<T>
            => T.TryParse(s, null, out result);

        CallTryParse<Pesel>(null, out _).ShouldBeFalse();
    }

    [Fact]
    public void IParsable_TryParse_Null_SetsOutParamToDefault()
    {
        static bool CallTryParse<T>(string? s, out T result) where T : struct, IParsable<T>
            => T.TryParse(s, null, out result);

        CallTryParse<Pesel>(null, out var pesel);

        pesel.ShouldBe(default);
    }

    [Fact]
    public void ISpanParsable_TryParse_EmptySpan_ReturnsFalse()
    {
        static bool CallTryParse<T>(ReadOnlySpan<char> s, out T result) where T : struct, ISpanParsable<T>
            => T.TryParse(s, null, out result);

        CallTryParse<Pesel>(ReadOnlySpan<char>.Empty, out _).ShouldBeFalse();
    }

    [Fact]
    public void ISpanParsable_TryParse_EmptySpan_SetsOutParamToDefault()
    {
        static bool CallTryParse<T>(ReadOnlySpan<char> s, out T result) where T : struct, ISpanParsable<T>
            => T.TryParse(s, null, out result);

        CallTryParse<Pesel>(ReadOnlySpan<char>.Empty, out var pesel);

        pesel.ShouldBe(default);
    }
#endif
}

[Collection(CultureSensitiveCollection.Name)]
public class PeselCultureInvarianceTests
{
    private const string ValidPesel = "44051401458";
    private const string ValidPeselWithLeadingZero = "02070803628";

    [Fact]
    public void ToString_OutputContainsOnlyAsciiDigits()
    {
        var pesel = Pesel.Parse(ValidPeselWithLeadingZero);

        pesel.ToString().ShouldAllBe(c => c >= '0' && c <= '9');
    }

    [Theory]
    [InlineData("ar-SA")]
    [InlineData("fa-IR")]
    public void ToString_WithNativeDigitFormatProvider_OutputContainsOnlyAsciiDigits(string cultureName)
    {
        var pesel = Pesel.Parse(ValidPeselWithLeadingZero);
        var culture = CultureInfo.GetCultureInfo(cultureName);

        pesel.ToString("G", culture).ShouldAllBe(c => c >= '0' && c <= '9');
    }

    [Theory]
    [InlineData("ar-SA")]
    [InlineData("fa-IR")]
    public void ToString_FormatProviderIsIgnored_MatchesParameterlessToString(string cultureName)
    {
        var pesel = Pesel.Parse(ValidPeselWithLeadingZero);
        var expected = pesel.ToString();
        var culture = CultureInfo.GetCultureInfo(cultureName);

        pesel.ToString(null, culture).ShouldBe(expected);
        pesel.ToString("G", culture).ShouldBe(expected);
        pesel.ToString("D11", culture).ShouldBe(expected);
    }

    [Theory]
    [InlineData("ar-SA")]
    [InlineData("fa-IR")]
    public void ToString_WhenCurrentCultureHasNativeDigits_OutputIsRoundtrippable(string cultureName)
    {
        var original = Pesel.Parse(ValidPeselWithLeadingZero);
        var savedCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);

            var serialized = original.ToString();
            Pesel.TryParse(serialized, out var reparsed).ShouldBeTrue();
            reparsed.ShouldBe(original);
        }
        finally
        {
            CultureInfo.CurrentCulture = savedCulture;
        }
    }

    [Theory]
    [InlineData("ar-SA")]
    [InlineData("fa-IR")]
    public void BirthDateTime_WhenCurrentCultureHasNativeDigits_ReturnsCorrectDate(string cultureName)
    {
        var pesel = Pesel.Parse(ValidPesel);
        var savedCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);

            pesel.BirthDateTime.ShouldBe(new DateTime(1944, 5, 14));
        }
        finally
        {
            CultureInfo.CurrentCulture = savedCulture;
        }
    }
}

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class CultureSensitiveCollection
{
    public const string Name = "Culture-sensitive tests";
}
