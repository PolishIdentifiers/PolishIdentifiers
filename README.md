# PolishIdentifiers

[![NuGet](https://img.shields.io/nuget/v/PolishIdentifiers.svg)](https://www.nuget.org/packages/PolishIdentifiers/)
![CI](https://github.com/PolishIdentifiers/PolishIdentifiers/actions/workflows/ci.yml/badge.svg?branch=main)

PolishIdentifiers is a .NET library for strongly typed handling of PESEL, NIP, and REGON.

It provides one unified `Parse` / `TryParse` / `Validate` API shape across the implemented identifiers, DataAnnotations attributes for request validation, strong identifier types instead of raw strings in domain code, generators for valid and intentionally invalid test values, broad unit-test coverage, and `ReadOnlySpan<char>`-based parsing and validation for low-allocation paths. In practice, that makes it a single package for parsing, validation, formatting, generation, and request-model validation across the implemented identifiers.

## Framework support

Targets `netstandard2.0` and `net10.0`. The `net10.0` build adds `IParsable<T>`, `ISpanParsable<T>`, and `DateOnly`-based PESEL members. See [Framework support](./docs/framework-support.md) for all target-specific differences.

## Supported identifiers

| Type | Identifier | Accepted examples | Docs |
|---|---|---|---|
| `Pesel` | PESEL | `44051401458` | [PESEL](./docs/pesel.md) |
| `Nip` | NIP | `1234563218`, `123-456-32-18`, `PL1234563218`, `PL 1234563218`, `PL 123-456-32-18` | [NIP](./docs/nip.md) |
| `Regon` | REGON | `123456785`, `12345678512347` | [REGON](./docs/regon.md) |

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

## Install

```powershell
dotnet add package PolishIdentifiers
```

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

## Parse, TryParse, or Validate?

- Use `TryParse` when you want a strong type and a non-throwing failure path.
- Use `Parse` when invalid input is exceptional and should throw an identifier-specific validation exception.
- Use `Validate` when you only need a pass-or-fail result with the first structured validation error and do not need the typed identifier instance.

```csharp
using PolishIdentifiers;

var result = Nip.Validate("123-456-32-18");

if (!result.IsValid)
{
    Console.WriteLine(result.Error);
}
```
