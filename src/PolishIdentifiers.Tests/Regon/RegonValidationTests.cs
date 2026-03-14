using PolishIdentifiers;
using Shouldly;

namespace PolishIdentifiers.Tests;

public class RegonValidationTests
{
    // --- Valid REGONs (manually verified) ---
    // "123456785": weights 8,9,2,3,4,5,6,7 → sum=192, 192%11=5, d8=5 ✓
    private const string ValidRegon9 = "123456785";
    // "012345675": sum=148, 148%11=5, d8=5 ✓
    private const string ValidRegon9WithLeadingZero = "012345675";
    // "491996453": sum=267, 267%11=3, d8=3 ✓
    private const string AnotherValidRegon9 = "491996453";
    // "12345678512347": base "123456785" valid; weights 2,4,8,5,0,9,7,3,6,1,2,4,8 → sum=260, 260%11=7, d13=7 ✓
    private const string ValidRegon14 = "12345678512347";
    // "01234567512342": base "012345675" valid; sum=222, 222%11=2, d13=2 ✓
    private const string ValidRegon14WithLeadingZeroBase = "01234567512342";
    // "12345678500010": base "123456785" valid; sum=219, 219%11=10, d13=0 ✓
    private const string ValidRegon14WithChecksumR10 = "12345678500010";

    // Edge case: r==10 → checkDigit=0 (NOT InvalidChecksum)
    // "000000030": 0*8+0*9+0*2+0*3+0*4+0*5+0*6+3*7=21, 21%11=10, checkDigit=0, d8=0 ✓
    private const string ValidRegon9WithChecksumR10 = "000000030";
    // "000000000": sum=0, 0%11=0, d8=0 ✓ — _value=0 is a valid REGON, distinct from default(Regon)
    private const string ValidRegon9AllZeros = "000000000";
    // "00000000000000": base "000000000" valid; sum=0, 0%11=0, d13=0 ✓
    private const string ValidRegon14AllZeros = "00000000000000";

    // --- Invalid: wrong characters ---
    private const string InvalidCharacterAtEnd = "12345678X";
    private const string InvalidCharacterAtStart = "X23456785";
    private const string InvalidCharacterSpace = "12345678 ";
    private const string InvalidCharacterLeadingSpace = " 123456785";
    private const string InvalidCharacterTrailingSpace = "123456785 ";
    private const string InvalidCharacterDash = "123-56785";
    private const string InvalidCharacterInRegon14 = "1234567851234X";

    // --- Invalid: wrong length ---
    private const string EmptyRegon = "";
    private const string TooShortRegon = "12345678";        // 8 digits
    private const string RegonLength10 = "1234567850";      // 10 digits — between valid lengths
    private const string RegonLength13 = "1234567851234";   // 13 digits
    private const string TooLongRegon = "123456785123456";  // 15 digits

    // --- Invalid: wrong checksum REGON-9 ---
    // "123456785" is valid (d8=5); change d8 to any other digit
    private const string InvalidChecksum9_d8is0 = "123456780";
    private const string InvalidChecksum9_d8is1 = "123456781";
    private const string InvalidChecksum9_d8is9 = "123456789";

    // --- Invalid: wrong checksum REGON-14 (valid base, wrong d13) ---
    // "12345678512347" is valid (d13=7); change d13 to another digit
    private const string InvalidChecksum14_d13is0 = "12345678512340";
    private const string InvalidChecksum14_d13is1 = "12345678512341";

    // --- Invalid: REGON-14 with invalid base REGON-9 ---
    // Base "123456784" has wrong d8 (should be 5, not 4)
    private const string InvalidChecksum14_BadBase = "12345678412347";
    // Same invalid base; suffix chosen so the full 14-digit checksum is coincidentally valid
    // (1*2+2*4+3*8+4*5+5*0+6*9+7*7+8*3+4*6=205, 205%11=7, d13=7 ✓) — must not pass two-step validation
    private const string InvalidBase14WithValid14DigitChecksum = "12345678400007";

    // --- Multiple issues ---
    private const string MultipleIssuesRegon = "12X";

    public static TheoryData<string> ValidRegon9Data => new()
    {
        ValidRegon9,
        ValidRegon9WithLeadingZero,
        AnotherValidRegon9,
        ValidRegon9WithChecksumR10,
        ValidRegon9AllZeros,
    };

    public static TheoryData<string> ValidRegon14Data => new()
    {
        ValidRegon14,
        ValidRegon14WithLeadingZeroBase,
        ValidRegon14WithChecksumR10,
        ValidRegon14AllZeros,
    };

    public static TheoryData<string> InvalidCharactersData => new()
    {
        InvalidCharacterAtEnd,
        InvalidCharacterAtStart,
        InvalidCharacterSpace,
        InvalidCharacterLeadingSpace,
        InvalidCharacterTrailingSpace,
        InvalidCharacterDash,
        InvalidCharacterInRegon14,
    };

    public static TheoryData<string?> InvalidLengthData => new()
    {
        null,
        EmptyRegon,
        TooShortRegon,
        RegonLength10,
        RegonLength13,
        TooLongRegon,
    };

    public static TheoryData<string> InvalidChecksumData => new()
    {
        InvalidChecksum9_d8is0,
        InvalidChecksum9_d8is1,
        InvalidChecksum9_d8is9,
        InvalidChecksum14_d13is0,
        InvalidChecksum14_d13is1,
    };

