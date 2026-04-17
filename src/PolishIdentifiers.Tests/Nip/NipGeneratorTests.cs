using PolishIdentifiers;
using Shouldly;

namespace PolishIdentifiers.Tests;

public class NipGeneratorTests
{
    // --- Generate() ---

    [Fact]
    public void Generate_ReturnsParsableNip()
    {
        var nip = NipGenerator.Generate();

        Nip.Validate(nip.ToString()).IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Generate_CalledMultipleTimes_ReturnsOnlyValidValues()
    {
        var results = Enumerable.Range(0, 100).Select(_ => NipGenerator.Generate().ToString()).ToList();

        results.ShouldAllBe(value => Nip.Validate(value).IsValid);
    }

    // --- Invalid generators ---

    [Fact]
    public void Invalid_WrongChecksum_YieldsExactlyInvalidChecksum()
    {
        var invalidValue = NipGenerator.Invalid.WrongChecksum();

        Nip.Validate(invalidValue).Error.ShouldBe(NipValidationError.InvalidChecksum);
    }

    [Fact]
    public void Invalid_WrongLength_YieldsExactlyInvalidLength()
    {
        var invalidValue = NipGenerator.Invalid.WrongLength();

        Nip.Validate(invalidValue).Error.ShouldBe(NipValidationError.InvalidLength);
    }

    [Fact]
    public void Invalid_WrongLength_CalledMultipleTimes_AlwaysYieldsInvalidLength()
    {
        var results = Enumerable.Range(0, 50).Select(_ => NipGenerator.Invalid.WrongLength()).ToList();

        results.ShouldAllBe(s => Nip.Validate(s).Error == NipValidationError.InvalidLength);
    }

    [Fact]
    public void Invalid_NonNumeric_YieldsExactlyInvalidCharacters()
    {
        var invalidValue = NipGenerator.Invalid.NonNumeric();

        Nip.Validate(invalidValue).Error.ShouldBe(NipValidationError.InvalidCharacters);
    }

    [Fact]
    public void Invalid_NonNumeric_CalledMultipleTimes_AlwaysYieldsInvalidCharacters()
    {
        var results = Enumerable.Range(0, 50).Select(_ => NipGenerator.Invalid.NonNumeric()).ToList();

        results.ShouldAllBe(s => Nip.Validate(s).Error == NipValidationError.InvalidCharacters);
    }

    // --- WrongChecksum: structural verification ---

    [Fact]
    public void Invalid_WrongChecksum_HasCorrectLengthAndDigits()
    {
        var invalidValue = NipGenerator.Invalid.WrongChecksum();

        invalidValue.Length.ShouldBe(10);
        invalidValue.ShouldAllBe(c => c >= '0' && c <= '9');
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
        var invalidValue = NipGenerator.Invalid.WrongLength();

        invalidValue.Length.ShouldNotBe(10);
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
        var invalidValue = NipGenerator.Invalid.NonNumeric();

        invalidValue.Length.ShouldBe(10);
    }

    [Fact]
    public void Invalid_NonNumeric_ContainsAtLeastOneNonDigit()
    {
        var invalidValue = NipGenerator.Invalid.NonNumeric();

        invalidValue.Any(c => c < '0' || c > '9').ShouldBeTrue();
    }

    // --- UnrecognizedFormat ---

    [Fact]
    public void Invalid_UnrecognizedFormat_YieldsExactlyUnrecognizedFormat()
    {
        var invalidValue = NipGenerator.Invalid.UnrecognizedFormat();

        Nip.Validate(invalidValue).Error.ShouldBe(NipValidationError.UnrecognizedFormat);
    }

    [Fact]
    public void Invalid_UnrecognizedFormat_CalledMultipleTimes_AlwaysYieldsUnrecognizedFormat()
    {
        var results = Enumerable.Range(0, 50).Select(_ => NipGenerator.Invalid.UnrecognizedFormat()).ToList();

        results.ShouldAllBe(s => Nip.Validate(s).Error == NipValidationError.UnrecognizedFormat);
    }

    [Fact]
    public void Invalid_UnrecognizedFormat_ContainsOnlyValidNipCharacters()
    {
        var invalidValue = NipGenerator.Invalid.UnrecognizedFormat();

        invalidValue.ShouldAllBe(c =>
            (c >= '0' && c <= '9') || c == 'P' || c == 'L' || c == ' ' || c == '-');
    }

    [Fact]
    public void Invalid_UnrecognizedFormat_EmbeddedDigitsAreValidNip()
    {
        var invalidValue = NipGenerator.Invalid.UnrecognizedFormat();

        var embedded = invalidValue.Substring(3);
        Nip.Validate(embedded).IsValid.ShouldBeTrue();
    }

    // --- WrongChecksum: wrap-around ---

    [Fact]
    public void Invalid_WrongChecksum_WhenBaseCheckDigitIs9_WrapsToZeroAndYieldsInvalidChecksum()
    {
        // 0123456789: weighted sum = 185, 185 % 11 = 9, so check digit is 9.
        // Applying WrongChecksum logic: (9 + 1) % 10 = 0 → produces "0123456780".
        // The validator must still report InvalidChecksum, not any earlier error.
        const string NipWithWrappedCheckDigit = "0123456780";

        Nip.Validate(NipWithWrappedCheckDigit).Error.ShouldBe(NipValidationError.InvalidChecksum);
    }

    // --- Generate(int count) ---

    [Fact]
    public void Generate_WithCount_ReturnsExactCount()
    {
        var results = NipGenerator.Generate(count: 10);

        results.Count.ShouldBe(10);
    }

    [Fact]
    public void Generate_WithCount_AllElementsAreValid()
    {
        var results = NipGenerator.Generate(count: 20);

        results.ShouldAllBe(n => Nip.Validate(n.ToString()).IsValid);
    }

    [Fact]
    public void Generate_WithCount0_ReturnsEmpty()
    {
        NipGenerator.Generate(count: 0).ShouldBeEmpty();
    }

    [Fact]
    public void Generate_WithNegativeCount_ThrowsArgumentOutOfRangeException()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => NipGenerator.Generate(count: -1));
    }

    // --- Invalid.WrongChecksum(int count) ---

    [Fact]
    public void Invalid_WrongChecksum_WithCount_AllYieldInvalidChecksum()
    {
        var results = NipGenerator.Invalid.WrongChecksum(count: 20);

        results.Count.ShouldBe(20);
        results.ShouldAllBe(s => Nip.Validate(s).Error == NipValidationError.InvalidChecksum);
    }

    [Fact]
    public void Invalid_WrongChecksum_WithCount0_ReturnsEmpty()
    {
        NipGenerator.Invalid.WrongChecksum(count: 0).ShouldBeEmpty();
    }

    [Fact]
    public void Invalid_WrongChecksum_WithNegativeCount_ThrowsArgumentOutOfRangeException()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => NipGenerator.Invalid.WrongChecksum(count: -1));
    }

    // --- Invalid.WrongLength(int count) ---

    [Fact]
    public void Invalid_WrongLength_WithCount_AllYieldInvalidLength()
    {
        var results = NipGenerator.Invalid.WrongLength(count: 20);

        results.Count.ShouldBe(20);
        results.ShouldAllBe(s => Nip.Validate(s).Error == NipValidationError.InvalidLength);
    }

    [Fact]
    public void Invalid_WrongLength_WithCount0_ReturnsEmpty()
    {
        NipGenerator.Invalid.WrongLength(count: 0).ShouldBeEmpty();
    }

    [Fact]
    public void Invalid_WrongLength_WithNegativeCount_ThrowsArgumentOutOfRangeException()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => NipGenerator.Invalid.WrongLength(count: -1));
    }

    // --- Invalid.NonNumeric(int count) ---

    [Fact]
    public void Invalid_NonNumeric_WithCount_AllYieldInvalidCharacters()
    {
        var results = NipGenerator.Invalid.NonNumeric(count: 20);

        results.Count.ShouldBe(20);
        results.ShouldAllBe(s => Nip.Validate(s).Error == NipValidationError.InvalidCharacters);
    }

    [Fact]
    public void Invalid_NonNumeric_WithCount0_ReturnsEmpty()
    {
        NipGenerator.Invalid.NonNumeric(count: 0).ShouldBeEmpty();
    }

    [Fact]
    public void Invalid_NonNumeric_WithNegativeCount_ThrowsArgumentOutOfRangeException()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => NipGenerator.Invalid.NonNumeric(count: -1));
    }

    // --- Invalid.UnrecognizedFormat(int count) ---

    [Fact]
    public void Invalid_UnrecognizedFormat_WithCount_AllYieldUnrecognizedFormat()
    {
        var results = NipGenerator.Invalid.UnrecognizedFormat(count: 20);

        results.Count.ShouldBe(20);
        results.ShouldAllBe(s => Nip.Validate(s).Error == NipValidationError.UnrecognizedFormat);
    }

    [Fact]
    public void Invalid_UnrecognizedFormat_WithCount0_ReturnsEmpty()
    {
        NipGenerator.Invalid.UnrecognizedFormat(count: 0).ShouldBeEmpty();
    }

    [Fact]
    public void Invalid_UnrecognizedFormat_WithNegativeCount_ThrowsArgumentOutOfRangeException()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => NipGenerator.Invalid.UnrecognizedFormat(count: -1));
    }
}
