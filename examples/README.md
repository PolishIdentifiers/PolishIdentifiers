# Examples

These examples are small console applications that show the intended developer flow:

1. read an identifier from the command line
2. parse it with the typed-error `TryParse(..., out value, out error)` path
3. print `valid` or `invalid` with the reason
4. print domain-specific information from the strong type

Run them from the repository root:

```powershell
dotnet run --project examples/PolishIdentifiers.Examples.Pesel -- 44051401458
dotnet run --project examples/PolishIdentifiers.Examples.Nip -- "PL 123-456-32-18"
dotnet run --project examples/PolishIdentifiers.Examples.Regon -- 12345678512347
```

Example invalid runs:

```powershell
dotnet run --project examples/PolishIdentifiers.Examples.Pesel -- 44051401459
dotnet run --project examples/PolishIdentifiers.Examples.Nip -- "PL-1234563218"
dotnet run --project examples/PolishIdentifiers.Examples.Regon -- 123456780
```
