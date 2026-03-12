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

        Assert.All(results, value => Assert.True(Regon.Validate(value).IsValid));
    }

    [Fact]
    public void Random_CalledMultipleTimes_ProducesAtLeastTwoDistinctValues()
    {
        var results = Enumerable.Range(0, 20).Select(_ => RegonGenerator.Random().ToString()).Distinct().ToList();

        results.Count.ShouldBeGreaterThanOrEqualTo(2);
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

        Assert.All(results, value => Assert.True(Regon.Validate(value).IsValid));
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

        Assert.Equal(RegonValidationError.InvalidChecksum, Regon.Validate(s).Error);
    }

    [Fact]
    public void Invalid_WrongLength_YieldsExactlyInvalidLength()
    {
        var s = RegonGenerator.Invalid.WrongLength();

        Assert.Equal(RegonValidationError.InvalidLength, Regon.Validate(s).Error);
    }

    [Fact]
    public void Invalid_NonNumeric_YieldsExactlyInvalidCharacters()
    {
        var s = RegonGenerator.Invalid.NonNumeric();

        Assert.Equal(RegonValidationError.InvalidCharacters, Regon.Validate(s).Error);
    }

    // --- WrongChecksum: structural verification ---

    [Fact]
    public void Invalid_WrongChecksum_HasCorrectLengthAndDigits()
    {
        var s = RegonGenerator.Invalid.WrongChecksum();

        s.Length.ShouldBe(9);
        Assert.All(s, c => Assert.True(c >= '0' && c <= '9'));
    }

    [Fact]
    public void Invalid_WrongChecksum_CalledMultipleTimes_AlwaysYieldsInvalidChecksum()
    {
        var results = Enumerable.Range(0, 50).Select(_ => RegonGenerator.Invalid.WrongChecksum()).ToList();

        Assert.All(results, s => Assert.Equal(RegonValidationError.InvalidChecksum, Regon.Validate(s).Error));
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

        value.All(char.IsDigit).ShouldBeTrue();
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

        Assert.Contains(s, c => c < '0' || c > '9');
    }

    [Fact]
    public void Invalid_NonNumeric_CalledMultipleTimes_PositionVaries()
    {
        // Non-digit appears at a random position; over many calls the position should vary.
        var positions = Enumerable.Range(0, 100)
            .Select(_ => RegonGenerator.Invalid.NonNumeric())
            .Select(s => s.IndexOf('X'))
            .Distinct()
            .ToList();

        positions.Count.ShouldBeGreaterThan(1);
    }
}
