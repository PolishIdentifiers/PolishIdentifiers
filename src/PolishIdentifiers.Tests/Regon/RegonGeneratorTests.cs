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
    public void Generate_WithRegon14Kind_BaseRegon9_IsValidRegon9()
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
        var invalidValue = RegonGenerator.Invalid.WrongChecksumRegon9();

        Regon.Validate(invalidValue).Error.ShouldBe(RegonValidationError.InvalidChecksum);
    }

    [Fact]
    public void Invalid_WrongLength_YieldsExactlyInvalidLength()
    {
        var invalidValue = RegonGenerator.Invalid.WrongLength();

        Regon.Validate(invalidValue).Error.ShouldBe(RegonValidationError.InvalidLength);
    }

    [Fact]
    public void Invalid_WrongLength_CalledMultipleTimes_AlwaysYieldsInvalidLength()
    {
        var results = Enumerable.Range(0, 50).Select(_ => RegonGenerator.Invalid.WrongLength()).ToList();

        results.ShouldAllBe(s => Regon.Validate(s).Error == RegonValidationError.InvalidLength);
    }

    [Fact]
    public void Invalid_NonNumeric_YieldsExactlyInvalidCharacters()
    {
        var invalidValue = RegonGenerator.Invalid.NonNumeric();

        Regon.Validate(invalidValue).Error.ShouldBe(RegonValidationError.InvalidCharacters);
    }

    [Fact]
    public void Invalid_NonNumeric_CalledMultipleTimes_AlwaysYieldsInvalidCharacters()
    {
        var results = Enumerable.Range(0, 50).Select(_ => RegonGenerator.Invalid.NonNumeric()).ToList();

        results.ShouldAllBe(s => Regon.Validate(s).Error == RegonValidationError.InvalidCharacters);
    }

    // --- WrongChecksumRegon9: structural verification ---

    [Fact]
    public void Invalid_WrongChecksumRegon9_HasCorrectLengthAndDigits()
    {
        var invalidValue = RegonGenerator.Invalid.WrongChecksumRegon9();

        invalidValue.Length.ShouldBe(9);
        invalidValue.ShouldAllBe(c => char.IsDigit(c));
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
        var invalidValue = RegonGenerator.Invalid.WrongChecksumRegon14();

        Regon.Validate(invalidValue).Error.ShouldBe(RegonValidationError.InvalidChecksum);
    }

    [Fact]
    public void Invalid_WrongChecksumRegon14_HasLength14AndAllDigits()
    {
        var invalidValue = RegonGenerator.Invalid.WrongChecksumRegon14();

        invalidValue.Length.ShouldBe(14);
        invalidValue.ShouldAllBe(c => char.IsDigit(c));
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
        var invalidValue = RegonGenerator.Invalid.WrongLength();

        invalidValue.Length.ShouldNotBe(9);
        invalidValue.Length.ShouldNotBe(14);
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
        var invalidValue = RegonGenerator.Invalid.NonNumeric();

        invalidValue.Length.ShouldBe(9);
    }

    [Fact]
    public void Invalid_NonNumeric_ContainsAtLeastOneNonDigit()
    {
        var invalidValue = RegonGenerator.Invalid.NonNumeric();

        invalidValue.Any(c => c < '0' || c > '9').ShouldBeTrue();
    }

    [Fact]
    public void Invalid_NonNumeric_ContainsExactlyOneNonDigit()
    {
        var invalidValue = RegonGenerator.Invalid.NonNumeric();

        invalidValue.Count(c => c < '0' || c > '9').ShouldBe(1);
    }

    // --- Generate(RegonKind, int count) ---

    [Fact]
    public void Generate_WithRegon9KindAndCount_ReturnsExactCount()
    {
        var results = RegonGenerator.Generate(RegonKind.Regon9, count: 10);

        results.Count.ShouldBe(10);
    }

    [Fact]
    public void Generate_WithRegon14KindAndCount_ReturnsExactCount()
    {
        var results = RegonGenerator.Generate(RegonKind.Regon14, count: 10);

        results.Count.ShouldBe(10);
    }

    [Fact]
    public void Generate_WithRegon9KindAndCount_AllElementsAreValid()
    {
        var results = RegonGenerator.Generate(RegonKind.Regon9, count: 20);

        results.ShouldAllBe(r => Regon.Validate(r.ToString()).IsValid);
    }

    [Fact]
    public void Generate_WithRegon14KindAndCount_AllElementsAreValid()
    {
        var results = RegonGenerator.Generate(RegonKind.Regon14, count: 20);

        results.ShouldAllBe(r => Regon.Validate(r.ToString()).IsValid);
    }

    [Fact]
    public void Generate_WithRegon9KindAndCount_AllElementsHaveRegon9Kind()
    {
        var results = RegonGenerator.Generate(RegonKind.Regon9, count: 10);

        results.ShouldAllBe(r => r.Kind == RegonKind.Regon9);
    }

    [Fact]
    public void Generate_WithRegon14KindAndCount_AllElementsHaveRegon14Kind()
    {
        var results = RegonGenerator.Generate(RegonKind.Regon14, count: 10);

        results.ShouldAllBe(r => r.Kind == RegonKind.Regon14);
    }

    [Fact]
    public void Generate_WithKindAndCount0_ReturnsEmpty()
    {
        RegonGenerator.Generate(RegonKind.Regon9, count: 0).ShouldBeEmpty();
    }

    [Fact]
    public void Generate_WithKindAndNegativeCount_ThrowsArgumentOutOfRangeException()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => RegonGenerator.Generate(RegonKind.Regon9, count: -1));
    }

    [Fact]
    public void Generate_WithUnsupportedKindAndAnyCount_ThrowsEagerly()
    {
        var invalidKind = (RegonKind)999;

        Should.Throw<ArgumentOutOfRangeException>(() => RegonGenerator.Generate(invalidKind, count: 5));
    }

    // --- Invalid.WrongChecksumRegon9(int count) ---

    [Fact]
    public void Invalid_WrongChecksumRegon9_WithCount_AllYieldInvalidChecksum()
    {
        var results = RegonGenerator.Invalid.WrongChecksumRegon9(count: 20);

        results.Count.ShouldBe(20);
        results.ShouldAllBe(s => Regon.Validate(s).Error == RegonValidationError.InvalidChecksum);
    }

    [Fact]
    public void Invalid_WrongChecksumRegon9_WithCount0_ReturnsEmpty()
    {
        RegonGenerator.Invalid.WrongChecksumRegon9(count: 0).ShouldBeEmpty();
    }

    [Fact]
    public void Invalid_WrongChecksumRegon9_WithNegativeCount_ThrowsArgumentOutOfRangeException()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => RegonGenerator.Invalid.WrongChecksumRegon9(count: -1));
    }

    // --- Invalid.WrongChecksumRegon14(int count) ---

    [Fact]
    public void Invalid_WrongChecksumRegon14_WithCount_AllYieldInvalidChecksum()
    {
        var results = RegonGenerator.Invalid.WrongChecksumRegon14(count: 20);

        results.Count.ShouldBe(20);
        results.ShouldAllBe(s => Regon.Validate(s).Error == RegonValidationError.InvalidChecksum);
    }

    [Fact]
    public void Invalid_WrongChecksumRegon14_WithCount0_ReturnsEmpty()
    {
        RegonGenerator.Invalid.WrongChecksumRegon14(count: 0).ShouldBeEmpty();
    }

    [Fact]
    public void Invalid_WrongChecksumRegon14_WithNegativeCount_ThrowsArgumentOutOfRangeException()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => RegonGenerator.Invalid.WrongChecksumRegon14(count: -1));
    }

    // --- Invalid.WrongLength(int count) ---

    [Fact]
    public void Invalid_WrongLength_WithCount_AllYieldInvalidLength()
    {
        var results = RegonGenerator.Invalid.WrongLength(count: 20);

        results.Count.ShouldBe(20);
        results.ShouldAllBe(s => Regon.Validate(s).Error == RegonValidationError.InvalidLength);
    }

    [Fact]
    public void Invalid_WrongLength_WithCount0_ReturnsEmpty()
    {
        RegonGenerator.Invalid.WrongLength(count: 0).ShouldBeEmpty();
    }

    [Fact]
    public void Invalid_WrongLength_WithNegativeCount_ThrowsArgumentOutOfRangeException()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => RegonGenerator.Invalid.WrongLength(count: -1));
    }

    // --- Invalid.NonNumeric(int count) ---

    [Fact]
    public void Invalid_NonNumeric_WithCount_AllYieldInvalidCharacters()
    {
        var results = RegonGenerator.Invalid.NonNumeric(count: 20);

        results.Count.ShouldBe(20);
        results.ShouldAllBe(s => Regon.Validate(s).Error == RegonValidationError.InvalidCharacters);
    }

    [Fact]
    public void Invalid_NonNumeric_WithCount0_ReturnsEmpty()
    {
        RegonGenerator.Invalid.NonNumeric(count: 0).ShouldBeEmpty();
    }

    [Fact]
    public void Invalid_NonNumeric_WithNegativeCount_ThrowsArgumentOutOfRangeException()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => RegonGenerator.Invalid.NonNumeric(count: -1));
    }
}
