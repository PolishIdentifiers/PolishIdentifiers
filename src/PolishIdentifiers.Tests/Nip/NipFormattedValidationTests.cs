using PolishIdentifiers;
using Shouldly;

namespace PolishIdentifiers.Tests;

public class NipFormattedValidationTests
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

    // --- Valid inputs across all 5 formats ---

    public static TheoryData<string> AllValidFormatsData => new()
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

    public static TheoryData<string, string> ValidFormattedCanonicalData => new()
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

    [Theory]
    [MemberData(nameof(AllValidFormatsData))]
    public void ValidateFormatted_ValidInput_ReturnsValid(string nip)
    {
        var result = Nip.ValidateFormatted(nip);

        Assert.True(result.IsValid);
        Assert.Null(result.Error);
    }

    // --- Unrecognized formats ---

    public static TheoryData<string> UnrecognizedFormatData => new()
    {
        "PL-1234563218",          // dash instead of space after PL
        "PL123-456-32-18",        // PL + hyphens without space
        "12-345-632-18",          // wrong hyphen positions
        "123 456 32 18",          // spaces instead of hyphens
        "pl1234563218",           // lowercase pl
        "pl 1234563218",          // lowercase pl with space
        "DE1234563218",           // wrong country prefix
        "PL  1234563218",         // double space
        " PL1234563218",          // leading space
        "PL1234563218 ",          // trailing space
        "123-456-3218",           // missing one hyphen
        "123-456-32-18-",         // trailing hyphen
    };

    [Theory]
    [MemberData(nameof(UnrecognizedFormatData))]
    public void ValidateFormatted_UnrecognizedFormat_ReturnsUnrecognizedFormat(string nip)
    {
        var result = Nip.ValidateFormatted(nip);

        Assert.False(result.IsValid);
        Assert.Equal(NipValidationError.UnrecognizedFormat, result.Error);
    }

    // --- Null input ---

    [Fact]
    public void ValidateFormatted_Null_ReturnsUnrecognizedFormat()
    {
        var result = Nip.ValidateFormatted(null);

        result.IsValid.ShouldBeFalse();
        result.Error.ShouldBe(NipValidationError.UnrecognizedFormat);
    }

    // --- Invalid checksum through formatted path ---

    [Theory]
    [InlineData("1234563210")]
    [InlineData("123-456-32-10")]
    [InlineData("PL1234563210")]
    [InlineData("PL 1234563210")]
    [InlineData("PL 123-456-32-10")]
    public void ValidateFormatted_InvalidChecksum_ReturnsInvalidChecksum(string nip)
    {
        var result = Nip.ValidateFormatted(nip);

        Assert.False(result.IsValid);
        Assert.Equal(NipValidationError.InvalidChecksum, result.Error);
    }

    // --- Checksum mod 11 == 10 edge case through formatted path ---

    [Theory]
    [InlineData("1234567890")]
    [InlineData("123-456-78-90")]
    [InlineData("PL1234567890")]
    public void ValidateFormatted_ChecksumMod11Equals10_ReturnsInvalidChecksum(string nip)
    {
        var result = Nip.ValidateFormatted(nip);

        Assert.False(result.IsValid);
        Assert.Equal(NipValidationError.InvalidChecksum, result.Error);
    }

    // --- TryParseFormatted ---

    [Theory]
    [MemberData(nameof(AllValidFormatsData))]
    public void TryParseFormatted_ValidInput_ReturnsTrue(string nip)
    {
        Assert.True(Nip.TryParseFormatted(nip, out _));
    }

    [Theory]
    [MemberData(nameof(AllValidFormatsData))]
    public void TryParseFormatted_ValidInput_ProducesCanonicalToString(string nip)
    {
        Nip.TryParseFormatted(nip, out var parsed);

        // All formats of the same underlying NIP should produce the same canonical output.
        var isFirstGroup = nip.Contains("1234563218") || nip.Contains("123-456-32-18");
        var expected = isFirstGroup ? "1234563218" : "7680002466";
        Assert.Equal(expected, parsed.ToString());
    }

    [Fact]
    public void TryParseFormatted_Null_ReturnsFalse()
    {
        Assert.False(Nip.TryParseFormatted(null, out _));
    }

    [Theory]
    [MemberData(nameof(UnrecognizedFormatData))]
    public void TryParseFormatted_UnrecognizedFormat_ReturnsFalse(string nip)
    {
        Assert.False(Nip.TryParseFormatted(nip, out _));
    }

    // --- ParseFormatted ---

    [Theory]
    [MemberData(nameof(AllValidFormatsData))]
    public void ParseFormatted_ValidInput_DoesNotThrow(string nip)
    {
        var exception = Record.Exception(() => Nip.ParseFormatted(nip));

        Assert.Null(exception);
    }

    [Theory]
    [MemberData(nameof(ValidFormattedCanonicalData))]
    public void ParseFormatted_ValidInput_ReturnsCanonicalNip(string input, string expectedCanonical)
    {
        var nip = Nip.ParseFormatted(input);

        nip.ToString().ShouldBe(expectedCanonical);
    }

    [Fact]
    public void ParseFormatted_Null_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Nip.ParseFormatted((string)null!));
    }

    [Theory]
    [MemberData(nameof(UnrecognizedFormatData))]
    public void ParseFormatted_UnrecognizedFormat_ThrowsNipValidationException(string nip)
    {
        var ex = Assert.Throws<NipValidationException>(() => Nip.ParseFormatted(nip));

        Assert.Equal(NipValidationError.UnrecognizedFormat, ex.Error);
    }

    [Theory]
    [InlineData("123-456-32-10")]
    [InlineData("PL1234563210")]
    public void ParseFormatted_InvalidChecksum_ThrowsNipValidationException(string nip)
    {
        var ex = Assert.Throws<NipValidationException>(() => Nip.ParseFormatted(nip));

        Assert.Equal(NipValidationError.InvalidChecksum, ex.Error);
    }

    // --- Span overloads ---

    [Fact]
    public void ValidateFormatted_SpanOverload_ValidHyphenated_ReturnsValid()
    {
        Assert.True(Nip.ValidateFormatted(ValidHyphenated.AsSpan()).IsValid);
    }

    [Fact]
    public void ValidateFormatted_SpanOverload_UnrecognizedFormat_ReturnsUnrecognizedFormat()
    {
        Assert.Equal(
            NipValidationError.UnrecognizedFormat,
            Nip.ValidateFormatted("PL-1234563218".AsSpan()).Error);
    }

    [Fact]
    public void ParseFormatted_SpanOverload_ValidPlPrefix_ParsesSuccessfully()
    {
        var nip = Nip.ParseFormatted(ValidPlPrefix.AsSpan());

        Assert.Equal("1234563218", nip.ToString());
    }

    [Fact]
    public void TryParseFormatted_SpanOverload_ValidInput_ReturnsTrue()
    {
        Assert.True(Nip.TryParseFormatted(ValidPlSpaceHyphenated.AsSpan(), out var nip));
        Assert.Equal("1234563218", nip.ToString());
    }

    [Theory]
    [MemberData(nameof(ValidFormattedCanonicalData))]
    public void TryParseFormatted_ValidInput_SetsCanonicalNip(string input, string expectedCanonical)
    {
        Nip.TryParseFormatted(input, out var nip).ShouldBeTrue();

        nip.ToString().ShouldBe(expectedCanonical);
    }

    [Fact]
    public void TryParseFormatted_SpanOverload_EmptySpan_ReturnsFalse()
    {
        Assert.False(Nip.TryParseFormatted(ReadOnlySpan<char>.Empty, out _));
    }
}
