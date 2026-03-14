using PolishIdentifiers;
using Shouldly;

namespace PolishIdentifiers.Tests;

public class RegonGeneratorTests
{
    // --- Random() ---

    [Fact]
    public void Random_ReturnsParsableRegon9()
    {
        var regon = RegonGenerator.Random();

        Regon.Validate(regon.ToString()).IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Random_ReturnsMainKind()
    {
        var regon = RegonGenerator.Random();

        regon.Kind.ShouldBe(RegonKind.Main);
        regon.IsMain.ShouldBeTrue();
    }

    [Fact]
    public void Random_ToString_Has9Digits()
    {
        var regon = RegonGenerator.Random();

        regon.ToString().Length.ShouldBe(9);
    }

    [Fact]
    public void Random_CalledMultipleTimes_ReturnsOnlyValidValues()
    {
        var results = Enumerable.Range(0, 100).Select(_ => RegonGenerator.Random().ToString()).ToList();

        results.ShouldAllBe(value => Regon.Validate(value).IsValid);
    }

    // --- RandomLocal() ---

    [Fact]
    public void RandomLocal_ReturnsParsableRegon14()
    {
        var regon = RegonGenerator.RandomLocal();

        Regon.Validate(regon.ToString()).IsValid.ShouldBeTrue();
    }

    [Fact]
    public void RandomLocal_ReturnsLocalKind()
    {
        var regon = RegonGenerator.RandomLocal();

        regon.Kind.ShouldBe(RegonKind.Local);
        regon.IsLocal.ShouldBeTrue();
    }

    [Fact]
    public void RandomLocal_ToString_Has14Digits()
    {
        var regon = RegonGenerator.RandomLocal();

        regon.ToString().Length.ShouldBe(14);
    }

    [Fact]
    public void RandomLocal_CalledMultipleTimes_ReturnsOnlyValidValues()
    {
        var results = Enumerable.Range(0, 50).Select(_ => RegonGenerator.RandomLocal().ToString()).ToList();

        results.ShouldAllBe(value => Regon.Validate(value).IsValid);
    }

    [Fact]
    public void RandomLocal_BaseRegon_IsValidRegon9()
    {
        var regon14 = RegonGenerator.RandomLocal();

        var base9 = regon14.BaseRegon;
        Regon.Validate(base9.ToString()).IsValid.ShouldBeTrue();
        base9.IsMain.ShouldBeTrue();
    }

    [Fact]
    public void RandomLocal_FirstNineDigitsMatchBaseRegon9()
    {
        var regon14 = RegonGenerator.RandomLocal();

        var base9FromProp = regon14.BaseRegon.ToString();
        var base9FromStr = regon14.ToString().Substring(0, 9);

        base9FromProp.ShouldBe(base9FromStr);
    }

    // --- Invalid generators ---

    [Fact]
    public void Invalid_WrongChecksum_YieldsExactlyInvalidChecksum()
    {
        var s = RegonGenerator.Invalid.WrongChecksum();

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

    // --- WrongChecksum: structural verification ---

    [Fact]
    public void Invalid_WrongChecksum_Regon9_HasCorrectLengthAndDigits()
    {
        var s = RegonGenerator.Invalid.WrongChecksum();

        s.Length.ShouldBe(9);
        s.ShouldAllBe(c => char.IsDigit(c));
    }

    [Fact]
    public void Invalid_WrongChecksum_CalledMultipleTimes_AlwaysYieldsInvalidChecksum()
    {
        var results = Enumerable.Range(0, 50).Select(_ => RegonGenerator.Invalid.WrongChecksum()).ToList();

        results.ShouldAllBe(s => Regon.Validate(s).Error == RegonValidationError.InvalidChecksum);
    }

    // --- WrongChecksum14: functional and structural verification ---

    [Fact]
    public void Invalid_WrongChecksum14_YieldsExactlyInvalidChecksum()
    {
        var s = RegonGenerator.Invalid.WrongChecksum14();

        Regon.Validate(s).Error.ShouldBe(RegonValidationError.InvalidChecksum);
    }

    [Fact]
    public void Invalid_WrongChecksum14_HasLength14AndAllDigits()
    {
        var s = RegonGenerator.Invalid.WrongChecksum14();

        s.Length.ShouldBe(14);
        s.ShouldAllBe(c => char.IsDigit(c));
    }

    [Fact]
    public void Invalid_WrongChecksum14_EmbeddedBase9IsValid()
    {
        var base9 = RegonGenerator.Invalid.WrongChecksum14().Substring(0, 9);

        Regon.Validate(base9).IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Invalid_WrongChecksum14_CalledMultipleTimes_AlwaysYieldsInvalidChecksum()
    {
        var results = Enumerable.Range(0, 50).Select(_ => RegonGenerator.Invalid.WrongChecksum14()).ToList();

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
