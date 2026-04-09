using System.Text.Json;
using PolishIdentifiers;
using Shouldly;

namespace PolishIdentifiers.SystemTextJson.Tests;

public class RegonJsonConverterTests
{
    private const string ValidRegon9 = "123456785";
    private const string ValidRegon14 = "12345678512347";
    private const string AllZeroRegon9 = "000000000";
    private const string AllZeroRegon14 = "00000000000000";
    private const string InvalidRegonChecksum = "123456780";
    private const string InvalidRegonChecksum14 = "12345678512340";
    private const string InvalidRegonChars = "12345678X";
    private const string InvalidRegonLength = "1234567";
    private const string EmptyRegon = "";
    private const string WhitespaceOnlyRegon = "   ";

    private sealed record RegonDto(Regon Regon);
    private sealed record NullableRegonDto(Regon? Regon);

    [Fact]
    public void Read_ValidRegon9_ReturnsCanonicalRegon()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();

        var dto = JsonSerializer.Deserialize<RegonDto>($$"""{"Regon":"{{ValidRegon9}}"}""", options);

        dto!.Regon.ToString().ShouldBe(ValidRegon9);
    }

    [Fact]
    public void Read_ValidRegon14_ReturnsCanonicalRegon()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();

        var dto = JsonSerializer.Deserialize<RegonDto>($$"""{"Regon":"{{ValidRegon14}}"}""", options);

        dto!.Regon.ToString().ShouldBe(ValidRegon14);
    }

    [Fact]
    public void Read_NullToken_ThrowsJsonException()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();

        var ex = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<RegonDto>("""{"Regon":null}""", options));

        ex.InnerException.ShouldBeNull();
    }

    [Fact]
    public void Read_NumberToken_ThrowsJsonException()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();

        var ex = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<RegonDto>("""{"Regon":123456785}""", options));

        ex.InnerException.ShouldBeNull();
    }

    [Fact]
    public void Read_ObjectToken_ThrowsJsonException()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();

        var ex = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<RegonDto>("""{"Regon":{}}""", options));

        ex.InnerException.ShouldBeNull();
    }

    [Fact]
    public void Read_ArrayToken_ThrowsJsonException()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();

        var ex = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<RegonDto>("""{"Regon":[]}""", options));

        ex.InnerException.ShouldBeNull();
    }

    [Fact]
    public void Read_BoolToken_ThrowsJsonException()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();

        var ex = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<RegonDto>("""{"Regon":true}""", options));

        ex.InnerException.ShouldBeNull();
    }

    [Fact]
    public void Read_EmptyString_ThrowsJsonExceptionWithoutInner()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();

        var ex = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<RegonDto>($$"""{"Regon":"{{EmptyRegon}}"}""", options));

        ex.InnerException.ShouldBeNull();
    }

    [Fact]
    public void Read_WhitespaceOnlyString_ThrowsJsonExceptionWithoutInner()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();

        var ex = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<RegonDto>($$"""{"Regon":"{{WhitespaceOnlyRegon}}"}""", options));

        ex.InnerException.ShouldBeNull();
    }

    [Fact]
    public void Read_InvalidCharacters_ThrowsJsonExceptionWithInvalidCharactersInner()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();

        var ex = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<RegonDto>($$"""{"Regon":"{{InvalidRegonChars}}"}""", options));
        var regonEx = ex.InnerException.ShouldBeOfType<RegonValidationException>();

        regonEx.Error.ShouldBe(RegonValidationError.InvalidCharacters);
    }

    [Fact]
    public void Read_InvalidLength_ThrowsJsonExceptionWithInvalidLengthInner()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();

        var ex = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<RegonDto>($$"""{"Regon":"{{InvalidRegonLength}}"}""", options));
        var regonEx = ex.InnerException.ShouldBeOfType<RegonValidationException>();

        regonEx.Error.ShouldBe(RegonValidationError.InvalidLength);
    }

    [Fact]
    public void Read_InvalidChecksum_ThrowsJsonExceptionWithInvalidChecksumInner()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();

        var ex = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<RegonDto>($$"""{"Regon":"{{InvalidRegonChecksum}}"}""", options));
        var regonEx = ex.InnerException.ShouldBeOfType<RegonValidationException>();

        regonEx.Error.ShouldBe(RegonValidationError.InvalidChecksum);
    }

    [Fact]
    public void Read_InvalidChecksum14_ThrowsJsonExceptionWithInvalidChecksumInner()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();

        var ex = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<RegonDto>($$"""{"Regon":"{{InvalidRegonChecksum14}}"}""", options));
        var regonEx = ex.InnerException.ShouldBeOfType<RegonValidationException>();

        regonEx.Error.ShouldBe(RegonValidationError.InvalidChecksum);
    }

    [Fact]
    public void Write_ValidRegon9_WritesCanonicalString()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();
        var dto = new RegonDto(Regon.Parse(ValidRegon9));

        var json = JsonSerializer.Serialize(dto, options);

        json.ShouldBe("""{"Regon":"123456785"}""");
    }

    [Fact]
    public void Write_ValidRegon14_WritesCanonicalString()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();
        var dto = new RegonDto(Regon.Parse(ValidRegon14));

        var json = JsonSerializer.Serialize(dto, options);

        json.ShouldBe("""{"Regon":"12345678512347"}""");
    }

    [Fact]
    public void Write_DefaultRegon_ThrowsJsonException()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();
        var dto = new RegonDto(default);

        var ex = Should.Throw<JsonException>(() => JsonSerializer.Serialize(dto, options));

        ex.InnerException.ShouldBeNull();
    }

    [Theory]
    [InlineData(ValidRegon9)]
    [InlineData(ValidRegon14)]
    public void RoundTrip_CanonicalValue_ProducesCanonicalOutput(string input)
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();
        var json = $$"""{"Regon":"{{input}}"}""";

        var dto = JsonSerializer.Deserialize<RegonDto>(json, options);
        var serialized = JsonSerializer.Serialize(dto, options);

        serialized.ShouldBe($$"""{"Regon":"{{input}}"}""");
    }

    [Fact]
    public void RoundTrip_AllZeroRegon9_PreservesLeadingZeros()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();
        var json = $$"""{"Regon":"{{AllZeroRegon9}}"}"""; 

        var dto = JsonSerializer.Deserialize<RegonDto>(json, options);
        var serialized = JsonSerializer.Serialize(dto, options);

        dto!.Regon.IsDefault.ShouldBeFalse();
        serialized.ShouldBe("""{"Regon":"000000000"}""");
    }

    [Fact]
    public void RoundTrip_AllZeroRegon14_PreservesLeadingZeros()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();
        var json = $$"""{"Regon":"{{AllZeroRegon14}}"}"""; 

        var dto = JsonSerializer.Deserialize<RegonDto>(json, options);
        var serialized = JsonSerializer.Serialize(dto, options);

        dto!.Regon.IsDefault.ShouldBeFalse();
        serialized.ShouldBe("""{"Regon":"00000000000000"}""");
    }

    [Fact]
    public void Deserialize_NullNullableRegon_ReturnsNull()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();

        var dto = JsonSerializer.Deserialize<NullableRegonDto>("""{"Regon":null}""", options);

        dto!.Regon.ShouldBeNull();
    }

    [Fact]
    public void Serialize_NullNullableRegon_WritesNull()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();
        var dto = new NullableRegonDto(null);

        var json = JsonSerializer.Serialize(dto, options);

        json.ShouldBe("""{"Regon":null}""");
    }
}
