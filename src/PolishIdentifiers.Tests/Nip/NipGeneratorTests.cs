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

        Nip.Validate(nip.ToString()).IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Random_CalledMultipleTimes_ReturnsOnlyValidValues()
    {
        var results = Enumerable.Range(0, 100).Select(_ => NipGenerator.Random().ToString()).ToList();

        results.ShouldAllBe(value => Nip.Validate(value).IsValid);
    }

    // --- Invalid generators ---

    [Fact]
    public void Invalid_WrongChecksum_YieldsExactlyInvalidChecksum()
    {
        var s = NipGenerator.Invalid.WrongChecksum();

        Nip.Validate(s).Error.ShouldBe(NipValidationError.InvalidChecksum);
    }

    [Fact]
    public void Invalid_WrongLength_YieldsExactlyInvalidLength()
    {
        var s = NipGenerator.Invalid.WrongLength();

        Nip.Validate(s).Error.ShouldBe(NipValidationError.InvalidLength);
    }

    [Fact]
    public void Invalid_NonNumeric_YieldsExactlyInvalidCharacters()
    {
        var s = NipGenerator.Invalid.NonNumeric();

        Nip.Validate(s).Error.ShouldBe(NipValidationError.InvalidCharacters);
    }

    // --- WrongChecksum: structural verification ---

    [Fact]
    public void Invalid_WrongChecksum_HasCorrectLengthAndDigits()
    {
        var s = NipGenerator.Invalid.WrongChecksum();

        s.Length.ShouldBe(10);
        s.ShouldAllBe(c => c >= '0' && c <= '9');
    }

    [Fact]
    public void Invalid_WrongChecksum_CalledMultipleTimes_AlwaysYieldsInvalidChecksum()
    {
        var results = Enumerable.Range(0, 50).Select(_ => NipGenerator.Invalid.WrongChecksum()).ToList();

        results.ShouldAllBe(s => Nip.Validate(s).Error == NipValidationError.InvalidChecksum);
    }

    // --- WrongLength: structural verification ---

    [Fact]
    public void Invalid_WrongLength_HasInvalidLength()
    {
        var s = NipGenerator.Invalid.WrongLength();

        s.Length.ShouldNotBe(10);
    }

    [Fact]
    public void Invalid_WrongLength_ReturnsOnlyDigits()
    {
        var value = NipGenerator.Invalid.WrongLength();

        value.ShouldAllBe(c => char.IsDigit(c));
    }

    // --- NonNumeric: structural verification ---

    [Fact]
    public void Invalid_NonNumeric_HasLength10()
    {
        var s = NipGenerator.Invalid.NonNumeric();

        s.Length.ShouldBe(10);
    }

    [Fact]
    public void Invalid_NonNumeric_ContainsAtLeastOneNonDigit()
    {
        var s = NipGenerator.Invalid.NonNumeric();

        s.Any(c => c < '0' || c > '9').ShouldBeTrue();
    }
}
