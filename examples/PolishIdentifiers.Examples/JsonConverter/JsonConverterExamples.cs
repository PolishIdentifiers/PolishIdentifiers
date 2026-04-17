using System.Text.Json;
using PolishNip = global::PolishIdentifiers.Nip;
using PolishNipFormat = global::PolishIdentifiers.NipFormat;
using PolishPesel = global::PolishIdentifiers.Pesel;
using PolishRegon = global::PolishIdentifiers.Regon;

namespace PolishIdentifiers.Examples.JsonConverter;

internal static class JsonConverterExamples
{
    public static void Run()
    {
        var options = new JsonSerializerOptions()
            .AddPolishIdentifiers(new PolishIdentifiersJsonOptions { NipOutputFormat = PolishNipFormat.Hyphenated });

        var supplier = new SupplierDto(
            "Acme Sp. z o.o.",
            PolishNip.Parse("1234563218"),
            PolishRegon.Parse("123456785"),
            PolishPesel.Parse("44051401458"));

        var json = JsonSerializer.Serialize(supplier, options);
        var roundTrip = JsonSerializer.Deserialize<SupplierDto>(json, options)
            ?? throw new JsonException("JSON could not be deserialized.");

        Console.WriteLine("JSON converter");
        Console.WriteLine(json);
        Console.WriteLine($"Round-trip company: {roundTrip.CompanyName}");
        Console.WriteLine();
    }

    private sealed record SupplierDto(
        string CompanyName,
        PolishNip Nip,
        PolishRegon Regon,
        PolishPesel Pesel);
}