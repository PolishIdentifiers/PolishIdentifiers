# PolishIdentifiers

Strongly typed Polish formal identifiers for .NET.

`PolishIdentifiers` provides value types for `Pesel`, `Nip`, and `Regon`, with parsing, validation, formatting, generation, and DataAnnotations support. The library performs offline structural validation only. It does not query government registries or external services.

Use `Pesel`, `Nip`, and `Regon` in your application model instead of raw `string` or `long` values once input has been validated at the boundary.

## Supported frameworks

- `netstandard2.0`
- `net10.0`

## Installation

```powershell
dotnet add package PolishIdentifiers
```

## Supported identifiers

| Type | Input shape | Notes |
|---|---|---|
| `Pesel` | 11 digits | Decodes birth date and gender |
| `Nip` | 10 digits | Also supports a separate formatted-input path |
| `Regon` | 9 or 14 digits | `Regon14` includes an embedded `Regon9` base |

## Quick start

### Use strong types in the domain model

```csharp
using PolishIdentifiers;

public sealed class Person
{
    public Person(Pesel pesel)
    {
        Pesel = pesel;
    }

    public Pesel Pesel { get; }
}
```

```csharp
using PolishIdentifiers;

public sealed class Company
{
    public Company(Nip nip, Regon regon)
    {
        Nip = nip;
        Regon = regon;
    }

    public Nip Nip { get; }
    public Regon Regon { get; }
}
```

### Parse and validate

```csharp
using PolishIdentifiers;

Pesel pesel = Pesel.Parse("44051401458");
Nip nip = Nip.Parse("1234563218");
Regon regon = Regon.Parse("12345678512347");
```

```csharp
using PolishIdentifiers;

if (Pesel.TryParse("44051401458", out Pesel pesel))
{
    Console.WriteLine(pesel.BirthDateTime);
}

if (Nip.TryParse("1234563218", out Nip nip))
{
    Console.WriteLine(nip.IssuingTaxOfficePrefix);
}

if (Regon.TryParse("123456785", out Regon regon))
{
    Console.WriteLine(regon.Kind);
}
```

```csharp
using PolishIdentifiers;

ValidationResult<PeselValidationError> peselResult = Pesel.Validate("44051401458");
ValidationResult<NipValidationError> nipResult = Nip.Validate("1234563218");
ValidationResult<RegonValidationError> regonResult = Regon.Validate("12345678512347");

if (peselResult.IsValid && nipResult.IsValid && regonResult.IsValid)
{
    Console.WriteLine("all identifiers are valid");
}
```

Formatted NIP input uses a separate API:

```csharp
using PolishIdentifiers;

Nip nip = Nip.ParseFormatted("PL 123-456-32-18");

Console.WriteLine(nip.ToString());
Console.WriteLine(nip.ToString(NipFormat.Hyphenated));
Console.WriteLine(nip.ToString(NipFormat.VatEu));
```

### Generation

```csharp
using PolishIdentifiers;

Pesel anyPesel = PeselGenerator.Generate();
Pesel knownPesel = PeselGenerator.Generate(Gender.Female, new DateTime(1990, 5, 14));

Nip anyNip = NipGenerator.Generate();

Regon regon9 = RegonGenerator.Generate(RegonKind.Regon9);
Regon regon14 = RegonGenerator.Generate(RegonKind.Regon14);
```

```csharp
using PolishIdentifiers;

string invalidPesel = PeselGenerator.Invalid.WrongChecksum();
string invalidNip = NipGenerator.Invalid.WrongLength();
string invalidRegon = RegonGenerator.Invalid.WrongChecksumRegon14();
```

## API model

| API | Use when | Success | Invalid input | `null` string input |
|---|---|---|---|---|
| `Parse(...)` | Invalid input is exceptional | Returns the strong type | Throws `PeselValidationException`, `NipValidationException`, or `RegonValidationException` | Throws `ArgumentNullException` |
| `TryParse(...)` | Invalid input is expected | Returns `true` and sets the out parameter | Returns `false` and sets the out parameter to `default` | Returns `false` and sets the out parameter to `default` |
| `Validate(...)` | You need a typed error without exceptions | Returns `ValidationResult<TError>.Valid()` | Returns `ValidationResult<TError>.Failure(...)` | Returns a failure result |

`Nip` also has a separate formatted-input path:

