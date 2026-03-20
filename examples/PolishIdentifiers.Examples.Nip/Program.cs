using PolishIdentifiers;

internal static class Program
{
    public static int Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Usage: dotnet run --project examples/PolishIdentifiers.Examples.Nip -- <NIP>");
            Console.WriteLine("Accepted examples: 1234563218, 123-456-32-18, PL1234563218, PL 1234563218, PL 123-456-32-18");
            return 1;
        }

        var input = args[0];
        if (!Nip.TryParse(input, out var nip, out var error))
        {
            Console.WriteLine("invalid");
            Console.WriteLine($"Reason: {error}");
            return 1;
        }

        Console.WriteLine("valid");
        Console.WriteLine($"Canonical: {nip}");
        Console.WriteLine($"Hyphenated: {nip.ToString(NipFormat.Hyphenated)}");
        Console.WriteLine($"With PL prefix: {nip.ToString(NipFormat.VatEu)}");
        Console.WriteLine($"Issuing tax office prefix: {nip.IssuingTaxOfficePrefix}");
        return 0;
    }
}
