using System.ComponentModel.DataAnnotations;
using DataAnnotationsValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;
using PolishIdentifiers;
using Shouldly;

namespace PolishIdentifiers.Tests;

public class RegonDataAnnotationsTests
{
    private const string ValidRegon9 = "123456785";
    private const string ValidRegon14 = "12345678512347";
    private const string ValidRegon9AllZeros = "000000000";
    private const string ValidRegon14AllZeros = "00000000000000";
    private const string InvalidRegon = "123456780";
    private const string InvalidRegonWrongLength = "1234567";
    private const string ExpectedErrorMessage = "The Regon field is not a valid REGON.";

    private sealed class RegonStringDto
    {
        [ValidRegon]
        public string? Regon { get; set; }
    }

    private sealed class RequiredRegonStringDto
    {
        [Required]
        [ValidRegon]
        public string? Regon { get; set; }
    }

    private sealed class RegonStructDto
    {
        [ValidRegon]
        public Regon Regon { get; set; }
    }

    private sealed class ObjectDto
    {
        [ValidRegon]
        public object? Regon { get; set; }
    }

    private static bool TryValidate(object instance, out List<DataAnnotationsValidationResult> results)
    {
        var context = new ValidationContext(instance);
        results = [];
        return Validator.TryValidateObject(instance, context, results, validateAllProperties: true);
    }

    // ── null is valid ─────────────────────────────────────────────────────────

    [Fact]
    public void ValidRegonAttribute_NullString_IsValid()
    {
        var dto = new RegonStringDto { Regon = null };

        var isValid = TryValidate(dto, out var results);

        isValid.ShouldBeTrue();
        results.ShouldBeEmpty();
    }

    [Fact]
    public void ValidRegonAttribute_NullStringWithRequired_IsInvalidByRequiredOnly()
    {
        var dto = new RequiredRegonStringDto { Regon = null };

        var isValid = TryValidate(dto, out var results);
        var error = results.ShouldHaveSingleItem();

        isValid.ShouldBeFalse();
        error.MemberNames.ShouldContain(nameof(RequiredRegonStringDto.Regon));
    }

    // ── Valid strings ─────────────────────────────────────────────────────────

    [Fact]
    public void ValidRegonAttribute_ValidRegon9String_IsValid()
    {
        var dto = new RegonStringDto { Regon = ValidRegon9 };

        var isValid = TryValidate(dto, out var results);

        isValid.ShouldBeTrue();
        results.ShouldBeEmpty();
    }

    [Fact]
    public void ValidRegonAttribute_ValidRegon14String_IsValid()
    {
        var dto = new RegonStringDto { Regon = ValidRegon14 };

        var isValid = TryValidate(dto, out var results);

        isValid.ShouldBeTrue();
        results.ShouldBeEmpty();
    }

    [Fact]
    public void ValidRegonAttribute_Regon9AllZerosString_IsValid()
    {
        var dto = new RegonStringDto { Regon = ValidRegon9AllZeros };

        var isValid = TryValidate(dto, out var results);

        isValid.ShouldBeTrue();
        results.ShouldBeEmpty();
    }

    [Fact]
    public void ValidRegonAttribute_Regon14AllZerosString_IsValid()
    {
        var dto = new RegonStringDto { Regon = ValidRegon14AllZeros };

        var isValid = TryValidate(dto, out var results);

        isValid.ShouldBeTrue();
        results.ShouldBeEmpty();
    }

    // ── Invalid strings ───────────────────────────────────────────────────────

    [Fact]
    public void ValidRegonAttribute_InvalidRegon_ReturnsMemberNameAndMessage()
    {
        var dto = new RegonStringDto { Regon = InvalidRegon };

        var isValid = TryValidate(dto, out var results);
        var error = results.ShouldHaveSingleItem();

        isValid.ShouldBeFalse();
        error.MemberNames.ShouldContain(nameof(RegonStringDto.Regon));
        error.ErrorMessage.ShouldBe(ExpectedErrorMessage);
    }

    [Fact]
    public void ValidRegonAttribute_EmptyString_IsInvalid()
    {
        var dto = new RegonStringDto { Regon = string.Empty };

        var isValid = TryValidate(dto, out var results);

        isValid.ShouldBeFalse();
        results.ShouldNotBeEmpty();
    }

    // ── Regon struct values ───────────────────────────────────────────────────

    [Fact]
    public void ValidRegonAttribute_ParsedRegon9Struct_IsValid()
    {
        var dto = new RegonStructDto { Regon = Regon.Parse(ValidRegon9) };

        var isValid = TryValidate(dto, out var results);

        isValid.ShouldBeTrue();
        results.ShouldBeEmpty();
    }

