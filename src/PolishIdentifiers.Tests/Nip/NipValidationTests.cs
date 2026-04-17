using PolishIdentifiers;
using Shouldly;

namespace PolishIdentifiers.Tests;

public class NipValidationTests
{
    // --- Valid NIPs (manually verified) ---
    // 1234563218: 1*6+2*5+3*7+4*2+5*3+6*4+3*5+2*6+1*7 = 118, 118%11=8, d9=8 ✓
    private const string ValidNip = "1234563218";
    // 7680002466: 7*6+6*5+8*7+0*2+0*3+0*4+2*5+4*6+6*7 = 204, 204%11=6, d9=6 ✓
    private const string AnotherValidNip = "7680002466";
    // 0123456789: 0*6+1*5+2*7+3*2+4*3+5*4+6*5+7*6+8*7 = 185, 185%11=9, d9=9 ✓
    private const string ValidNipWithLeadingZero = "0123456789";
    // 5270000001: 5*6+2*5+7*7+0*2+0*3+0*4+0*5+0*6+0*7 = 89, 89%11=1, d9=1 ✓
    private const string ValidNipTaxOffice527 = "5270000001";

    // --- Invalid: wrong characters ---
    private const string InvalidCharacterAtEnd = "123456321X";
    private const string InvalidCharacterAtStart = "A234563218";
    private const string InvalidCharacterDot = "1234563.18";
    private const string InvalidCharacterLowercasePrefix = "pl1234563218";
    private const string InvalidCharacterForeignPrefix = "DE1234563218";

    // --- Invalid: wrong length ---
    private const string EmptyNip = "";
    private const string TooShortNip = "123456321";
    private const string TooLongNip = "12345632180";

    // --- Invalid: wrong checksum ---
    private const string InvalidChecksum0 = "1234563210";
    private const string InvalidChecksum1 = "1234563211";
    private const string InvalidChecksum2 = "1234563212";
    private const string InvalidChecksum3 = "1234563213";
    private const string InvalidChecksum4 = "1234563214";
    private const string InvalidChecksum5 = "1234563215";
    private const string InvalidChecksum6 = "1234563216";
    private const string InvalidChecksum7 = "1234563217";
    private const string InvalidChecksum9 = "1234563219";

    // --- Edge case: checksum mod 11 == 10 ---
    // 1234567890: 1*6+2*5+3*7+4*2+5*3+6*4+7*5+8*6+9*7 = 230, 230%11=10 → InvalidChecksum
    private const string ChecksumMod11Equals10 = "1234567890";

    private const string MultipleIssuesNip = "12X";

    // --- Invalid: unrecognized format ---
    private const string InvalidFormatWrongSeparator = "PL-1234563218";   // dash after PL — not a recognized layout
    private const string InvalidFormatSpaceSeparated = "123 456 32 18";   // spaces instead of hyphens
    private const string InvalidFormatDoubleSpace    = "PL  1234563218";  // two spaces after PL

    public static TheoryData<string> ValidNipData => new()
    {
        ValidNip,
        AnotherValidNip,
        ValidNipWithLeadingZero,
        ValidNipTaxOffice527,
    };

    public static TheoryData<string> InvalidCharactersData => new()
    {
        InvalidCharacterAtEnd,
        InvalidCharacterAtStart,
        InvalidCharacterDot,
        InvalidCharacterLowercasePrefix,
        InvalidCharacterForeignPrefix,
    };

    public static TheoryData<string?> InvalidLengthData => new()
    {
        null,
        EmptyNip,
        TooShortNip,
        TooLongNip,
    };

    public static TheoryData<string> InvalidChecksumData => new()
    {
        InvalidChecksum0,
        InvalidChecksum1,
        InvalidChecksum2,
        InvalidChecksum3,
        InvalidChecksum4,
        InvalidChecksum5,
        InvalidChecksum6,
        InvalidChecksum7,
        InvalidChecksum9,
        ChecksumMod11Equals10,
    };

    public static TheoryData<string> UnrecognizedFormatData => new()
    {
        InvalidFormatWrongSeparator,
        InvalidFormatSpaceSeparated,
        InvalidFormatDoubleSpace,
    };

    // --- Happy path ---

    [Theory]
    [MemberData(nameof(ValidNipData))]
    public void Validate_ValidNip_ReturnsValid(string nip)
    {
        var result = Nip.Validate(nip);

        result.IsValid.ShouldBeTrue();
        result.Error.ShouldBeNull();
    }

