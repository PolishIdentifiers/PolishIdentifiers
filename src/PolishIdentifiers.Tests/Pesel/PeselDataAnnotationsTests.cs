using System.ComponentModel.DataAnnotations;
using DataAnnotationsValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;
using Shouldly;

namespace PolishIdentifiers.Tests;

public class PeselDataAnnotationsTests
{
    private const string ValidPesel = "44051401458";
    private const string InvalidChecksumPesel = "44051401457";
    private const string InvalidLengthPesel = "4405140145";
    private const string InvalidCharactersPesel = "4405140145A";
    private const string InvalidDatePesel = "99223112345";
    private const string LeadingWhitespacePesel = " 44051401458";
    private const string TrailingWhitespacePesel = "44051401458 ";

    private sealed class StringDto
    {
        [ValidPesel]
        public string? Pesel { get; set; }
    }

    private sealed class RequiredStringDto
    {
        [Required]
        [ValidPesel]
        public string? Pesel { get; set; }
    }

    private sealed class StrongTypeDto
    {
        [ValidPesel]
        public Pesel Pesel { get; set; }
    }

    private sealed class ObjectDto
    {
        [ValidPesel]
        public object? Pesel { get; set; }
    }

    public static TheoryData<string> InvalidStringPeselData => new()
    {
        InvalidChecksumPesel,
        InvalidLengthPesel,
        InvalidCharactersPesel,
        InvalidDatePesel,
        LeadingWhitespacePesel,
        TrailingWhitespacePesel
    };

    [Fact]
    public void ValidPeselAttribute_ValidString_IsValid()
    {
        var dto = new StringDto { Pesel = ValidPesel };

        var isValid = TryValidate(dto, out var results);

        isValid.ShouldBeTrue();
        results.ShouldBeEmpty();
    }

    [Fact]
    public void ValidPeselAttribute_InvalidString_ReturnsMemberNameAndMessage()
    {
        var dto = new StringDto { Pesel = InvalidChecksumPesel };

        var isValid = TryValidate(dto, out var results);
        var error = results.ShouldHaveSingleItem();

        isValid.ShouldBeFalse();
        error.MemberNames.ShouldContain(nameof(StringDto.Pesel));
        error.ErrorMessage.ShouldBe("The Pesel field is not a valid PESEL.");
    }

    [Fact]
    public void ValidPeselAttribute_NullString_IsValid()
    {
        var dto = new StringDto { Pesel = null };

        var isValid = TryValidate(dto, out var results);

        isValid.ShouldBeTrue();
        results.ShouldBeEmpty();
    }

    [Fact]
    public void ValidPeselAttribute_NullStringWithRequired_IsInvalidByRequiredOnly()
    {
        var dto = new RequiredStringDto { Pesel = null };

        var isValid = TryValidate(dto, out var results);
        var error = results.ShouldHaveSingleItem();

        isValid.ShouldBeFalse();
        error.MemberNames.ShouldContain(nameof(RequiredStringDto.Pesel));
    }

    [Fact]
    public void ValidPeselAttribute_DefaultPeselStruct_IsInvalid()
    {
        var dto = new StrongTypeDto();

        var isValid = TryValidate(dto, out var results);
        var error = results.ShouldHaveSingleItem();

        isValid.ShouldBeFalse();
        error.MemberNames.ShouldContain(nameof(StrongTypeDto.Pesel));
    }

    [Fact]
    public void ValidPeselAttribute_ParsedPeselStruct_IsValid()
    {
        var dto = new StrongTypeDto { Pesel = Pesel.Parse(ValidPesel) };

        var isValid = TryValidate(dto, out var results);

        isValid.ShouldBeTrue();
        results.ShouldBeEmpty();
    }

    [Fact]
    public void ValidPeselAttribute_UnsupportedType_IsInvalid()
    {
        var dto = new ObjectDto { Pesel = 12345 };

        var isValid = TryValidate(dto, out var results);
        var error = results.ShouldHaveSingleItem();

        isValid.ShouldBeFalse();
        error.MemberNames.ShouldContain(nameof(ObjectDto.Pesel));
    }

    [Theory]
    [MemberData(nameof(InvalidStringPeselData))]
    public void ValidPeselAttribute_InvalidStringEdgeCases_AreInvalid(string invalidPesel)
    {
        var dto = new StringDto { Pesel = invalidPesel };

        var isValid = TryValidate(dto, out var results);
        var error = results.ShouldHaveSingleItem();

        isValid.ShouldBeFalse();
        error.MemberNames.ShouldContain(nameof(StringDto.Pesel));
        error.ErrorMessage.ShouldBe("The Pesel field is not a valid PESEL.");
    }

    [Fact]
    public void ValidPeselAttribute_RequiredAndInvalidString_ReturnsOnlyValidPeselError()
    {
        var dto = new RequiredStringDto { Pesel = InvalidChecksumPesel };

        var isValid = TryValidate(dto, out var results);
        var error = results.ShouldHaveSingleItem();

        isValid.ShouldBeFalse();
        error.MemberNames.ShouldContain(nameof(RequiredStringDto.Pesel));
        error.ErrorMessage.ShouldBe("The Pesel field is not a valid PESEL.");
    }

    [Fact]
    public void ValidPeselAttribute_GetValidationResult_WithoutMemberName_HasNoMemberNames()
    {
        var attribute = new ValidPeselAttribute();
        var context = new ValidationContext(new object())
        {
            DisplayName = "National ID"
        };

        var result = attribute.GetValidationResult(InvalidChecksumPesel, context);

        result.ShouldNotBeNull();
        result!.ErrorMessage.ShouldBe("The National ID field is not a valid PESEL.");
        result.MemberNames.ShouldBeEmpty();
    }

    [Fact]
    public void ValidPeselAttribute_IsValid_ObjectOverload_CoversSupportedAndUnsupportedValues()
    {
        var attribute = new ValidPeselAttribute();

        attribute.IsValid(null).ShouldBeTrue();
        attribute.IsValid(ValidPesel).ShouldBeTrue();
        attribute.IsValid(InvalidChecksumPesel).ShouldBeFalse();
        attribute.IsValid(Pesel.Parse(ValidPesel)).ShouldBeTrue();
        attribute.IsValid(default(Pesel)).ShouldBeFalse();
        attribute.IsValid(12345).ShouldBeFalse();
    }

    private static bool TryValidate(object instance, out List<DataAnnotationsValidationResult> results)
    {
        var context = new ValidationContext(instance);
        results = new List<DataAnnotationsValidationResult>();
        return Validator.TryValidateObject(instance, context, results, validateAllProperties: true);
    }
}
