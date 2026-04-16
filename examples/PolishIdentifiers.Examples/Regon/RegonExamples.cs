using PolishRegon = global::PolishIdentifiers.Regon;
using PolishRegonGenerator = global::PolishIdentifiers.RegonGenerator;
using PolishRegonKind = global::PolishIdentifiers.RegonKind;

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

    public static void Generators()
    {
        var single = PolishRegonGenerator.Generate(PolishRegonKind.Regon14);
        var batch = PolishRegonGenerator.Generate(PolishRegonKind.Regon9, count: 3);
        var invalidBatch = PolishRegonGenerator.Invalid.WrongChecksumRegon9(count: 3);

        Console.WriteLine("REGON");
        Console.WriteLine($"Single generated: {single}");
        Console.WriteLine($"Batch count: {batch.Count}");
        Console.WriteLine($"Invalid batch count: {invalidBatch.Count}");
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