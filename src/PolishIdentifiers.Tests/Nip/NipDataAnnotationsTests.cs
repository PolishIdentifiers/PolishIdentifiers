using System.ComponentModel.DataAnnotations;
using DataAnnotationsValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;
using Shouldly;

namespace PolishIdentifiers.Tests;

public class NipDataAnnotationsTests
{
    private const string ValidNip = "1234563218";
    private const string InvalidChecksumNip = "1234563217";
    private const string InvalidLengthNip = "123456321";
    private const string InvalidCharactersNip = "123456321X";
    private const string HyphenatedNip = "123-456-32-18";
    private const string VatEuNip = "PL1234563218";
    private const string VatEuWithSpaceNip = "PL 1234563218";
    private const string VatEuHyphenatedNip = "PL 123-456-32-18";
    private const string LeadingWhitespaceNip = " 1234563218";
    private const string TrailingWhitespaceNip = "1234563218 ";
    private const string ExpectedErrorMessage = "The Nip field is not a valid NIP.";
    private const string AlternateDisplayName = "Tax ID";
    private const string AlternateDisplayNameErrorMessage = "The Tax ID field is not a valid NIP.";

    private sealed class StringDto
    {
        [ValidNip]
        public string? Nip { get; set; }
    }

    private sealed class RequiredStringDto
    {
        [Required]
        [ValidNip]
        public string? Nip { get; set; }
    }

    private sealed class StrongTypeDto
    {
        [ValidNip]
        public Nip Nip { get; set; }
    }

    private sealed class ObjectDto
    {
        [ValidNip]
        public object? Nip { get; set; }
    }

    public static TheoryData<string> ValidFormattedStringNipData => new()
    {
        HyphenatedNip,
        VatEuNip,
        VatEuWithSpaceNip,
        VatEuHyphenatedNip,
    };

    public static TheoryData<string> InvalidStringNipData => new()
    {
        InvalidChecksumNip,
        InvalidLengthNip,
        InvalidCharactersNip,
        LeadingWhitespaceNip,
        TrailingWhitespaceNip,
    };

    [Fact]
    public void ValidNipAttribute_ValidString_IsValid()
    {
        var dto = new StringDto { Nip = ValidNip };

        var isValid = TryValidate(dto, out var results);

        isValid.ShouldBeTrue();
        results.ShouldBeEmpty();
    }

    [Theory]
    [MemberData(nameof(ValidFormattedStringNipData))]
    public void ValidNipAttribute_ValidFormattedString_IsValid(string formattedNip)
    {
        var dto = new StringDto { Nip = formattedNip };

        var isValid = TryValidate(dto, out var results);

        isValid.ShouldBeTrue();
        results.ShouldBeEmpty();
    }

    [Fact]
    public void ValidNipAttribute_InvalidString_ReturnsMemberNameAndMessage()
    {
        var dto = new StringDto { Nip = InvalidChecksumNip };

        var isValid = TryValidate(dto, out var results);
        var error = results.ShouldHaveSingleItem();

        isValid.ShouldBeFalse();
        error.MemberNames.ShouldContain(nameof(StringDto.Nip));
        error.ErrorMessage.ShouldBe(ExpectedErrorMessage);
    }

    [Fact]
    public void ValidNipAttribute_NullString_IsValid()
    {
        var dto = new StringDto { Nip = null };

        var isValid = TryValidate(dto, out var results);

        isValid.ShouldBeTrue();
        results.ShouldBeEmpty();
    }

    [Fact]
    public void ValidNipAttribute_NullStringWithRequired_IsInvalidByRequiredOnly()
    {
        var dto = new RequiredStringDto { Nip = null };

        var isValid = TryValidate(dto, out var results);
        var error = results.ShouldHaveSingleItem();

        isValid.ShouldBeFalse();
        error.MemberNames.ShouldContain(nameof(RequiredStringDto.Nip));
    }

    [Fact]
    public void ValidNipAttribute_DefaultNipStruct_IsInvalid()
    {
        var dto = new StrongTypeDto();

        var isValid = TryValidate(dto, out var results);
        var error = results.ShouldHaveSingleItem();

        isValid.ShouldBeFalse();
        error.MemberNames.ShouldContain(nameof(StrongTypeDto.Nip));
    }

    [Fact]
    public void ValidNipAttribute_ParsedNipStruct_IsValid()
    {
        var dto = new StrongTypeDto { Nip = Nip.Parse(ValidNip) };

        var isValid = TryValidate(dto, out var results);

        isValid.ShouldBeTrue();
        results.ShouldBeEmpty();
    }

    [Fact]
    public void ValidNipAttribute_UnsupportedType_IsInvalid()
    {
        var dto = new ObjectDto { Nip = 12345 };

        var isValid = TryValidate(dto, out var results);
        var error = results.ShouldHaveSingleItem();

        isValid.ShouldBeFalse();
        error.MemberNames.ShouldContain(nameof(ObjectDto.Nip));
    }

    [Theory]
    [MemberData(nameof(InvalidStringNipData))]
    public void ValidNipAttribute_InvalidStringEdgeCases_AreInvalid(string invalidNip)
    {
        var dto = new StringDto { Nip = invalidNip };

        var isValid = TryValidate(dto, out var results);
        var error = results.ShouldHaveSingleItem();

        isValid.ShouldBeFalse();
        error.MemberNames.ShouldContain(nameof(StringDto.Nip));
        error.ErrorMessage.ShouldBe(ExpectedErrorMessage);
    }

    [Fact]
    public void ValidNipAttribute_RequiredAndInvalidString_ReturnsOnlyValidNipError()
    {
        var dto = new RequiredStringDto { Nip = InvalidChecksumNip };

        var isValid = TryValidate(dto, out var results);
        var error = results.ShouldHaveSingleItem();

        isValid.ShouldBeFalse();
        error.MemberNames.ShouldContain(nameof(RequiredStringDto.Nip));
        error.ErrorMessage.ShouldBe(ExpectedErrorMessage);
    }

    [Fact]
    public void ValidNipAttribute_GetValidationResult_WithoutMemberName_HasNoMemberNames()
    {
        var attribute = new ValidNipAttribute();
        var context = new ValidationContext(new object())
        {
            DisplayName = AlternateDisplayName,
        };

        var result = attribute.GetValidationResult(InvalidChecksumNip, context);

        result.ShouldNotBeNull();
        result.ErrorMessage.ShouldBe(AlternateDisplayNameErrorMessage);
        result.MemberNames.ShouldBeEmpty();
    }

    [Fact]
    public void ValidNipAttribute_IsValid_ObjectOverload_CoversSupportedAndUnsupportedValues()
    {
        var attribute = new ValidNipAttribute();

        attribute.IsValid(null).ShouldBeTrue();
        attribute.IsValid(ValidNip).ShouldBeTrue();
        attribute.IsValid(HyphenatedNip).ShouldBeTrue();
        attribute.IsValid(VatEuNip).ShouldBeTrue();
        attribute.IsValid(InvalidChecksumNip).ShouldBeFalse();
        attribute.IsValid(Nip.Parse(ValidNip)).ShouldBeTrue();
        attribute.IsValid(default(Nip)).ShouldBeFalse();
        attribute.IsValid(12345).ShouldBeFalse();
    }

    private static bool TryValidate(object instance, out List<DataAnnotationsValidationResult> results)
    {
        var context = new ValidationContext(instance);
        results = [];
        return Validator.TryValidateObject(instance, context, results, validateAllProperties: true);
    }
}
