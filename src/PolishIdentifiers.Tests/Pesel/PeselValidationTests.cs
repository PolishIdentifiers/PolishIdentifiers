using PolishIdentifiers;
using Shouldly;

namespace PolishIdentifiers.Tests;

public class PeselValidationTests
{
    private const string ValidPesel = "44051401458";
    private const string ValidPeselWithLeadingZero = "02070803628";
    private const string AnotherValidPesel = "90061502867";
    private const string ValidChecksumZeroPesel = "44051401410";

    private const string EmptyPesel = "";
    private const string TooShortPesel = "4405140145";
    private const string TooLongPesel = "440514014589";

    private const string InvalidCharacterAtEndPesel = "4405140145X";
    private const string InvalidCharacterAtStartPesel = "A4051401458";
    private const string InvalidCharacterSpacePesel = "4405 401458";
    private const string InvalidCharacterDotPesel = "44051401.58";

    private const string InvalidEncodedMonth00Pesel = "99001301458";
    private const string InvalidEncodedMonth13Pesel = "99133201458";
    private const string InvalidAllZerosDatePesel = "00000012345";
    private const string InvalidFebruary30Pesel = "01023012345";
    private const string InvalidEncodedMonth95Pesel = "00953212345";
    private const string InvalidDay00Pesel = "44050001234";

    private const string InvalidChecksum0Pesel = "44051401450";
    private const string InvalidChecksum1Pesel = "44051401451";
    private const string InvalidChecksum2Pesel = "44051401452";
    private const string InvalidChecksum3Pesel = "44051401453";
    private const string InvalidChecksum4Pesel = "44051401454";
    private const string InvalidChecksum5Pesel = "44051401455";
    private const string InvalidChecksum6Pesel = "44051401456";
    private const string InvalidChecksum7Pesel = "44051401457";
    private const string InvalidChecksum9Pesel = "44051401459";

    private const string MultipleValidationIssuesPesel = "12X";

    private const string Century1800StartPesel = "00810101234";
    private const string Century1800EndPesel = "99920101234";
    private const string Century1900StartPesel = "00010101234";
    private const string Century1900EndPesel = "99120101234";
    private const string Century2000StartPesel = "00210101234";
    private const string Century2000EndPesel = "99320101234";
    private const string Century2100StartPesel = "00410101234";
    private const string Century2200StartPesel = "00610101234";

    public static TheoryData<string> ValidPeselData => new()
    {
        ValidPesel,
        ValidPeselWithLeadingZero,
        AnotherValidPesel
    };

    public static TheoryData<string?> InvalidLengthData => new()
    {
        null,
        EmptyPesel,
        TooShortPesel,
        TooLongPesel
    };

    public static TheoryData<string> InvalidCharactersData => new()
    {
        InvalidCharacterAtEndPesel,
        InvalidCharacterAtStartPesel,
        InvalidCharacterSpacePesel,
        InvalidCharacterDotPesel
    };

    public static TheoryData<string> InvalidDateData => new()
    {
        InvalidEncodedMonth00Pesel,
        InvalidEncodedMonth13Pesel,
        InvalidAllZerosDatePesel,
        InvalidFebruary30Pesel,
        InvalidEncodedMonth95Pesel,
        InvalidDay00Pesel
    };

    public static TheoryData<string> InvalidChecksumData => new()
    {
        InvalidChecksum0Pesel,
        InvalidChecksum1Pesel,
        InvalidChecksum2Pesel,
        InvalidChecksum3Pesel,
        InvalidChecksum4Pesel,
        InvalidChecksum5Pesel,
        InvalidChecksum6Pesel,
        InvalidChecksum7Pesel,
        InvalidChecksum9Pesel
    };

    public static TheoryData<string> CenturyEncodingData => new()
    {
        Century1800StartPesel,
        Century1800EndPesel,
        Century1900StartPesel,
        Century1900EndPesel,
        Century2000StartPesel,
        Century2000EndPesel,
        Century2100StartPesel,
        Century2200StartPesel
    };

    // --- Happy path ---

    [Theory]
    [MemberData(nameof(ValidPeselData))]
    public void Validate_ValidPesel_ReturnsValid(string pesel)
    {
        var result = Pesel.Validate(pesel);

        result.IsValid.ShouldBeTrue();
        result.Error.ShouldBeNull();
    }

    [Fact]
    public void Validate_CheckDigitZeroPesel_ReturnsValid()
    {
        var result = Pesel.Validate(ValidChecksumZeroPesel);

        result.IsValid.ShouldBeTrue();
    }

    // --- InvalidLength ---