    // --- Happy path: REGON-9 ---

    [Theory]
    [MemberData(nameof(ValidRegon9Data))]
    public void Validate_ValidRegon9_ReturnsValid(string regon)
    {
        var result = Regon.Validate(regon);

        result.IsValid.ShouldBeTrue();
        result.Error.ShouldBeNull();
    }

    // --- Happy path: REGON-14 ---

    [Theory]
    [MemberData(nameof(ValidRegon14Data))]
    public void Validate_ValidRegon14_ReturnsValid(string regon)
    {
        var result = Regon.Validate(regon);

        result.IsValid.ShouldBeTrue();
        result.Error.ShouldBeNull();
    }

    // --- InvalidCharacters ---

    [Theory]
    [MemberData(nameof(InvalidCharactersData))]
    public void Validate_NonDigitCharacters_ReturnsInvalidCharacters(string regon)
    {
        var result = Regon.Validate(regon);

        result.IsValid.ShouldBeFalse();
        result.Error.ShouldBe(RegonValidationError.InvalidCharacters);
    }

    // --- InvalidLength ---

    [Theory]
    [MemberData(nameof(InvalidLengthData))]
    public void Validate_WrongLength_ReturnsInvalidLength(string? regon)
    {
        var result = Regon.Validate(regon);

        result.IsValid.ShouldBeFalse();
        result.Error.ShouldBe(RegonValidationError.InvalidLength);
    }

    // --- InvalidChecksum ---

    [Theory]
    [MemberData(nameof(InvalidChecksumData))]
    public void Validate_WrongChecksum_ReturnsInvalidChecksum(string regon)
    {
        var result = Regon.Validate(regon);

        result.IsValid.ShouldBeFalse();
        result.Error.ShouldBe(RegonValidationError.InvalidChecksum);
    }

    // --- REGON-14 two-step: bad base checksum also returns InvalidChecksum ---

    [Fact]
    public void Validate_Regon14WithInvalidBase_ReturnsInvalidChecksum()
    {
        var result = Regon.Validate(InvalidChecksum14_BadBase);

        result.Error.ShouldBe(RegonValidationError.InvalidChecksum);
    }

    [Fact]
    public void Validate_Regon14BadBaseWithCoincidentallyValid14DigitChecksum_ReturnsInvalidChecksum()
    {
        // Base fails; 14-digit checksum is coincidentally valid — validator must short-circuit at base failure.
        var result = Regon.Validate(InvalidBase14WithValid14DigitChecksum);

        result.Error.ShouldBe(RegonValidationError.InvalidChecksum);
    }

    [Fact]
    public void Validate_SpanOverload_Regon14WithInvalidBase_ReturnsInvalidChecksum()
    {
        var result = Regon.Validate(InvalidChecksum14_BadBase.AsSpan());

        result.Error.ShouldBe(RegonValidationError.InvalidChecksum);
    }

    // --- Validation order: characters → length → checksum ---

    [Fact]
    public void Validate_InvalidCharactersTakesPriorityOverInvalidLength()
    {
        var result = Regon.Validate(MultipleIssuesRegon);

        result.Error.ShouldBe(RegonValidationError.InvalidCharacters);
    }

    // --- Span overload ---

    [Fact]
    public void Validate_SpanOverload_ValidRegon9_ReturnsValid()
    {
        Regon.Validate(ValidRegon9.AsSpan()).IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_SpanOverload_ValidRegon14_ReturnsValid()
    {
        Regon.Validate(ValidRegon14.AsSpan()).IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_SpanOverload_InvalidCharacters_ReturnsInvalidCharacters()
    {
        Regon.Validate(InvalidCharacterAtEnd.AsSpan()).Error.ShouldBe(RegonValidationError.InvalidCharacters);
    }

    [Fact]
    public void Validate_SpanOverload_InvalidChecksum_ReturnsInvalidChecksum()
    {
        Regon.Validate(InvalidChecksum9_d8is0.AsSpan()).Error.ShouldBe(RegonValidationError.InvalidChecksum);
    }

    [Fact]
    public void Validate_SpanOverload_Regon14BadBaseWithCoincidentallyValid14DigitChecksum_ReturnsInvalidChecksum()
    {
        var result = Regon.Validate(InvalidBase14WithValid14DigitChecksum.AsSpan());

        result.Error.ShouldBe(RegonValidationError.InvalidChecksum);
    }

    [Fact]
    public void Validate_SpanOverload_EmptySpan_ReturnsInvalidLength()
    {
        Regon.Validate(ReadOnlySpan<char>.Empty).Error.ShouldBe(RegonValidationError.InvalidLength);
    }

    // --- Match ---

    [Fact]
    public void Match_ValidRegon_InvokesOnValid()
    {
        var result = Regon.Validate(ValidRegon9);

        var value = result.Match(onValid: () => "ok", onError: e => e.ToString());

        value.ShouldBe("ok");
    }

    [Fact]
    public void Match_InvalidRegon_InvokesOnErrorWithCorrectEnum()
    {
        var result = Regon.Validate(InvalidChecksum9_d8is0);

        var error = result.Match(onValid: () => (RegonValidationError?)null, onError: e => (RegonValidationError?)e);

        error.ShouldBe(RegonValidationError.InvalidChecksum);
    }
}