| Formatted NIP API | Use when | Invalid input | `null` string input |
|---|---|---|---|
| `ParseFormatted(...)` | The input contract explicitly allows one of the five recognized NIP formats | Throws `NipValidationException` | Throws `ArgumentNullException` |
| `TryParseFormatted(...)` | Formatted NIP input is expected and exceptions are not wanted | Returns `false` and sets the out parameter to `default` | Returns `false` and sets the out parameter to `default` |
| `ValidateFormatted(...)` | You need a typed error for formatted NIP input | Returns `NipValidationError.UnrecognizedFormat`, `InvalidChecksum`, or another NIP error | Returns `NipValidationError.UnrecognizedFormat` |

## Identifier notes

| Type | Input shape | Main properties | Validation order | Special notes |
|---|---|---|---|---|
| `Pesel` | 11 digits | `BirthDateTime`, `Gender` | `InvalidCharacters` -> `InvalidLength` -> `InvalidDate` -> `InvalidChecksum` | `.NET 10` also adds `BirthDateOnly`; generator overloads support birth years 1800-2299 |
| `Nip` | 10 digits | `IssuingTaxOfficePrefix` | `InvalidCharacters` -> `InvalidLength` -> `InvalidChecksum` | Also supports `ParseFormatted(...)`, `TryParseFormatted(...)`, and `ValidateFormatted(...)`; output formats: `DigitsOnly`, `Hyphenated`, `VatEu` |
| `Regon` | 9 or 14 digits | `Kind`, `IsRegon9`, `IsRegon14`, `BaseRegon` | `InvalidCharacters` -> `InvalidLength` -> `InvalidChecksum` | `Regon14` validation checks the embedded `Regon9` base first, then the 14-digit checksum |

Recognized formatted NIP inputs:

| Format | Example |
|---|---|
| Canonical | `1234563218` |
| Hyphenated | `123-456-32-18` |
| `PL` prefix | `PL1234563218` |
| `PL` prefix with space | `PL 1234563218` |
| `PL` prefix with space and hyphens | `PL 123-456-32-18` |

Formatting is strict. Unsupported punctuation, lowercase `pl`, extra spaces, and leading or trailing whitespace are rejected.

## Input and failure behavior

Common rules:

- Leading or trailing whitespace is invalid input
- `default(Pesel)`, `default(Nip)`, and `default(Regon)` are invalid sentinel values
- Calling domain properties or formatting methods on a default value throws `InvalidOperationException`

Important edge cases:

- `Nip.ValidateFormatted(null)` returns `NipValidationError.UnrecognizedFormat`
- `0000000000` is a valid `Nip`
- `000000000` and `00000000000000` are valid `Regon` values
- `00000000000` is not a valid `Pesel`
- For NIP, checksum remainder `10` is invalid
- For REGON, checksum remainder `10` maps to check digit `0`

## DataAnnotations

The package includes:

- `[ValidPesel]`
- `[ValidNip]`
- `[ValidRegon]`

Behavior:

- `null` is treated as valid by these attributes; combine with `[Required]` when the field must be present
- `ValidNipAttribute` accepts the same canonical and recognized formatted string inputs as `Nip.ValidateFormatted(...)`
- The attributes also validate strong-type values, not only strings
- Default strong-type values are treated as invalid

## Generators

The package includes:

- `PeselGenerator`
- `NipGenerator`
- `RegonGenerator`

Use generators for tests, fixtures, and demos.

- Valid generator methods return the strong type
- `Invalid.*` helpers return `string`
- Each `Invalid.*` helper is intended to violate exactly one public validation rule

## Version-specific APIs

Available only on `net10.0`:

- `IParsable<T>` and `ISpanParsable<T>` for all implemented identifiers
- `Pesel.BirthDateOnly`
- `PeselGenerator.Generate(DateOnly)`
- `PeselGenerator.Generate(Gender, DateOnly)`

## Documentation

- Repository docs index: [`docs/README.md`](./docs/README.md)
- Shared behavior reference: [`docs/Common/ValidationAndBehavior.md`](./docs/Common/ValidationAndBehavior.md)
- `Pesel` guides: [`docs/Pesel/`](./docs/Pesel/)
- `Nip` guides: [`docs/Nip/`](./docs/Nip/)
- `Regon` guides: [`docs/Regon/`](./docs/Regon/)

## Non-goals

This library does not:

- query official registries
- verify real-world assignment or current registration status
- accept heuristic normalization beyond the documented `Nip` formatted-input path

## License

MIT
