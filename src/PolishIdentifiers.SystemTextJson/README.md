# PolishIdentifiers.SystemTextJson

`PolishIdentifiers.SystemTextJson` adds `System.Text.Json` support for the strong identifier types from `PolishIdentifiers`.

It provides converters for `Pesel`, `Nip`, and `Regon`, plus a single registration method for `JsonSerializerOptions`.

It extends the core package: [PolishIdentifiers](https://www.nuget.org/packages/PolishIdentifiers).

## Install

```bash
dotnet add package PolishIdentifiers.SystemTextJson
```

## Getting Started

```csharp
using System.Text.Json;
using PolishIdentifiers;

var options = new JsonSerializerOptions()
    .AddPolishIdentifiers(new PolishIdentifiersJsonOptions
    {
        NipOutputFormat = NipFormat.Hyphenated
    });

var company = new CompanyDto(
    "Acme Sp. z o.o.",
    Nip.Parse("1234563218"),
    Regon.Parse("123456785"),
    Pesel.Parse("44051401458"));

var json = JsonSerializer.Serialize(company, options);
var roundTrip = JsonSerializer.Deserialize<CompanyDto>(json, options);

public sealed record CompanyDto(
    string CompanyName,
    Nip Nip,
    Regon Regon,
    Pesel Pesel);
```

## What It Adds

- `NipJsonConverter`
- `PeselJsonConverter`
- `RegonJsonConverter`
- `AddPolishIdentifiers(this JsonSerializerOptions, PolishIdentifiersJsonOptions?)`
- `PolishIdentifiersJsonOptions` for NIP output formatting

## Behavior Notes

- Polish identifiers are always serialized as JSON strings.
- `NipJsonConverter` can write NIP values as `DigitsOnly`, `Hyphenated`, or `VatEu`.
- `AddPolishIdentifiers()` is idempotent for the built-in converter types.
- First registration wins. If `AddPolishIdentifiers()` is called twice with different NIP formatting options, the first `NipJsonConverter` remains active.
- Converter detection is type-specific. If you register a custom converter of a different type, `AddPolishIdentifiers()` can still add the built-in converter, and `System.Text.Json` resolution then depends on converter order.

## Error Handling And Privacy

- Invalid token types, empty strings, and whitespace-only strings throw `JsonException`.
- Domain-invalid values throw `JsonException` with the corresponding `*ValidationException` in `InnerException`.
- Exception messages are designed not to include raw PESEL, NIP, or REGON values.
- For optional JSON fields, prefer `Nip?`, `Pesel?`, or `Regon?` instead of default struct instances.

## Documentation

- Core package: https://www.nuget.org/packages/PolishIdentifiers/
- Repository: https://github.com/PolishIdentifiers/PolishIdentifiers
- Root documentation: https://github.com/PolishIdentifiers/PolishIdentifiers/blob/main/README.md
- JSON example project: https://github.com/PolishIdentifiers/PolishIdentifiers/blob/main/examples/PolishIdentifiers.Examples/JsonConverter/JsonConverterExamples.cs
- Changelog: https://github.com/PolishIdentifiers/PolishIdentifiers/blob/main/CHANGELOG.md

## Feedback

Report bugs or request improvements at:

- https://github.com/PolishIdentifiers/PolishIdentifiers/issues