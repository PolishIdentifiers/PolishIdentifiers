using System.Text.Json;
using PolishIdentifiers;
using Shouldly;

namespace PolishIdentifiers.SystemTextJson.Tests;

public class NipJsonConverterTests
{
    private const string ValidNip = "1234563218";
    private const string AllZeroNip = "0000000000";
    private const string ValidNipHyphenated = "123-456-32-18";
    private const string ValidNipVatEu = "PL1234563218";
    private const string ValidNipVatEuWithSpace = "PL 1234563218";
    private const string ValidNipVatEuHyphenated = "PL 123-456-32-18";
    private const string InvalidNipChecksum = "1234563217";
    private const string InvalidNipChars = "12345XXXXX";
    private const string InvalidNipLength = "123456";
    private const string UnrecognizedFormatNip = "NL1234563218";
    private const string EmptyNip = "";
    private const string WhitespaceOnlyNip = "   ";

    private sealed record NipDto(Nip Nip);
    private sealed record NullableNipDto(Nip? Nip);

    [Fact]
    public void Read_CanonicalValue_ReturnsNip()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();

        var dto = JsonSerializer.Deserialize<NipDto>($$"""{"Nip":"{{ValidNip}}"}""", options);

        dto!.Nip.ToString().ShouldBe(ValidNip);
    }

    [Fact]
    public void Read_HyphenatedValue_ReturnsCanonicalNip()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();

        var dto = JsonSerializer.Deserialize<NipDto>($$"""{"Nip":"{{ValidNipHyphenated}}"}""", options);

        dto!.Nip.ToString().ShouldBe(ValidNip);
    }

    [Fact]
    public void Read_VatEuValue_ReturnsCanonicalNip()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();

        var dto = JsonSerializer.Deserialize<NipDto>($$"""{"Nip":"{{ValidNipVatEu}}"}""", options);

        dto!.Nip.ToString().ShouldBe(ValidNip);
    }

    [Fact]
    public void Read_VatEuWithSpaceValue_ReturnsCanonicalNip()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();

        var dto = JsonSerializer.Deserialize<NipDto>($$"""{"Nip":"{{ValidNipVatEuWithSpace}}"}""", options);

        dto!.Nip.ToString().ShouldBe(ValidNip);
    }

    [Fact]
    public void Read_VatEuHyphenatedValue_ReturnsCanonicalNip()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();

        var dto = JsonSerializer.Deserialize<NipDto>($$"""{"Nip":"{{ValidNipVatEuHyphenated}}"}""", options);

        dto!.Nip.ToString().ShouldBe(ValidNip);
    }

    [Fact]
    public void Read_NullToken_ThrowsJsonException()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();

        var ex = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<NipDto>("""{"Nip":null}""", options));

        ex.InnerException.ShouldBeNull();
    }

    [Fact]
    public void Read_NumberToken_ThrowsJsonException()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();

        var ex = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<NipDto>("""{"Nip":1234563218}""", options));

        ex.InnerException.ShouldBeNull();
    }

    [Fact]
    public void Read_ObjectToken_ThrowsJsonException()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();

        var ex = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<NipDto>("""{"Nip":{}}""", options));

        ex.InnerException.ShouldBeNull();
    }

    [Fact]
    public void Read_ArrayToken_ThrowsJsonException()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();

        var ex = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<NipDto>("""{"Nip":[]}""", options));

        ex.InnerException.ShouldBeNull();
    }

    [Fact]
    public void Read_BoolToken_ThrowsJsonException()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();

        var ex = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<NipDto>("""{"Nip":true}""", options));

        ex.InnerException.ShouldBeNull();
    }

    [Fact]
    public void Read_EmptyString_ThrowsJsonExceptionWithoutInner()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();

        var ex = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<NipDto>($$"""{"Nip":"{{EmptyNip}}"}""", options));

        ex.InnerException.ShouldBeNull();
    }

    [Fact]
    public void Read_WhitespaceOnlyString_ThrowsJsonExceptionWithoutInner()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();

        var ex = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<NipDto>($$"""{"Nip":"{{WhitespaceOnlyNip}}"}""", options));

        ex.InnerException.ShouldBeNull();
    }

    [Fact]
    public void Read_InvalidCharacters_ThrowsJsonExceptionWithInvalidCharactersInner()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();

        var ex = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<NipDto>($$"""{"Nip":"{{InvalidNipChars}}"}""", options));
        var nipEx = ex.InnerException.ShouldBeOfType<NipValidationException>();

        nipEx.Error.ShouldBe(NipValidationError.InvalidCharacters);
    }

    [Fact]
    public void Read_InvalidLength_ThrowsJsonExceptionWithInvalidLengthInner()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();

        var ex = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<NipDto>($$"""{"Nip":"{{InvalidNipLength}}"}""", options));
        var nipEx = ex.InnerException.ShouldBeOfType<NipValidationException>();

        nipEx.Error.ShouldBe(NipValidationError.InvalidLength);
    }

    [Fact]
    public void Read_InvalidChecksum_ThrowsJsonExceptionWithInvalidChecksumInner()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();

        var ex = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<NipDto>($$"""{"Nip":"{{InvalidNipChecksum}}"}""", options));
        var nipEx = ex.InnerException.ShouldBeOfType<NipValidationException>();

        nipEx.Error.ShouldBe(NipValidationError.InvalidChecksum);
    }

    [Fact]
    public void Read_UnrecognizedFormat_ThrowsJsonExceptionWithInvalidCharactersInner()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();

        var ex = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<NipDto>($$"""{"Nip":"{{UnrecognizedFormatNip}}"}""", options));
        var nipEx = ex.InnerException.ShouldBeOfType<NipValidationException>();

        nipEx.Error.ShouldBe(NipValidationError.InvalidCharacters);
    }

    [Fact]
    public void Write_ValidNip_WritesCanonicalString()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();
        var dto = new NipDto(Nip.Parse(ValidNip));

        var json = JsonSerializer.Serialize(dto, options);

        json.ShouldBe("""{"Nip":"1234563218"}""");
    }

    [Fact]
    public void Write_ValidNipWithVatEuFormat_WritesVatEuString()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers(new PolishIdentifiersJsonOptions
        {
            NipOutputFormat = NipFormat.VatEu
        });
        var dto = new NipDto(Nip.Parse(ValidNip));

        var json = JsonSerializer.Serialize(dto, options);

        json.ShouldBe("""{"Nip":"PL1234563218"}""");
    }

    [Fact]
    public void Write_ValidNipWithHyphenatedFormat_WritesHyphenatedString()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers(new PolishIdentifiersJsonOptions
        {
            NipOutputFormat = NipFormat.Hyphenated
        });
        var dto = new NipDto(Nip.Parse(ValidNip));

        var json = JsonSerializer.Serialize(dto, options);

        json.ShouldBe("""{"Nip":"123-456-32-18"}""");
    }

    [Fact]
    public void Write_DefaultNip_ThrowsJsonException()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();
        var dto = new NipDto(default);

        var ex = Should.Throw<JsonException>(() => JsonSerializer.Serialize(dto, options));

        ex.InnerException.ShouldBeNull();
    }

    [Theory]
    [InlineData(ValidNip)]
    [InlineData(ValidNipHyphenated)]
    [InlineData(ValidNipVatEu)]
    [InlineData(ValidNipVatEuWithSpace)]
    [InlineData(ValidNipVatEuHyphenated)]
    public void RoundTrip_EachInputFormat_WithDigitsOnlyOutput_ProducesCanonicalOutput(string input)
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();
        var json = $$"""{"Nip":"{{input}}"}""";

        var dto = JsonSerializer.Deserialize<NipDto>(json, options);
        var serialized = JsonSerializer.Serialize(dto, options);

        serialized.ShouldBe("""{"Nip":"1234563218"}""");
    }

    [Fact]
    public void RoundTrip_AllZeroNip_PreservesLeadingZeros()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();
        var json = $$"""{"Nip":"{{AllZeroNip}}"}"""; 

        var dto = JsonSerializer.Deserialize<NipDto>(json, options);
        var serialized = JsonSerializer.Serialize(dto, options);

        dto!.Nip.IsDefault.ShouldBeFalse();
        serialized.ShouldBe("""{"Nip":"0000000000"}""");
    }

    [Fact]
    public void Deserialize_NullNullableNip_ReturnsNull()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();

        var dto = JsonSerializer.Deserialize<NullableNipDto>("""{"Nip":null}""", options);

        dto!.Nip.ShouldBeNull();
    }

    [Fact]
    public void Serialize_NullNullableNip_WritesNull()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();
        var dto = new NullableNipDto(null);

        var json = JsonSerializer.Serialize(dto, options);

        json.ShouldBe("""{"Nip":null}""");
    }
}