    [Theory]
    [MemberData(nameof(InvalidLengthData))]
    public void Validate_WrongLength_ReturnsInvalidLength(string? pesel)
    {
        var result = Pesel.Validate(pesel);

        result.IsValid.ShouldBeFalse();
        result.Error.ShouldBe(PeselValidationError.InvalidLength);
    }

    // --- InvalidCharacters ---

    [Theory]
    [MemberData(nameof(InvalidCharactersData))]
    public void Validate_NonDigitCharacters_ReturnsInvalidCharacters(string pesel)
    {
        var result = Pesel.Validate(pesel);

        result.IsValid.ShouldBeFalse();
        result.Error.ShouldBe(PeselValidationError.InvalidCharacters);
    }

    // --- InvalidDate ---

    [Theory]
    [MemberData(nameof(InvalidDateData))]
    public void Validate_InvalidDate_ReturnsInvalidDate(string pesel)
    {
        var result = Pesel.Validate(pesel);

        result.IsValid.ShouldBeFalse();
        result.Error.ShouldBe(PeselValidationError.InvalidDate);
    }

    // --- InvalidChecksum ---

    [Theory]
    [MemberData(nameof(InvalidChecksumData))]
    public void Validate_WrongChecksum_ReturnsInvalidChecksum(string pesel)
    {
        // For ValidPesel the correct check digit is 8 — all other digits yield InvalidChecksum.
        var result = Pesel.Validate(pesel);

        result.IsValid.ShouldBeFalse();
        result.Error.ShouldBe(PeselValidationError.InvalidChecksum);
    }

    // --- Validation order: characters → length → date → checksum ---

    [Fact]
    public void Validate_InvalidCharactersTakesPriorityOverInvalidLength()
    {
        var result = Pesel.Validate(MultipleValidationIssuesPesel);

        result.Error.ShouldBe(PeselValidationError.InvalidCharacters);
    }

    // --- Century encoding (full specification 1800–2299) ---

    [Theory]
    [MemberData(nameof(CenturyEncodingData))]
    public void Validate_CenturyEncoding_DateIsNotInvalid(string pesel)
    {
        var result = Pesel.Validate(pesel);

        result.Error.ShouldNotBe(PeselValidationError.InvalidDate);
    }

    // --- Span overload ---

    [Fact]
    public void Validate_SpanOverload_ProducesSameResultAsString()
    {
        Pesel.Validate(ValidPesel.AsSpan()).IsValid.ShouldBeTrue();
        Pesel.Validate(InvalidChecksum7Pesel.AsSpan()).Error.ShouldBe(PeselValidationError.InvalidChecksum);
    }

    // --- Match ---

    [Fact]
    public void Match_ValidPesel_InvokesOnValid()
    {
        var result = Pesel.Validate(ValidPesel);

        var value = result.Match(onValid: () => "ok", onError: e => e.ToString());

        value.ShouldBe("ok");
    }

    [Fact]
    public void Match_InvalidPesel_InvokesOnErrorWithCorrectEnum()
    {
        var result = Pesel.Validate(InvalidChecksum7Pesel);

        var error = result.Match(onValid: () => (PeselValidationError?)null, onError: e => (PeselValidationError?)e);

        error.ShouldBe(PeselValidationError.InvalidChecksum);
    }

    [Fact]
    public void Match_ValidPesel_OnErrorIsNotInvoked()
    {
        var result = Pesel.Validate(ValidPesel);
        var onErrorCalled = false;

        result.Match(onValid: () => true, onError: _ => { onErrorCalled = true; return false; });

        onErrorCalled.ShouldBeFalse();
    }

    [Fact]
    public void Match_InvalidPesel_OnValidIsNotInvoked()
    {
        var result = Pesel.Validate(InvalidChecksum7Pesel);
        var onValidCalled = false;

        result.Match(onValid: () => { onValidCalled = true; return true; }, onError: _ => false);

        onValidCalled.ShouldBeFalse();
    }

    // --- Default struct ---

    [Fact]
    public void Match_DefaultStruct_ThrowsInvalidOperationException()
    {
        var result = default(ValidationResult<PeselValidationError>);

        Should.Throw<InvalidOperationException>(() =>
            result.Match(onValid: () => "ok", onError: e => e.ToString()));
    }

    [Fact]
    public void DefaultStruct_IsValidIsFalse()
    {
        var result = default(ValidationResult<PeselValidationError>);

        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void DefaultStruct_ErrorIsNull()
    {
        var result = default(ValidationResult<PeselValidationError>);

        result.Error.ShouldBeNull();
    }
}
