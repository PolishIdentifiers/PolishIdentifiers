using System.Linq;
using System.Text.Json;
using PolishIdentifiers;
using Shouldly;

namespace PolishIdentifiers.SystemTextJson.Tests;

public class JsonSerializerOptionsExtensionsTests
{
    private const string ValidNip = "1234563218";

    private sealed record NipDto(Nip Nip);

    [Fact]
    public void AddPolishIdentifiers_CalledTwice_RegistersExactlyOneConverterPerType()
    {
        var options = new JsonSerializerOptions();

        options.AddPolishIdentifiers();
        options.AddPolishIdentifiers();

        options.Converters.OfType<NipJsonConverter>().Count().ShouldBe(1);
        options.Converters.OfType<PeselJsonConverter>().Count().ShouldBe(1);
        options.Converters.OfType<RegonJsonConverter>().Count().ShouldBe(1);
    }

    [Fact]
    public void AddPolishIdentifiers_NipConverterAlreadyRegistered_DoesNotAddSecondInstance()
    {
        var options = new JsonSerializerOptions();

        options.Converters.Add(new NipJsonConverter());
        options.AddPolishIdentifiers();

        options.Converters.OfType<NipJsonConverter>().Count().ShouldBe(1);
    }

    [Fact]
    public void AddPolishIdentifiers_SecondCallWithDifferentFormat_KeepsFirstFormat()
    {
        var options = new JsonSerializerOptions();

        options.AddPolishIdentifiers(new PolishIdentifiersJsonOptions { NipOutputFormat = NipFormat.VatEu });
        options.AddPolishIdentifiers(new PolishIdentifiersJsonOptions { NipOutputFormat = NipFormat.Hyphenated });

        var dto = new NipDto(Nip.Parse(ValidNip));
        var json = JsonSerializer.Serialize(dto, options);

        json.ShouldBe("""{"Nip":"PL1234563218"}""");
    }

    [Fact]
    public void AddPolishIdentifiers_WithNipOutputFormat_UsesSpecifiedFormat()
    {
        var options = new JsonSerializerOptions();

        options.AddPolishIdentifiers(new PolishIdentifiersJsonOptions { NipOutputFormat = NipFormat.VatEu });

        var dto = new NipDto(Nip.Parse(ValidNip));
        var json = JsonSerializer.Serialize(dto, options);

        json.ShouldBe("""{"Nip":"PL1234563218"}""");
    }

    [Fact]
    public void AddPolishIdentifiers_NullJsonSerializerOptions_ThrowsArgumentNullException()
    {
        JsonSerializerOptions options = null!;

        Should.Throw<ArgumentNullException>(() => options.AddPolishIdentifiers());
    }

    [Fact]
    public void AddPolishIdentifiers_ReturnsTheSameInstance()
    {
        var options = new JsonSerializerOptions();

        var returned = options.AddPolishIdentifiers();

        returned.ShouldBeSameAs(options);
    }
}
