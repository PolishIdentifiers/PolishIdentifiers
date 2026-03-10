using PolishIdentifiers;
using Shouldly;

namespace PolishIdentifiers.Tests;

public class NipGeneratorTests
{
    // --- Random() ---

    [Fact]
    public void Random_ReturnsParsableNip()
    {
        var nip = NipGenerator.Random();

        Assert.True(Nip.Validate(nip.ToString()).IsValid);
    }

    [Fact]
    public void Random_CalledMultipleTimes_ReturnsOnlyValidValues()
    {
        var results = Enumerable.Range(0, 100).Select(_ => NipGenerator.Random().ToString()).ToList();

        Assert.All(results, value => Assert.True(Nip.Validate(value).IsValid));
    }

    [Fact]
    public void Random_CalledManyTimes_CanGenerateNipWithLeadingZero()
    {
        var generatedNips = Enumerable.Range(0, 500)
            .Select(_ => NipGenerator.Random().ToString())
            .ToList();

        generatedNips.Any(value => value[0] == '0').ShouldBeTrue();
    }

    // --- Invalid generators ---

    [Fact]
    public void Invalid_WrongChecksum_YieldsExactlyInvalidChecksum()
    {
        var s = NipGenerator.Invalid.WrongChecksum();

        Assert.Equal(NipValidationError.InvalidChecksum, Nip.Validate(s).Error);
    }

    [Fact]
    public void Invalid_WrongLength_YieldsExactlyInvalidLength()
    {
        var s = NipGenerator.Invalid.WrongLength();

        Assert.Equal(NipValidationError.InvalidLength, Nip.Validate(s).Error);
    }

    [Fact]
    public void Invalid_NonNumeric_YieldsExactlyInvalidCharacters()
    {
        var s = NipGenerator.Invalid.NonNumeric();

        Assert.Equal(NipValidationError.InvalidCharacters, Nip.Validate(s).Error);
    }

    // --- WrongChecksum: structural verification ---

    [Fact]
    public void Invalid_WrongChecksum_HasCorrectLengthAndDigits()
    {
        var s = NipGenerator.Invalid.WrongChecksum();

        Assert.Equal(10, s.Length);
        Assert.All(s, c => Assert.True(c >= '0' && c <= '9'));
    }

    [Fact]
    public void Invalid_WrongChecksum_CalledMultipleTimes_AlwaysYieldsInvalidChecksum()
    {
        var results = Enumerable.Range(0, 50).Select(_ => NipGenerator.Invalid.WrongChecksum()).ToList();

        Assert.All(results, s => Assert.Equal(NipValidationError.InvalidChecksum, Nip.Validate(s).Error));
    }

    // --- WrongLength: structural verification ---

    [Fact]
    public void Invalid_WrongLength_LengthIsNot10()
    {
        var s = NipGenerator.Invalid.WrongLength();

        Assert.NotEqual(10, s.Length);
    }

    // --- NonNumeric: structural verification ---

    [Fact]
    public void Invalid_NonNumeric_HasLength10()
    {
        var s = NipGenerator.Invalid.NonNumeric();

        Assert.Equal(10, s.Length);
    }

    [Fact]
    public void Invalid_NonNumeric_ContainsAtLeastOneNonDigit()
    {
        var s = NipGenerator.Invalid.NonNumeric();

        Assert.Contains(s, c => c < '0' || c > '9');
    }

    // --- Random stability: multiple calls produce valid, non-identical results ---

    [Fact]
    public void Random_CalledMultipleTimes_ProducesAtLeastTwoDistinctValues()
    {
        var results = Enumerable.Range(0, 20).Select(_ => NipGenerator.Random().ToString()).Distinct().ToList();

        Assert.True(results.Count >= 2);
    }
}