    [Fact]
    public void ValidRegonAttribute_ParsedRegon14Struct_IsValid()
    {
        var dto = new RegonStructDto { Regon = Regon.Parse(ValidRegon14) };

        var isValid = TryValidate(dto, out var results);

        isValid.ShouldBeTrue();
        results.ShouldBeEmpty();
    }

    [Fact]
    public void ValidRegonAttribute_ParsedRegon9AllZerosStruct_IsValid()
    {
        var dto = new RegonStructDto { Regon = Regon.Parse(ValidRegon9AllZeros) };

        var isValid = TryValidate(dto, out var results);

        isValid.ShouldBeTrue();
        results.ShouldBeEmpty();
    }

    [Fact]
    public void ValidRegonAttribute_ParsedRegon14AllZerosStruct_IsValid()
    {
        var dto = new RegonStructDto { Regon = Regon.Parse(ValidRegon14AllZeros) };

        var isValid = TryValidate(dto, out var results);

        isValid.ShouldBeTrue();
        results.ShouldBeEmpty();
    }

    [Fact]
    public void ValidRegonAttribute_DefaultRegonStruct_IsInvalid()
    {
        var dto = new RegonStructDto();

        var isValid = TryValidate(dto, out var results);
        var error = results.ShouldHaveSingleItem();

        isValid.ShouldBeFalse();
        error.MemberNames.ShouldContain(nameof(RegonStructDto.Regon));
    }

    // ── Unsupported type ──────────────────────────────────────────────────────

    [Fact]
    public void ValidRegonAttribute_UnsupportedType_IsInvalid()
    {
        var dto = new ObjectDto { Regon = 42 };

        var isValid = TryValidate(dto, out var results);
        var error = results.ShouldHaveSingleItem();

        isValid.ShouldBeFalse();
        error.MemberNames.ShouldContain(nameof(ObjectDto.Regon));
    }

    // ── IsValid public overload covers all cases ──────────────────────────────

    [Fact]
    public void ValidRegonAttribute_IsValid_Null_ReturnsTrue()
    {
        new ValidRegonAttribute().IsValid(null).ShouldBeTrue();
    }

    [Fact]
    public void ValidRegonAttribute_IsValid_ValidRegon9String_ReturnsTrue()
    {
        new ValidRegonAttribute().IsValid(ValidRegon9).ShouldBeTrue();
    }

    [Fact]
    public void ValidRegonAttribute_IsValid_ValidRegon14String_ReturnsTrue()
    {
        new ValidRegonAttribute().IsValid(ValidRegon14).ShouldBeTrue();
    }

    [Fact]
    public void ValidRegonAttribute_IsValid_InvalidRegonString_ReturnsFalse()
    {
        new ValidRegonAttribute().IsValid(InvalidRegon).ShouldBeFalse();
    }

    [Fact]
    public void ValidRegonAttribute_IsValid_ParsedRegonStruct_ReturnsTrue()
    {
        new ValidRegonAttribute().IsValid(Regon.Parse(ValidRegon9)).ShouldBeTrue();
    }

    [Fact]
    public void ValidRegonAttribute_IsValid_DefaultRegonStruct_ReturnsFalse()
    {
        new ValidRegonAttribute().IsValid(default(Regon)).ShouldBeFalse();
    }

    [Fact]
    public void ValidRegonAttribute_IsValid_UnsupportedType_ReturnsFalse()
    {
        new ValidRegonAttribute().IsValid(12345).ShouldBeFalse();
    }

    // ── GetValidationResult with custom display name ──────────────────────────

    [Fact]
    public void ValidRegonAttribute_GetValidationResult_InvalidRegon_ReturnsError()
    {
        var attribute = new ValidRegonAttribute();
        var context = new ValidationContext(new RegonStringDto()) { DisplayName = "Tax REGON" };

        var result = attribute.GetValidationResult(InvalidRegon, context);

        result.ShouldNotBeNull();
        result!.ErrorMessage!.ShouldContain("Tax REGON");
    }

    [Fact]
    public void ValidRegonAttribute_GetValidationResult_ValidRegon_ReturnsSuccess()
    {
        var attribute = new ValidRegonAttribute();
        var context = new ValidationContext(new RegonStringDto());

        var result = attribute.GetValidationResult(ValidRegon9, context);

        result.ShouldBe(DataAnnotationsValidationResult.Success);
    }
}
