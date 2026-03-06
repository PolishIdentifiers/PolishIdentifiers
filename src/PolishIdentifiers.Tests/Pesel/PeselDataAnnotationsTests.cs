using System.ComponentModel.DataAnnotations;
using DataAnnotationsValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

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

        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Fact]
    public void ValidPeselAttribute_InvalidString_ReturnsMemberNameAndMessage()
    {
        var dto = new StringDto { Pesel = InvalidChecksumPesel };

        var isValid = TryValidate(dto, out var results);
        var error = Assert.Single(results);

        Assert.False(isValid);
        Assert.Contains(nameof(StringDto.Pesel), error.MemberNames);
        Assert.Equal("The Pesel field is not a valid PESEL.", error.ErrorMessage);
    }

    [Fact]
    public void ValidPeselAttribute_NullString_IsValid()
    {
        var dto = new StringDto { Pesel = null };

        var isValid = TryValidate(dto, out var results);

        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Fact]
    public void ValidPeselAttribute_NullStringWithRequired_IsInvalidByRequiredOnly()
    {
        var dto = new RequiredStringDto { Pesel = null };

        var isValid = TryValidate(dto, out var results);
        var error = Assert.Single(results);

        Assert.False(isValid);
        Assert.Contains(nameof(RequiredStringDto.Pesel), error.MemberNames);
    }

    [Fact]
    public void ValidPeselAttribute_DefaultPeselStruct_IsInvalid()
    {
        var dto = new StrongTypeDto();

        var isValid = TryValidate(dto, out var results);
        var error = Assert.Single(results);

        Assert.False(isValid);
        Assert.Contains(nameof(StrongTypeDto.Pesel), error.MemberNames);
    }

    [Fact]
    public void ValidPeselAttribute_ParsedPeselStruct_IsValid()
    {
        var dto = new StrongTypeDto { Pesel = Pesel.Parse(ValidPesel) };

        var isValid = TryValidate(dto, out var results);

        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Fact]
    public void ValidPeselAttribute_UnsupportedType_IsInvalid()
    {
        var dto = new ObjectDto { Pesel = 12345 };

        var isValid = TryValidate(dto, out var results);
        var error = Assert.Single(results);

        Assert.False(isValid);
        Assert.Contains(nameof(ObjectDto.Pesel), error.MemberNames);
    }

    [Theory]
    [MemberData(nameof(InvalidStringPeselData))]
    public void ValidPeselAttribute_InvalidStringEdgeCases_AreInvalid(string invalidPesel)
    {
        var dto = new StringDto { Pesel = invalidPesel };

        var isValid = TryValidate(dto, out var results);
        var error = Assert.Single(results);

        Assert.False(isValid);
        Assert.Contains(nameof(StringDto.Pesel), error.MemberNames);
        Assert.Equal("The Pesel field is not a valid PESEL.", error.ErrorMessage);
    }

    [Fact]
    public void ValidPeselAttribute_RequiredAndInvalidString_ReturnsOnlyValidPeselError()
    {
        var dto = new RequiredStringDto { Pesel = InvalidChecksumPesel };

        var isValid = TryValidate(dto, out var results);
        var error = Assert.Single(results);

        Assert.False(isValid);
        Assert.Contains(nameof(RequiredStringDto.Pesel), error.MemberNames);
        Assert.Equal("The Pesel field is not a valid PESEL.", error.ErrorMessage);
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

        Assert.NotNull(result);
        Assert.Equal("The National ID field is not a valid PESEL.", result!.ErrorMessage);
        Assert.Empty(result.MemberNames);
    }

    [Fact]
    public void ValidPeselAttribute_IsValid_ObjectOverload_CoversSupportedAndUnsupportedValues()
    {
        var attribute = new ValidPeselAttribute();

        Assert.True(attribute.IsValid(null));
        Assert.True(attribute.IsValid(ValidPesel));
        Assert.False(attribute.IsValid(InvalidChecksumPesel));
        Assert.True(attribute.IsValid(Pesel.Parse(ValidPesel)));
        Assert.False(attribute.IsValid(default(Pesel)));
        Assert.False(attribute.IsValid(12345));
    }

    private static bool TryValidate(object instance, out List<DataAnnotationsValidationResult> results)
    {
        var context = new ValidationContext(instance);
        results = new List<DataAnnotationsValidationResult>();
        return Validator.TryValidateObject(instance, context, results, validateAllProperties: true);
    }
}
