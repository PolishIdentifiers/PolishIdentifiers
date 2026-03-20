using PolishIdentifiers;

internal static class Program
{
    public static int Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Usage: dotnet run --project examples/PolishIdentifiers.Examples.Regon -- <REGON>");
            Console.WriteLine("Accepted lengths: 9 or 14 digits.");
            return 1;
        }

        var input = args[0];
        if (!Regon.TryParse(input, out var regon, out var error))
        {
            Console.WriteLine("invalid");
            Console.WriteLine($"Reason: {error}");
            return 1;
        }

        Console.WriteLine("valid");
        Console.WriteLine($"Canonical: {regon}");
        Console.WriteLine($"Kind: {regon.Kind}");
        Console.WriteLine($"Is REGON-9: {regon.IsRegon9}");
        Console.WriteLine($"Is REGON-14: {regon.IsRegon14}");
        Console.WriteLine($"Base REGON-9: {regon.BaseRegon9}");
        return 0;
    }
}
