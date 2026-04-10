using PolishRegon = global::PolishIdentifiers.Regon;

namespace PolishIdentifiers.Examples.Regon;

internal static class RegonExamples
{
    public static void TryParse()
    {
        const string input = "12345678512347";
        var success = PolishRegon.TryParse(input, out var regon, out var error);

        Console.WriteLine("REGON");
        Console.WriteLine($"Success: {success}");
        if (success)
        {
            Console.WriteLine($"Kind: {regon.Kind}");
            Console.WriteLine($"Base REGON-9: {regon.BaseRegon9}");
        }
        else
        {
            Console.WriteLine($"Error: {error}");
        }

        Console.WriteLine();
    }

    public static void Parse()
    {
        var regon = PolishRegon.Parse("12345678512347");

        Console.WriteLine("REGON");
        Console.WriteLine($"Canonical: {regon}");
        Console.WriteLine($"Kind: {regon.Kind}");
        Console.WriteLine();
    }

    public static void Validate()
    {
        var result = PolishRegon.Validate("123456780");

        Console.WriteLine("REGON");
        Console.WriteLine($"Is valid: {result.IsValid}");
        Console.WriteLine($"Error: {result.Error}");
    }
}