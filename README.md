# PolishIdentifiers

[![NuGet](https://img.shields.io/nuget/v/PolishIdentifiers.svg)](https://www.nuget.org/packages/PolishIdentifiers/)
[![CI](https://github.com/PolishIdentifiers/PolishIdentifiers/actions/workflows/ci.yml/badge.svg)](https://github.com/PolishIdentifiers/PolishIdentifiers/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue)](LICENSE)
![Targets: netstandard2.0 · net10.0](https://img.shields.io/badge/targets-netstandard2.0%20%7C%20net10.0-informational)

PolishIdentifiers is a .NET library that exposes PESEL, NIP, and REGON as strong identifier types (`Pesel`, `Nip`, `Regon`) instead of raw strings plus includes their strong validation.

These identifiers are implemented as tightly constrained `readonly struct` value types, with one unified `Parse` / `TryParse` / `Validate` API shape across the implemented surface, `TypeConverter` support, DataAnnotations attributes for request validation, generators for valid and intentionally invalid test values, broad unit-test coverage, and `ReadOnlySpan<char>`-based parsing and validation for low-allocation paths.

In practice, that gives you one package for parsing, validation, formatting, generation, ASP.NET Core MVC and Minimal API parameter binding, and request-model validation while keeping strong identifier types throughout domain code.

## Scope

This library covers **strongly typed Polish formal identifiers** — identity, registration, and business numbers used in Polish administrative and commercial contexts.

Currently implemented: `Pesel`, `Nip`, `Regon`.

## Framework support

Targets `netstandard2.0` and `net10.0`.

The `net10.0` build adds `IParsable<T>`, `ISpanParsable<T>`, and `DateOnly`-based PESEL members. All targets include `TryParse(string?, IFormatProvider?, out T)`, which enables ASP.NET Core Minimal API route and query parameter binding without any additional configuration. See [Framework support](./docs/framework-support.md) for all target-specific differences.

## Supported identifiers

| Type | Identifier | Annotation | Accepted formats                                                                   | Docs |
|---|---|---|------------------------------------------------------------------------------------|---|
| `Pesel` | PESEL | `[ValidPesel]` | `44051401458`                                                                      | [PESEL](./docs/pesel.md) |
| `Nip` | NIP | `[ValidNip]` | `1234563218`, `123-456-32-18`, `PL1234563218`, `PL 1234563218`, `PL 123-456-32-18` | [NIP](./docs/nip.md) |
| `Regon` | REGON | `[ValidRegon]` | `123456785`, `12345678512347`                                                      | [REGON](./docs/regon.md) |

## Parse, TryParse, Validate

| Method | Use when |
|---|---|
| `TryParse` | You want a strong type and a non-throwing failure path; use the `out TError? error` overload to also receive the first structured validation error on failure |
| `Parse` | Invalid input is exceptional and should throw an identifier-specific validation exception |
| `Validate` | You only need a pass-or-fail result with the first structured validation error and do not need the typed identifier instance |

## Generators

<table>
    <thead>
        <tr>
            <th>Type</th>
            <th>Kind</th>
            <th>Outputs</th>
            <th>Docs</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td rowspan="2"><code>PeselGenerator</code></td>
            <td>Valid</td>
            <td>PESEL with random or specified birth date and gender</td>
            <td rowspan="2"><a href="./docs/pesel-generator.md">PESEL generator</a></td>
        </tr>
        <tr>
            <td>Invalid</td>
            <td>invalid characters, wrong checksum, wrong date, wrong length</td>
        </tr>
        <tr>
            <td rowspan="2"><code>NipGenerator</code></td>
            <td>Valid</td>
            <td>canonical valid NIP</td>
            <td rowspan="2"><a href="./docs/nip-generator.md">NIP generator</a></td>
        </tr>
        <tr>
            <td>Invalid</td>
            <td>invalid characters, wrong checksum, wrong length</td>
        </tr>
        <tr>
            <td rowspan="2"><code>RegonGenerator</code></td>
            <td>Valid</td>
            <td>REGON-9 and REGON-14</td>
            <td rowspan="2"><a href="./docs/regon-generator.md">REGON generator</a></td>
        </tr>
        <tr>
            <td>Invalid</td>
            <td>invalid characters, wrong checksum for REGON-9, wrong checksum for REGON-14, wrong length</td>
        </tr>
    </tbody>
</table>

## TypeConverters

Each identifier type registers a `TypeConverter`; ASP.NET Core MVC and Minimal APIs support binding through different mechanisms.

| Scenario | netstandard2.0 | net10.0 | Uses TypeConverter? |
|---|---|---|---|
| MVC route/query/form binding | Supported | Supported | Yes |
| Minimal API binding | Supported | Supported | No |

Minimal API binding uses `public static TryParse(string?, IFormatProvider?, out T)` rather than `TypeConverter`.

## Examples

### Parse — when invalid input is a bug

```csharp
using PolishIdentifiers;

var nip = Nip.Parse("1234563218");

Console.WriteLine(nip);                              // 1234563218
Console.WriteLine(nip.ToString(NipFormat.VatEu));   // PL1234563218
```

### TryParse — non-throwing path with typed error

```csharp
using PolishIdentifiers;

if (!Pesel.TryParse("44051401458", out var pesel, out var error))
{
    Console.WriteLine($"Rejected: {error}"); // e.g. InvalidChecksum
    return;
}

Console.WriteLine(pesel.BirthDate.ToString("yyyy-MM-dd")); // 1944-05-14
Console.WriteLine(pesel.Gender);                           // Male
```

```csharp
using PolishIdentifiers;

if (!Nip.TryParse("PL 123-456-32-18", out var nip, out var error))
{
    Console.WriteLine($"Rejected: {error}");
    return;
}

Console.WriteLine(nip);                                   // 1234563218
Console.WriteLine(nip.ToString(NipFormat.Hyphenated));    // 123-456-32-18
```

```csharp
using PolishIdentifiers;

if (!Regon.TryParse("12345678512347", out var regon, out var error))
{
    Console.WriteLine($"Rejected: {error}");
    return;
}

Console.WriteLine(regon.Kind);       // Regon14
Console.WriteLine(regon.BaseRegon9); // 123456785
```

### Validate — check validity without allocating a typed instance

```csharp
using PolishIdentifiers;

var result = Nip.Validate("123-456-32-18");

Console.WriteLine(result.IsValid); // True

var bad = Pesel.Validate("44051401459");

Console.WriteLine(bad.IsValid); // False
Console.WriteLine(bad.Error);   // InvalidChecksum
```

### DataAnnotations attributes

```csharp
using System.ComponentModel.DataAnnotations;
using PolishIdentifiers;

public sealed class InvoiceRequest
{
    [ValidNip]
    public string SellerNip { get; init; } = string.Empty;

    [ValidPesel]
    public string? BuyerPesel { get; init; }

    [ValidRegon]
    public string? SellerRegon { get; init; }
}
```

### Generators

```csharp
using PolishIdentifiers;

// Valid values for seeding test data
var pesel = PeselGenerator.Generate(Gender.Female, new DateTime(1990, 6, 15));
var nip   = NipGenerator.Generate();
var regon = RegonGenerator.Generate(RegonKind.Regon14);

Console.WriteLine(pesel); // e.g. 90061512345
Console.WriteLine(nip);   // e.g. 5261040828
Console.WriteLine(regon); // e.g. 12345678512347

// Intentionally invalid strings for negative test cases
string badPesel = PeselGenerator.Invalid.WrongChecksum();
string badNip   = NipGenerator.Invalid.WrongChecksum();
string badRegon = RegonGenerator.Invalid.WrongChecksumRegon9();
```

### MVC / Controllers

```csharp
using Microsoft.AspNetCore.Mvc;
using PolishIdentifiers;

[HttpGet("{nip}")]
public IActionResult Get(Nip nip) => Ok(nip);

// Query string
[HttpGet]
public IActionResult Find([FromQuery] Pesel pesel) => Ok(pesel);

// Minimal API
app.MapGet("/companies/{regon}", (Regon regon) => Results.Ok(regon));
```

For route, query-string, and form binding, the types bind from a single string via `TypeConverter` / `TryParse`. For JSON request bodies, add JSON converters in your application.
