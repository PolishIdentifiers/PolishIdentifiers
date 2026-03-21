# PolishIdentifiers

[![NuGet](https://img.shields.io/nuget/v/PolishIdentifiers.svg)](https://www.nuget.org/packages/PolishIdentifiers/)
[![CI](https://github.com/PolishIdentifiers/PolishIdentifiers/actions/workflows/ci.yml/badge.svg)](https://github.com/PolishIdentifiers/PolishIdentifiers/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue)](LICENSE)
![Targets: netstandard2.0 · net10.0](https://img.shields.io/badge/targets-netstandard2.0%20%7C%20net10.0-informational)

PolishIdentifiers is a .NET library that exposes PESEL, NIP, and REGON as strong identifier types (`Pesel`, `Nip`, `Regon`) instead of raw strings plus includes their strong validation.

These identifiers are implemented as tightly constrained `readonly struct` value types, with one unified `Parse` / `TryParse` / `Validate` API shape across the implemented surface, DataAnnotations attributes for request validation, generators for valid and intentionally invalid test values, broad unit-test coverage, and `ReadOnlySpan<char>`-based parsing and validation for low-allocation paths.

In practice, that gives you one package for parsing, validation, formatting, generation, and request-model validation while keeping strong identifier types throughout domain code.

## Scope

This library covers **strongly typed Polish formal identifiers** — identity, registration, and business numbers used in Polish administrative and commercial contexts.

Currently implemented: `Pesel`, `Nip`, `Regon`.

Internally documented and planned for future releases: Polish bank account number (NRB), Polish ID card number, Polish passport number, land register number. These are not yet available as public types.

## Framework support

Targets `netstandard2.0` and `net10.0`.

The `net10.0` build adds `IParsable<T>`, `ISpanParsable<T>`, and `DateOnly`-based PESEL members. See [Framework support](./docs/framework-support.md) for all target-specific differences.

## Supported identifiers

| Type | Identifier | Accepted formats                                                                   | Docs |
|---|---|------------------------------------------------------------------------------------|---|
| `Pesel` | PESEL | `44051401458`                                                                      | [PESEL](./docs/pesel.md) |
| `Nip` | NIP | `1234563218`, `123-456-32-18`, `PL1234563218`, `PL 1234563218`, `PL 123-456-32-18` | [NIP](./docs/nip.md) |
| `Regon` | REGON | `123456785`, `12345678512347`                                                      | [REGON](./docs/regon.md) |

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

## Quick introduction

PolishIdentifiers is designed around one consistent flow for each implemented identifier:

1. accept text input
2. validate or parse using the identifier type
3. keep the parsed identifier as a strong type in application code
4. format it explicitly when producing output

For request handling, forms, imports, and DTO validation, prefer `TryParse(..., out value, out error)`.

```csharp
using PolishIdentifiers;

var input = "44051401458";

if (!Pesel.TryParse(input, out var pesel, out var error))
{
    Console.WriteLine($"Rejected PESEL: {error}");
    return;
}

Console.WriteLine($"Birth date: {pesel.BirthDate:yyyy-MM-dd}");
Console.WriteLine($"Gender: {pesel.Gender}");
```

```csharp
using PolishIdentifiers;

var input = "PL 123-456-32-18";

if (!Nip.TryParse(input, out var nip, out var error))
{
    Console.WriteLine($"Rejected NIP: {error}");
    return;
}

Console.WriteLine($"Canonical: {nip}");
Console.WriteLine($"Display: {nip.ToString(NipFormat.Hyphenated)}");
```

```csharp
using PolishIdentifiers;

var input = "12345678512347";

if (!Regon.TryParse(input, out var regon, out var error))
{
    Console.WriteLine($"Rejected REGON: {error}");
    return;
}

Console.WriteLine($"Kind: {regon.Kind}");
Console.WriteLine($"Base REGON-9: {regon.BaseRegon9}");
```

For ASP.NET Core request models, the package also includes `[ValidPesel]`, `[ValidNip]`, and `[ValidRegon]` attributes for DataAnnotations-based validation.

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

## Common patterns

### Deduplicating records by NIP

`Nip` is a value type with correct equality semantics. Two `Nip` values parsed from different format representations of the same 10-digit number are equal. Use a `HashSet<Nip>` or `Dictionary<Nip, T>` to deduplicate without format-specific string comparisons:

```csharp
using PolishIdentifiers;

var seen = new HashSet<Nip>();

foreach (var raw in importedNipValues)
{
    if (!Nip.TryParse(raw.Trim().ToUpperInvariant(), out var nip, out _))
        continue;

    if (!seen.Add(nip))
        Console.WriteLine($"Duplicate NIP: {nip}");
}
```

### Linking REGON branches to their parent company

REGON-14 identifies a local organizational unit. Its first 9 digits are the REGON-9 of the parent entity. Use `BaseRegon9` to group branches under their parent:

```csharp
using PolishIdentifiers;

var entities = new Dictionary<Regon, List<Regon>>();

foreach (var raw in importedRegonValues)
{
    if (!Regon.TryParse(raw, out var regon, out _))
        continue;

    var parentKey = regon.BaseRegon9; // equals regon itself for REGON-9
    if (!entities.TryGetValue(parentKey, out var units))
        entities[parentKey] = units = [];
    units.Add(regon);
}
```

See [docs/patterns.md](./docs/patterns.md) for persistence, import loop, and person-vs-company decision patterns.

## Design philosophy

Domain properties on identifier types expose values that are **direct structural decodes of the identifier's own digit fields**: `Pesel.BirthDate` and `Pesel.Gender` are read directly from encoded digit positions in the PESEL number.

Derivations that require external state — such as calculating age from birth date, or checking eligibility based on a reference date — are **consumer responsibilities**. These are not added to the library. See [docs/pesel.md](./docs/pesel.md#age-and-other-derivations) for the recommended pattern.
