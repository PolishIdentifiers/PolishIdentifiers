using PolishNip = global::PolishIdentifiers.Nip;
using PolishNipFormat = global::PolishIdentifiers.NipFormat;

namespace PolishIdentifiers.Examples.Nip;

internal static class NipExamples
{
    public static void TryParse()
    {
        const string input = "PL 123-456-32-18";
        var success = PolishNip.TryParse(input, out var nip, out var error);

        Console.WriteLine("NIP");
        Console.WriteLine($"Success: {success}");
        if (success)
        {
            Console.WriteLine($"Canonical: {nip}");
            Console.WriteLine($"Hyphenated: {nip.ToString(PolishNipFormat.Hyphenated)}");
        }
        else
        {
            Console.WriteLine($"Error: {error}");
        }

        Console.WriteLine();
    }

    public static void Parse()
    {
        var nip = PolishNip.Parse("1234563218");

        Console.WriteLine("NIP");
        Console.WriteLine($"Canonical: {nip}");
        Console.WriteLine($"VAT-EU: {nip.ToString(PolishNipFormat.VatEu)}");
        Console.WriteLine();
    }

    public static void Validate()
    {
        var result = PolishNip.Validate("PL-1234563218");

        Console.WriteLine("NIP");
        Console.WriteLine($"Is valid: {result.IsValid}");
        Console.WriteLine($"Error: {result.Error}");
        Console.WriteLine();
    }
}