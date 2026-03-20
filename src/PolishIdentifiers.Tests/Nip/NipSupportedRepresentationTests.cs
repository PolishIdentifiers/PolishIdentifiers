using PolishIdentifiers;
using Shouldly;

namespace PolishIdentifiers.Tests;

public class NipSupportedRepresentationTests
{
    private const string ValidNip = "1234563218";
    private const string ValidHyphenated = "123-456-32-18";
    private const string ValidPlPrefix = "PL1234563218";
    private const string ValidPlSpacePrefix = "PL 1234563218";
    private const string ValidPlSpaceHyphenated = "PL 123-456-32-18";

    private const string AnotherValidNip = "7680002466";
    private const string AnotherValidHyphenated = "768-000-24-66";
    private const string AnotherValidPlPrefix = "PL7680002466";
    private const string AnotherValidPlSpacePrefix = "PL 7680002466";
    private const string AnotherValidPlSpaceHyphenated = "PL 768-000-24-66";

    public static TheoryData<string> SupportedTextRepresentationData => new()
    {
        ValidNip,
        ValidHyphenated,
        ValidPlPrefix,
        ValidPlSpacePrefix,
        ValidPlSpaceHyphenated,
        AnotherValidNip,
        AnotherValidHyphenated,
        AnotherValidPlPrefix,
        AnotherValidPlSpacePrefix,
        AnotherValidPlSpaceHyphenated,
    };

    public static TheoryData<string, string> SupportedTextRepresentationCanonicalData => new()
    {
        { ValidNip, ValidNip },
        { ValidHyphenated, ValidNip },
        { ValidPlPrefix, ValidNip },
        { ValidPlSpacePrefix, ValidNip },
        { ValidPlSpaceHyphenated, ValidNip },
        { AnotherValidNip, AnotherValidNip },
        { AnotherValidHyphenated, AnotherValidNip },
        { AnotherValidPlPrefix, AnotherValidNip },
        { AnotherValidPlSpacePrefix, AnotherValidNip },
        { AnotherValidPlSpaceHyphenated, AnotherValidNip },
    };

    public static TheoryData<string> UnsupportedFormattedLayoutData => new()
    {
        "PL-1234563218",
        "PL123-456-32-18",
        "12-345-632-18",
        "123 456 32 18",
        "12345 3218",
        "PL  1234563218",
        " PL1234563218",
        "PL1234563218 ",
        "123-456-3218",
        "123-456-32-18-",
    };

    public static TheoryData<string> InvalidCharactersData => new()
    {
        "pl1234563218",
        "pl 1234563218",
        "DE1234563218",
    };

    [Theory]
    [MemberData(nameof(SupportedTextRepresentationData))]
    public void Validate_SupportedTextRepresentation_ReturnsValid(string input)
    {
        var result = Nip.Validate(input);

        result.IsValid.ShouldBeTrue();
        result.Error.ShouldBeNull();
    }

    [Theory]
    [MemberData(nameof(UnsupportedFormattedLayoutData))]
    public void Validate_UnsupportedFormattedLayout_ReturnsUnrecognizedFormat(string input)
    {
        var result = Nip.Validate(input);

        result.IsValid.ShouldBeFalse();
        result.Error.ShouldBe(NipValidationError.UnrecognizedFormat);
    }

    [Theory]
    [MemberData(nameof(InvalidCharactersData))]
    public void Validate_UnsupportedCharacters_ReturnsInvalidCharacters(string input)
    {
        var result = Nip.Validate(input);

        result.IsValid.ShouldBeFalse();
        result.Error.ShouldBe(NipValidationError.InvalidCharacters);
    }

    [Theory]
    [InlineData("1234563210")]
    [InlineData("123-456-32-10")]
    [InlineData("PL1234563210")]
    [InlineData("PL 1234563210")]
    [InlineData("PL 123-456-32-10")]
    public void Validate_InvalidChecksumAcrossSupportedRepresentations_ReturnsInvalidChecksum(string input)
    {
        var result = Nip.Validate(input);

        result.IsValid.ShouldBeFalse();
        result.Error.ShouldBe(NipValidationError.InvalidChecksum);
    }

    [Theory]
    [MemberData(nameof(SupportedTextRepresentationCanonicalData))]
    public void Parse_SupportedTextRepresentation_ReturnsCanonicalNip(string input, string expectedCanonical)
    {
        var nip = Nip.Parse(input);

        nip.ToString().ShouldBe(expectedCanonical);
    }

    [Theory]
    [MemberData(nameof(SupportedTextRepresentationCanonicalData))]
    public void TryParse_SupportedTextRepresentation_ReturnsCanonicalNip(string input, string expectedCanonical)
    {
        Nip.TryParse(input, out var nip).ShouldBeTrue();

        nip.ToString().ShouldBe(expectedCanonical);
    }

    [Theory]
    [MemberData(nameof(SupportedTextRepresentationCanonicalData))]
    public void TryParse_WithError_SupportedTextRepresentation_ReturnsCanonicalNipAndNullError(string input, string expectedCanonical)
    {
        var success = Nip.TryParse(input, out var nip, out var error);

        success.ShouldBeTrue();
        nip.ToString().ShouldBe(expectedCanonical);
        error.ShouldBeNull();
    }
}