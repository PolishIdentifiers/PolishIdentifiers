using System.ComponentModel.DataAnnotations;
using PolishIdentifiers;
using Shouldly;

namespace PolishIdentifiers.Tests;

public class ValidPeselAttributeTests
{
    private const string ValidPesel              = "44051401458";
    private const string ValidPeselLeadingZero   = "02070803628"; // leading zero must round-trip correctly
    private const string InvalidChecksumPesel    = "44051401457";
    private const string TooShortPesel           = "4405140145";
    private const string TooLongPesel            = "440514014580";
    private const string InvalidCharactersPesel  = "4405140145X";
    private const string InvalidDatePesel        = "99223112345";
    private const string LeadingWhitespacePesel  = " 44051401458";
    private const string TrailingWhitespacePesel = "44051401458 ";
    private const string MiddleTabPesel          = "4405140\t458";
    private const string AllZerosPesel           = "00000000000"; // InvalidDate
    private const string AllNinesPesel           = "99999999999"; // InvalidDate

    // --- helpers ---

    private static ValidationResult? Validate(
        object? value,
        string? memberName  = "Pesel",
        string? displayName = null)
    {
        var attr = new ValidPeselAttribute();
        var ctx  = new ValidationContext(new object()) { MemberName = memberName };
        // DisplayName setter throws on null or empty — only set when explicitly provided
        if (!string.IsNullOrEmpty(displayName))
            ctx.DisplayName = displayName;
        return attr.GetValidationResult(value, ctx);
    }

    private static bool IsValidDirect(object? value)
        => new ValidPeselAttribute().IsValid(value);

    // --- null ---

    [Fact]
    public void Null_ReturnsSuccess()
        => Validate(null).ShouldBe(ValidationResult.Success);

    [Fact]
    public void Null_IsValid_ReturnsTrue()
        => IsValidDirect(null).ShouldBeTrue();

    // --- valid string ---

    [Fact]
    public void ValidString_ReturnsSuccess()
        => Validate(ValidPesel).ShouldBe(ValidationResult.Success);

    [Fact]
    public void ValidString_LeadingZero_ReturnsSuccess()
        => Validate(ValidPeselLeadingZero).ShouldBe(ValidationResult.Success);

    [Fact]
    public void ValidString_IsValid_ReturnsTrue()
        => IsValidDirect(ValidPesel).ShouldBeTrue();

    // --- invalid string: each error code in isolation ---

    [Fact]
    public void InvalidChecksum_ReturnsFailure()
        => Validate(InvalidChecksumPesel).ShouldNotBe(ValidationResult.Success);

    [Fact]
    public void InvalidChecksum_IsValid_ReturnsFalse()
        => IsValidDirect(InvalidChecksumPesel).ShouldBeFalse();

    [Fact]
    public void InvalidLength_TooShort_ReturnsFailure()
        => Validate(TooShortPesel).ShouldNotBe(ValidationResult.Success);

    [Fact]
    public void InvalidLength_TooLong_ReturnsFailure()
        => Validate(TooLongPesel).ShouldNotBe(ValidationResult.Success);

    [Fact]
    public void InvalidLength_Empty_ReturnsFailure()
        => Validate(string.Empty).ShouldNotBe(ValidationResult.Success);

    [Fact]
    public void InvalidCharacters_ReturnsFailure()
        => Validate(InvalidCharactersPesel).ShouldNotBe(ValidationResult.Success);

    [Fact]
    public void InvalidDate_ReturnsFailure()
        => Validate(InvalidDatePesel).ShouldNotBe(ValidationResult.Success);

    // via Invalid.* generators — each violates exactly one rule
    [Fact]
    public void Invalid_WrongChecksum_Generator_ReturnsFailure()
        => Validate(PeselGenerator.Invalid.WrongChecksum()).ShouldNotBe(ValidationResult.Success);

    [Fact]
    public void Invalid_WrongDate_Generator_ReturnsFailure()
        => Validate(PeselGenerator.Invalid.WrongDate()).ShouldNotBe(ValidationResult.Success);

    [Fact]
    public void Invalid_WrongLength_Generator_ReturnsFailure()
        => Validate(PeselGenerator.Invalid.WrongLength()).ShouldNotBe(ValidationResult.Success);

    [Fact]
    public void Invalid_NonNumeric_Generator_ReturnsFailure()
        => Validate(PeselGenerator.Invalid.NonNumeric()).ShouldNotBe(ValidationResult.Success);

    // --- whitespace edge cases ---

    [Fact]
    public void LeadingWhitespace_ReturnsFailure()
        => Validate(LeadingWhitespacePesel).ShouldNotBe(ValidationResult.Success);

    [Fact]
    public void TrailingWhitespace_ReturnsFailure()
        => Validate(TrailingWhitespacePesel).ShouldNotBe(ValidationResult.Success);

    [Fact]
    public void MiddleTab_ReturnsFailure()
        => Validate(MiddleTabPesel).ShouldNotBe(ValidationResult.Success);

