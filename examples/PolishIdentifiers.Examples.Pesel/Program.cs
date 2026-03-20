using PolishIdentifiers;

internal static class Program
{
    public static int Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Usage: dotnet run --project examples/PolishIdentifiers.Examples.Pesel -- <PESEL>");
            return 1;
        }

        var input = args[0];
        if (!Pesel.TryParse(input, out var pesel, out var error))
        {
            Console.WriteLine("invalid");
            Console.WriteLine($"Reason: {error}");
            return 1;
        }

        Console.WriteLine("valid");
        Console.WriteLine($"Canonical: {pesel}");
        Console.WriteLine($"Birth date: {pesel.BirthDate:yyyy-MM-dd}");
        Console.WriteLine($"Gender: {pesel.Gender}");
        return 0;
    }
}
