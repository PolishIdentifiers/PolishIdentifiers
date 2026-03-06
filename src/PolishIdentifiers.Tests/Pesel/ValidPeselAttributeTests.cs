using System.ComponentModel.DataAnnotations;
using PolishIdentifiers;

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
        => Assert.Equal(ValidationResult.Success, Validate(null));

    [Fact]
    public void Null_IsValid_ReturnsTrue()
        => Assert.True(IsValidDirect(null));

    // --- valid string ---

    [Fact]
    public void ValidString_ReturnsSuccess()
        => Assert.Equal(ValidationResult.Success, Validate(ValidPesel));

    [Fact]
    public void ValidString_LeadingZero_ReturnsSuccess()
        => Assert.Equal(ValidationResult.Success, Validate(ValidPeselLeadingZero));

    [Fact]
    public void ValidString_IsValid_ReturnsTrue()
        => Assert.True(IsValidDirect(ValidPesel));

    // --- invalid string: each error code in isolation ---

    [Fact]
    public void InvalidChecksum_ReturnsFailure()
        => Assert.NotEqual(ValidationResult.Success, Validate(InvalidChecksumPesel));

    [Fact]
    public void InvalidChecksum_IsValid_ReturnsFalse()
        => Assert.False(IsValidDirect(InvalidChecksumPesel));

    [Fact]
    public void InvalidLength_TooShort_ReturnsFailure()
        => Assert.NotEqual(ValidationResult.Success, Validate(TooShortPesel));

    [Fact]
    public void InvalidLength_TooLong_ReturnsFailure()
        => Assert.NotEqual(ValidationResult.Success, Validate(TooLongPesel));

    [Fact]
    public void InvalidLength_Empty_ReturnsFailure()
        => Assert.NotEqual(ValidationResult.Success, Validate(string.Empty));

    [Fact]
    public void InvalidCharacters_ReturnsFailure()
        => Assert.NotEqual(ValidationResult.Success, Validate(InvalidCharactersPesel));

    [Fact]
    public void InvalidDate_ReturnsFailure()
        => Assert.NotEqual(ValidationResult.Success, Validate(InvalidDatePesel));

    // via Invalid.* generators — each violates exactly one rule
    [Fact]
    public void Invalid_WrongChecksum_Generator_ReturnsFailure()
        => Assert.NotEqual(ValidationResult.Success, Validate(PeselGenerator.Invalid.WrongChecksum()));

    [Fact]
    public void Invalid_WrongDate_Generator_ReturnsFailure()
        => Assert.NotEqual(ValidationResult.Success, Validate(PeselGenerator.Invalid.WrongDate()));

    [Fact]
    public void Invalid_WrongLength_Generator_ReturnsFailure()
        => Assert.NotEqual(ValidationResult.Success, Validate(PeselGenerator.Invalid.WrongLength()));

    [Fact]
    public void Invalid_NonNumeric_Generator_ReturnsFailure()
        => Assert.NotEqual(ValidationResult.Success, Validate(PeselGenerator.Invalid.NonNumeric()));

    // --- whitespace edge cases ---

    [Fact]
    public void LeadingWhitespace_ReturnsFailure()
        => Assert.NotEqual(ValidationResult.Success, Validate(LeadingWhitespacePesel));

    [Fact]
    public void TrailingWhitespace_ReturnsFailure()
        => Assert.NotEqual(ValidationResult.Success, Validate(TrailingWhitespacePesel));

    [Fact]
    public void MiddleTab_ReturnsFailure()
        => Assert.NotEqual(ValidationResult.Success, Validate(MiddleTabPesel));

    // --- pathological strings ---

    [Fact]
    public void AllZeros_ReturnsFailure()
        => Assert.NotEqual(ValidationResult.Success, Validate(AllZerosPesel));

    [Fact]
    public void AllNines_ReturnsFailure()
        => Assert.NotEqual(ValidationResult.Success, Validate(AllNinesPesel));

    // --- Pesel struct ---

    [Fact]
    public void ValidPeselStruct_ReturnsSuccess()
    {
        object value = Pesel.Parse(ValidPesel); // boxed Pesel
        Assert.Equal(ValidationResult.Success, Validate(value));
    }

    [Fact]
    public void ValidPeselStruct_IsValid_ReturnsTrue()
    {
        object value = Pesel.Parse(ValidPesel);
        Assert.True(IsValidDirect(value));
    }

    [Fact]
    public void DefaultPeselStruct_ReturnsFailure()
    {
        object value = default(Pesel); // boxed default
        Assert.NotEqual(ValidationResult.Success, Validate(value));
    }

    [Fact]
    public void DefaultPeselStruct_IsValid_ReturnsFalse()
    {
        object value = default(Pesel);
        Assert.False(IsValidDirect(value));
    }

    // --- nullable Pesel struct ---

    [Fact]
    public void NullablePeselStruct_WithValue_ReturnsSuccess()
    {
        Pesel? nullable = Pesel.Parse(ValidPesel);
        Assert.Equal(ValidationResult.Success, Validate(nullable)); // boxed as Pesel
    }

    [Fact]
    public void NullablePeselStruct_Null_ReturnsSuccess()
    {
        Pesel? nullable = null;
        Assert.Equal(ValidationResult.Success, Validate(nullable)); // null object
    }

    // --- unknown type ---

    [Fact]
    public void UnknownType_Long_ReturnsFailure()
        => Assert.NotEqual(ValidationResult.Success, Validate(44051401458L));

    [Fact]
    public void UnknownType_Long_IsValid_ReturnsFalse()
        => Assert.False(IsValidDirect(44051401458L));

    // --- error result shape ---

    [Fact]
    public void Failure_MemberNames_ContainsMemberName()
    {
        const string memberName = "PeselNumber";
        var result = Validate(InvalidChecksumPesel, memberName: memberName);

        Assert.Contains(memberName, result!.MemberNames);
    }

    [Fact]
    public void Failure_ErrorMessage_ContainsDisplayName()
    {
        const string displayName = "PESEL Number";
        var result = Validate(InvalidChecksumPesel, displayName: displayName);

        Assert.Contains(displayName, result!.ErrorMessage, StringComparison.Ordinal);
    }

    [Fact]
    public void Failure_DisplayName_And_MemberName_AreIndependent()
    {
        // DisplayName goes into the message, MemberName goes into MemberNames — they are separate
        const string memberName  = "peselField";
        const string displayName = "Your PESEL";

        var result = Validate(InvalidChecksumPesel, memberName: memberName, displayName: displayName);

        Assert.Contains(displayName, result!.ErrorMessage, StringComparison.Ordinal);
        Assert.Contains(memberName,  result.MemberNames);
        Assert.DoesNotContain(memberName, result.ErrorMessage, StringComparison.Ordinal);
    }

    [Fact]
    public void Failure_WhenMemberNameIsNull_MemberNamesIsEmpty()
    {
        var result = Validate(InvalidChecksumPesel, memberName: null);

        Assert.Empty(result!.MemberNames);
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

        Assert.Equal(expected, directResult);
        Assert.Equal(expected, contextIsValid);
    }

    // --- composability with [Required] ---

    [Fact]
    public void NullPesel_PassesValidPeselAttribute_ButFailsRequiredAttribute()
    {
        Assert.True(IsValidDirect(null));

        var required = new RequiredAttribute();
        Assert.False(required.IsValid(null));
    }
}
