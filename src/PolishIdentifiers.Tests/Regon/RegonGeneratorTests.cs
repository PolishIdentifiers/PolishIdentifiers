using PolishIdentifiers;
using Shouldly;

namespace PolishIdentifiers.Tests;

public class RegonGeneratorTests
{
    // --- Generate(RegonKind.Regon9) ---

    [Fact]
    public void Generate_WithUnsupportedKind_ThrowsArgumentOutOfRangeException()
    {
        var invalidKind = (RegonKind)999;

        Should.Throw<ArgumentOutOfRangeException>(() => RegonGenerator.Generate(invalidKind));
    }

    [Fact]
    public void Generate_WithRegon9Kind_ReturnsParsableRegon9()
    {
        var regon = RegonGenerator.Generate(RegonKind.Regon9);

        Regon.Validate(regon.ToString()).IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Generate_WithRegon9Kind_ReturnsRegon9Kind()
    {
        var regon = RegonGenerator.Generate(RegonKind.Regon9);

        regon.Kind.ShouldBe(RegonKind.Regon9);
        regon.IsRegon9.ShouldBeTrue();
    }

    [Fact]
    public void Generate_WithRegon9Kind_ToString_Has9Digits()
    {
        var regon = RegonGenerator.Generate(RegonKind.Regon9);

        regon.ToString().Length.ShouldBe(9);
    }

    [Fact]
    public void Generate_WithRegon9Kind_CalledMultipleTimes_ReturnsOnlyValidValues()
    {
        var results = Enumerable.Range(0, 100).Select(_ => RegonGenerator.Generate(RegonKind.Regon9).ToString()).ToList();

        results.ShouldAllBe(value => Regon.Validate(value).IsValid);
    }

    // --- Generate(RegonKind.Regon14) ---

    [Fact]
    public void Generate_WithRegon14Kind_ReturnsParsableRegon14()
    {
        var regon = RegonGenerator.Generate(RegonKind.Regon14);

        Regon.Validate(regon.ToString()).IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Generate_WithRegon14Kind_ReturnsRegon14Kind()
    {
        var regon = RegonGenerator.Generate(RegonKind.Regon14);

        regon.Kind.ShouldBe(RegonKind.Regon14);
        regon.IsRegon14.ShouldBeTrue();
    }

    [Fact]
    public void Generate_WithRegon14Kind_ToString_Has14Digits()
    {
        var regon = RegonGenerator.Generate(RegonKind.Regon14);

        regon.ToString().Length.ShouldBe(14);
    }

    [Fact]
    public void Generate_WithRegon14Kind_CalledMultipleTimes_ReturnsOnlyValidValues()
    {
        var results = Enumerable.Range(0, 50).Select(_ => RegonGenerator.Generate(RegonKind.Regon14).ToString()).ToList();

        results.ShouldAllBe(value => Regon.Validate(value).IsValid);
    }

    [Fact]
    public void Generate_WithRegon14Kind_BaseRegon_IsValidRegon9()
    {
        var regon14 = RegonGenerator.Generate(RegonKind.Regon14);

        var base9 = regon14.BaseRegon9;
        Regon.Validate(base9.ToString()).IsValid.ShouldBeTrue();
        base9.IsRegon9.ShouldBeTrue();
    }

    [Fact]
    public void Generate_WithRegon14Kind_FirstNineDigitsMatchBaseRegon9()
    {
        var regon14 = RegonGenerator.Generate(RegonKind.Regon14);

        var base9FromProp = regon14.BaseRegon9.ToString();
        var base9FromStr = regon14.ToString().Substring(0, 9);

        base9FromProp.ShouldBe(base9FromStr);
    }

    // --- Invalid generators ---

    [Fact]
    public void Invalid_WrongChecksumRegon9_YieldsExactlyInvalidChecksum()
    {
        var s = RegonGenerator.Invalid.WrongChecksumRegon9();

        Regon.Validate(s).Error.ShouldBe(RegonValidationError.InvalidChecksum);
    }

    [Fact]
    public void Invalid_WrongLength_YieldsExactlyInvalidLength()
    {
        var s = RegonGenerator.Invalid.WrongLength();

        Regon.Validate(s).Error.ShouldBe(RegonValidationError.InvalidLength);
    }

    [Fact]
    public void Invalid_NonNumeric_YieldsExactlyInvalidCharacters()
    {
        var s = RegonGenerator.Invalid.NonNumeric();

        Regon.Validate(s).Error.ShouldBe(RegonValidationError.InvalidCharacters);
    }

    // --- WrongChecksumRegon9: structural verification ---

    [Fact]
    public void Invalid_WrongChecksumRegon9_HasCorrectLengthAndDigits()
    {
        var s = RegonGenerator.Invalid.WrongChecksumRegon9();

        s.Length.ShouldBe(9);
        s.ShouldAllBe(c => char.IsDigit(c));
    }

    [Fact]
    public void Invalid_WrongChecksumRegon9_CalledMultipleTimes_AlwaysYieldsInvalidChecksum()
    {
        var results = Enumerable.Range(0, 50).Select(_ => RegonGenerator.Invalid.WrongChecksumRegon9()).ToList();

        results.ShouldAllBe(s => Regon.Validate(s).Error == RegonValidationError.InvalidChecksum);
    }

    // --- WrongChecksumRegon14: functional and structural verification ---

    [Fact]
    public void Invalid_WrongChecksumRegon14_YieldsExactlyInvalidChecksum()
    {
        var s = RegonGenerator.Invalid.WrongChecksumRegon14();

        Regon.Validate(s).Error.ShouldBe(RegonValidationError.InvalidChecksum);
    }

    [Fact]
    public void Invalid_WrongChecksumRegon14_HasLength14AndAllDigits()
    {
        var s = RegonGenerator.Invalid.WrongChecksumRegon14();

        s.Length.ShouldBe(14);
        s.ShouldAllBe(c => char.IsDigit(c));
    }

    [Fact]
    public void Invalid_WrongChecksumRegon14_EmbeddedBase9IsValid()
    {
        var base9 = RegonGenerator.Invalid.WrongChecksumRegon14().Substring(0, 9);

        Regon.Validate(base9).IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Invalid_WrongChecksumRegon14_CalledMultipleTimes_AlwaysYieldsInvalidChecksum()
    {
        var results = Enumerable.Range(0, 50).Select(_ => RegonGenerator.Invalid.WrongChecksumRegon14()).ToList();

        results.ShouldAllBe(s => Regon.Validate(s).Error == RegonValidationError.InvalidChecksum);
    }

    // --- WrongLength: structural verification ---

    [Fact]
    public void Invalid_WrongLength_HasNeitherLength9Nor14()
    {
        var s = RegonGenerator.Invalid.WrongLength();

        s.Length.ShouldNotBe(9);
        s.Length.ShouldNotBe(14);
    }

    [Fact]
    public void Invalid_WrongLength_ContainsOnlyDigits()
    {
        var value = RegonGenerator.Invalid.WrongLength();

        value.ShouldAllBe(c => char.IsDigit(c));
    }

    // --- NonNumeric: structural verification ---

    [Fact]
    public void Invalid_NonNumeric_HasLength9()
    {
        var s = RegonGenerator.Invalid.NonNumeric();

        s.Length.ShouldBe(9);
    }

    [Fact]
    public void Invalid_NonNumeric_ContainsAtLeastOneNonDigit()
    {
        var s = RegonGenerator.Invalid.NonNumeric();

        s.Any(c => c < '0' || c > '9').ShouldBeTrue();
    }

    [Fact]
    public void Invalid_NonNumeric_ContainsExactlyOneNonDigit()
    {
        var s = RegonGenerator.Invalid.NonNumeric();

        s.Count(c => c < '0' || c > '9').ShouldBe(1);
    }
}