    // --- InvalidCharacters ---

    [Theory]
    [MemberData(nameof(InvalidCharactersData))]
    public void Validate_NonDigitCharacters_ReturnsInvalidCharacters(string nip)
    {
        var result = Nip.Validate(nip);

        result.IsValid.ShouldBeFalse();
        result.Error.ShouldBe(NipValidationError.InvalidCharacters);
    }

    // --- InvalidLength ---

    [Theory]
    [MemberData(nameof(InvalidLengthData))]
    public void Validate_WrongLength_ReturnsInvalidLength(string? nip)
    {
        var result = Nip.Validate(nip);

        result.IsValid.ShouldBeFalse();
        result.Error.ShouldBe(NipValidationError.InvalidLength);
    }

    // --- InvalidChecksum ---

    [Theory]
    [MemberData(nameof(InvalidChecksumData))]
    public void Validate_WrongChecksum_ReturnsInvalidChecksum(string nip)
    {
        var result = Nip.Validate(nip);

        result.IsValid.ShouldBeFalse();
        result.Error.ShouldBe(NipValidationError.InvalidChecksum);
    }

    // --- UnrecognizedFormat ---

    [Theory]
    [MemberData(nameof(UnrecognizedFormatData))]
    public void Validate_UnrecognizedFormat_ReturnsUnrecognizedFormat(string nip)
    {
        var result = Nip.Validate(nip);

        result.IsValid.ShouldBeFalse();
        result.Error.ShouldBe(NipValidationError.UnrecognizedFormat);
    }

    // --- Checksum mod 11 == 10 edge case ---

    [Fact]
    public void Validate_ChecksumMod11Equals10_ReturnsInvalidChecksum()
    {
        var result = Nip.Validate(ChecksumMod11Equals10);

        result.IsValid.ShouldBeFalse();
        result.Error.ShouldBe(NipValidationError.InvalidChecksum);
    }

    // --- Validation order: characters → length → format → checksum ---

    [Fact]
    public void Validate_InvalidCharactersTakesPriorityOverInvalidLength()
    {
        var result = Nip.Validate(MultipleIssuesNip);

        result.Error.ShouldBe(NipValidationError.InvalidCharacters);
    }

    // --- Span overload ---

    [Fact]
    public void Validate_SpanOverload_ValidNip_ReturnsValid()
    {
        Nip.Validate(ValidNip.AsSpan()).IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_SpanOverload_InvalidChecksum_ReturnsInvalidChecksum()
    {
        Nip.Validate(InvalidChecksum7.AsSpan()).Error.ShouldBe(NipValidationError.InvalidChecksum);
    }

    // --- Match ---

    [Fact]
    public void Match_ValidNip_InvokesOnValid()
    {
        var result = Nip.Validate(ValidNip);

        var value = result.Match(onValid: () => "ok", onError: e => e.ToString());

        value.ShouldBe("ok");
    }

    [Fact]
    public void Match_InvalidNip_InvokesOnErrorWithCorrectEnum()
    {
        var result = Nip.Validate(InvalidChecksum7);

        var error = result.Match(onValid: () => (NipValidationError?)null, onError: e => (NipValidationError?)e);

        error.ShouldBe(NipValidationError.InvalidChecksum);
    }

    [Fact]
    public void Match_ValidNip_OnErrorIsNotInvoked()
    {
        var result = Nip.Validate(ValidNip);
        var onErrorCalled = false;

        result.Match(onValid: () => true, onError: _ => { onErrorCalled = true; return false; });

        onErrorCalled.ShouldBeFalse();
    }

    [Fact]
    public void Match_InvalidNip_OnValidIsNotInvoked()
    {
        var result = Nip.Validate(InvalidChecksum7);
        var onValidCalled = false;

        result.Match(onValid: () => { onValidCalled = true; return true; }, onError: _ => false);

        onValidCalled.ShouldBeFalse();
    }

    // --- Default struct ---

    [Fact]
    public void Match_DefaultStruct_ThrowsInvalidOperationException()
    {
        var result = default(ValidationResult<NipValidationError>);

        Should.Throw<InvalidOperationException>(() =>
            result.Match(onValid: () => "ok", onError: e => e.ToString()));
    }

    [Fact]
    public void DefaultStruct_IsValidIsFalse()
    {
        var result = default(ValidationResult<NipValidationError>);

        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void DefaultStruct_ErrorIsNull()
    {
        var result = default(ValidationResult<NipValidationError>);

        result.Error.ShouldBeNull();
    }

}
