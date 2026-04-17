using System.Text.Json;
using PolishIdentifiers;
using Shouldly;

namespace PolishIdentifiers.SystemTextJson.Tests;

public class PeselJsonConverterTests
{
    private const string ValidPesel = "44051401458";
    private const string LeadingZeroPesel = "00010100015"; // born 1900-01-01, serial 001, female, checksum 5
    private const string InvalidPeselChecksum = "44051401457";
    private const string InvalidPeselChars = "4405140145A";
    private const string InvalidPeselLength = "4405140145";
    private const string InvalidPeselDate = "99223112345";
    private const string EmptyPesel = "";
    private const string WhitespaceOnlyPesel = "   ";

    private sealed record PeselDto(Pesel Pesel);
    private sealed record NullablePeselDto(Pesel? Pesel);

    [Fact]
    public void Read_CanonicalValue_ReturnsPesel()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();

        var dto = JsonSerializer.Deserialize<PeselDto>($$"""{"Pesel":"{{ValidPesel}}"}""", options);

        dto!.Pesel.ToString().ShouldBe(ValidPesel);
    }

    [Fact]
    public void Read_NullToken_ThrowsJsonException()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();

        var ex = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<PeselDto>("""{"Pesel":null}""", options));

        ex.InnerException.ShouldBeNull();
    }

    [Fact]
    public void Read_NumberToken_ThrowsJsonException()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();

        var ex = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<PeselDto>("""{"Pesel":44051401458}""", options));

        ex.InnerException.ShouldBeNull();
    }

    [Fact]
    public void Read_ObjectToken_ThrowsJsonException()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();

        var ex = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<PeselDto>("""{"Pesel":{}}""", options));

        ex.InnerException.ShouldBeNull();
    }

    [Fact]
    public void Read_ArrayToken_ThrowsJsonException()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();

        var ex = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<PeselDto>("""{"Pesel":[]}""", options));

        ex.InnerException.ShouldBeNull();
    }

    [Fact]
    public void Read_BoolToken_ThrowsJsonException()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();

        var ex = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<PeselDto>("""{"Pesel":true}""", options));

        ex.InnerException.ShouldBeNull();
    }

    [Fact]
    public void Read_EmptyString_ThrowsJsonExceptionWithoutInner()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();

        var ex = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<PeselDto>($$"""{"Pesel":"{{EmptyPesel}}"}""", options));

        ex.InnerException.ShouldBeNull();
    }

    [Fact]
    public void Read_WhitespaceOnlyString_ThrowsJsonExceptionWithoutInner()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();

        var ex = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<PeselDto>($$"""{"Pesel":"{{WhitespaceOnlyPesel}}"}""", options));

        ex.InnerException.ShouldBeNull();
    }

    [Fact]
    public void Read_InvalidCharacters_ThrowsJsonExceptionWithInvalidCharactersInner()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();

        var ex = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<PeselDto>($$"""{"Pesel":"{{InvalidPeselChars}}"}""", options));
        var peselEx = ex.InnerException.ShouldBeOfType<PeselValidationException>();

        peselEx.Error.ShouldBe(PeselValidationError.InvalidCharacters);
    }

    [Fact]
    public void Read_InvalidLength_ThrowsJsonExceptionWithInvalidLengthInner()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();

        var ex = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<PeselDto>($$"""{"Pesel":"{{InvalidPeselLength}}"}""", options));
        var peselEx = ex.InnerException.ShouldBeOfType<PeselValidationException>();

        peselEx.Error.ShouldBe(PeselValidationError.InvalidLength);
    }

    [Fact]
    public void Read_InvalidDate_ThrowsJsonExceptionWithInvalidDateInner()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();

        var ex = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<PeselDto>($$"""{"Pesel":"{{InvalidPeselDate}}"}""", options));
        var peselEx = ex.InnerException.ShouldBeOfType<PeselValidationException>();

        peselEx.Error.ShouldBe(PeselValidationError.InvalidDate);
    }

    [Fact]
    public void Read_InvalidChecksum_ThrowsJsonExceptionWithInvalidChecksumInner()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();

        var ex = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<PeselDto>($$"""{"Pesel":"{{InvalidPeselChecksum}}"}""", options));
        var peselEx = ex.InnerException.ShouldBeOfType<PeselValidationException>();

        peselEx.Error.ShouldBe(PeselValidationError.InvalidChecksum);
    }

    [Fact]
    public void Write_ValidPesel_WritesCanonicalString()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();
        var dto = new PeselDto(Pesel.Parse(ValidPesel));

        var json = JsonSerializer.Serialize(dto, options);

        json.ShouldBe("""{"Pesel":"44051401458"}""");
    }

    [Fact]
    public void Write_DefaultPesel_ThrowsJsonException()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();
        var dto = new PeselDto(default);

        var ex = Should.Throw<JsonException>(() => JsonSerializer.Serialize(dto, options));

        ex.InnerException.ShouldBeNull();
    }

    [Fact]
    public void RoundTrip_CanonicalValue_ProducesCanonicalOutput()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();
        var json = $$"""{"Pesel":"{{ValidPesel}}"}""";

        var dto = JsonSerializer.Deserialize<PeselDto>(json, options);
        var serialized = JsonSerializer.Serialize(dto, options);

        serialized.ShouldBe("""{"Pesel":"44051401458"}""");
    }

    [Fact]
    public void RoundTrip_LeadingZeroPesel_PreservesLeadingZeros()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();
        var json = $$"""{"Pesel":"{{LeadingZeroPesel}}"}"""; 

        var dto = JsonSerializer.Deserialize<PeselDto>(json, options);
        var serialized = JsonSerializer.Serialize(dto, options);

        dto!.Pesel.IsDefault.ShouldBeFalse();
        serialized.ShouldBe("""{"Pesel":"00010100015"}""");
    }

    [Fact]
    public void Deserialize_NullNullablePesel_ReturnsNull()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();

        var dto = JsonSerializer.Deserialize<NullablePeselDto>("""{"Pesel":null}""", options);

        dto!.Pesel.ShouldBeNull();
    }

    [Fact]
    public void Serialize_NullNullablePesel_WritesNull()
    {
        var options = new JsonSerializerOptions().AddPolishIdentifiers();
        var dto = new NullablePeselDto(null);

        var json = JsonSerializer.Serialize(dto, options);

        json.ShouldBe("""{"Pesel":null}""");
    }
}