    // --- pathological strings ---

    [Fact]
    public void AllZeros_ReturnsFailure()
        => Validate(AllZerosPesel).ShouldNotBe(ValidationResult.Success);

    [Fact]
    public void AllNines_ReturnsFailure()
        => Validate(AllNinesPesel).ShouldNotBe(ValidationResult.Success);

    // --- Pesel struct ---

    [Fact]
    public void ValidPeselStruct_ReturnsSuccess()
    {
        object value = Pesel.Parse(ValidPesel); // boxed Pesel
        Validate(value).ShouldBe(ValidationResult.Success);
    }

    [Fact]
    public void ValidPeselStruct_IsValid_ReturnsTrue()
    {
        object value = Pesel.Parse(ValidPesel);
        IsValidDirect(value).ShouldBeTrue();
    }

    [Fact]
    public void DefaultPeselStruct_ReturnsFailure()
    {
        object value = default(Pesel); // boxed default
        Validate(value).ShouldNotBe(ValidationResult.Success);
    }

    [Fact]
    public void DefaultPeselStruct_IsValid_ReturnsFalse()
    {
        object value = default(Pesel);
        IsValidDirect(value).ShouldBeFalse();
    }

    // --- nullable Pesel struct ---

    [Fact]
    public void NullablePeselStruct_WithValue_ReturnsSuccess()
    {
        Pesel? nullable = Pesel.Parse(ValidPesel);
        Validate(nullable).ShouldBe(ValidationResult.Success); // boxed as Pesel
    }

    [Fact]
    public void NullablePeselStruct_Null_ReturnsSuccess()
    {
        Pesel? nullable = null;
        Validate(nullable).ShouldBe(ValidationResult.Success); // null object
    }

    // --- unknown type ---

    [Fact]
    public void UnknownType_Long_ReturnsFailure()
        => Validate(44051401458L).ShouldNotBe(ValidationResult.Success);

    [Fact]
    public void UnknownType_Long_IsValid_ReturnsFalse()
        => IsValidDirect(44051401458L).ShouldBeFalse();

    // --- error result shape ---

    [Fact]
    public void Failure_MemberNames_ContainsMemberName()
    {
        const string memberName = "PeselNumber";
        var result = Validate(InvalidChecksumPesel, memberName: memberName);

        result.ShouldNotBeNull();
        result.MemberNames.ShouldContain(memberName);
    }

    [Fact]
    public void Failure_ErrorMessage_ContainsDisplayName()
    {
        const string displayName = "PESEL Number";
        var result = Validate(InvalidChecksumPesel, displayName: displayName);

        result.ShouldNotBeNull();
        result.ErrorMessage.ShouldContain(displayName);
    }

    [Fact]
    public void Failure_DisplayName_And_MemberName_AreIndependent()
    {
        // DisplayName goes into the message, MemberName goes into MemberNames — they are separate
        const string memberName  = "peselField";
        const string displayName = "Your PESEL";

        var result = Validate(InvalidChecksumPesel, memberName: memberName, displayName: displayName);

        result.ShouldNotBeNull();
        result.ErrorMessage.ShouldContain(displayName);
        result.MemberNames.ShouldContain(memberName);
        result.ErrorMessage.ShouldNotContain(memberName);
    }

    [Fact]
    public void Failure_WhenMemberNameIsNull_MemberNamesIsEmpty()
    {
        var result = Validate(InvalidChecksumPesel, memberName: null);

        result.ShouldNotBeNull();
        result.MemberNames.ShouldBeEmpty();
    }

    // --- IsValid(object?) direct calls are consistent with context-based path ---

    [Theory]
    [InlineData(null,                  true)]
    [InlineData(ValidPesel,            true)]
    [InlineData(ValidPeselLeadingZero, true)]
    [InlineData(InvalidChecksumPesel,  false)]
    [InlineData("",                    false)]
    [InlineData(AllZerosPesel,         false)]
    [InlineData(AllNinesPesel,         false)]
    [InlineData(InvalidDatePesel,      false)]
    [InlineData(InvalidCharactersPesel,false)]
    [InlineData(TooShortPesel,         false)]
    [InlineData(TooLongPesel,          false)]
    [InlineData(LeadingWhitespacePesel,false)]
    public void IsValid_Direct_MatchesContextBasedValidation(string? input, bool expected)
    {
        var directResult  = IsValidDirect(input);
        var contextResult = Validate(input);
        var contextIsValid = contextResult == ValidationResult.Success;

        directResult.ShouldBe(expected);
        contextIsValid.ShouldBe(expected);
    }

    // --- composability with [Required] ---

    [Fact]
    public void NullPesel_PassesValidPeselAttribute_ButFailsRequiredAttribute()
    {
        IsValidDirect(null).ShouldBeTrue();

        var required = new RequiredAttribute();
        required.IsValid(null).ShouldBeFalse();
    }
}
