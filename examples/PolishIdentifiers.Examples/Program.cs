using PolishIdentifiers.Examples.JsonConverter;
using PolishIdentifiers.Examples.Nip;
using PolishIdentifiers.Examples.Pesel;
using PolishIdentifiers.Examples.Regon;

namespace PolishIdentifiers.Examples;

internal static class Program
{
    public static int Main()
    {
        TryParse();
        Parse();
        Validate();
        return 0;
    }

    private static void TryParse()
    {
        Console.WriteLine("=== TryParse ===");
        NipExamples.TryParse();
        PeselExamples.TryParse();
        RegonExamples.TryParse();
        Console.WriteLine();
    }

    private static void Parse()
    {
        Console.WriteLine("=== Parse ===");
        NipExamples.Parse();
        PeselExamples.Parse();
        RegonExamples.Parse();
        JsonConverterExamples.Run();
        Console.WriteLine();
    }

    private static void Validate()
    {
        Console.WriteLine("=== Validate ===");
        NipExamples.Validate();
        PeselExamples.Validate();
        RegonExamples.Validate();
    }
}