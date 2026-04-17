using PolishPesel = global::PolishIdentifiers.Pesel;
using PolishPeselGenerator = global::PolishIdentifiers.PeselGenerator;
using PolishGender = global::PolishIdentifiers.Gender;

namespace PolishIdentifiers.Examples.Pesel;

internal static class PeselExamples
{
    public static void TryParse()
    {
        const string input = "44051401458";
        var success = PolishPesel.TryParse(input, out var pesel, out var error);

        Console.WriteLine("PESEL");
        Console.WriteLine($"Success: {success}");
        if (success)
        {
            Console.WriteLine($"Birth date: {pesel.BirthDate:yyyy-MM-dd}");
            Console.WriteLine($"Gender: {pesel.Gender}");
        }
        else
        {
            Console.WriteLine($"Error: {error}");
        }

        Console.WriteLine();
    }

    public static void Parse()
    {
        var pesel = PolishPesel.Parse("44051401458");

        Console.WriteLine("PESEL");
        Console.WriteLine($"Canonical: {pesel}");
        Console.WriteLine($"Birth date: {pesel.BirthDate:yyyy-MM-dd}");
        Console.WriteLine();
    }

    public static void Generators()
    {
        var single = PolishPeselGenerator.Generate(PolishGender.Female, new DateTime(1990, 6, 15));
        var batch = PolishPeselGenerator.Generate(count: 3);
        var invalidBatch = PolishPeselGenerator.Invalid.WrongChecksum(count: 3);

        Console.WriteLine("PESEL");
        Console.WriteLine($"Single generated: {single}");
        Console.WriteLine($"Batch count: {batch.Count}");
        Console.WriteLine($"Invalid batch count: {invalidBatch.Count}");
        Console.WriteLine();
    }

    public static void Validate()
    {
        var result = PolishPesel.Validate("44051401459");

        Console.WriteLine("PESEL");
        Console.WriteLine($"Is valid: {result.IsValid}");
        Console.WriteLine($"Error: {result.Error}");
        Console.WriteLine();
    }
}