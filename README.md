[![NuGet](https://img.shields.io/nuget/v/PolishIdentifiers.svg)](https://www.nuget.org/packages/PolishIdentifiers/)
[![CI](https://github.com/PolishIdentifiers/PolishIdentifiers/actions/workflows/ci.yml/badge.svg)](https://github.com/PolishIdentifiers/PolishIdentifiers/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue)](LICENSE)
![Targets: netstandard2.0 · net10.0](https://img.shields.io/badge/targets-netstandard2.0%20%7C%20net10.0-informational)

# PolishIdentifiers

PolishIdentifiers is a .NET library that exposes PESEL, NIP, and REGON as strong identifier types (`Pesel`, `Nip`, `Regon`) instead of raw strings plus includes their strong validation.

## Available NuGet Packages

| Package | Description |
|---|---|
| [NuGet PolishIdentifiers](https://www.nuget.org/packages/PolishIdentifiers/) | Core library - strongly typed PESEL, NIP, and REGON with validation, parsing, formatting, generators, and DataAnnotations attributes |
| [NuGet PolishIdentifiers.SystemTextJson](https://www.nuget.org/packages/PolishIdentifiers.SystemTextJson/) | Integration package for `System.Text.Json` converters for `Pesel`, `Nip`, and `Regon` |

## Design decisions

- Strong domain types first: PESEL, NIP, and REGON are exposed as dedicated identifier types (`Pesel`, `Nip`, `Regon`) instead of raw strings.
- Constrained value semantics: implemented identifiers are tightly constrained `readonly struct` value types to keep invalid states out of normal domain flow.
- One consistent contract: the implemented surface uses a unified `Parse` / `TryParse` / `Validate` shape for predictable usage across identifiers.
- Explicit failure paths: `TryParse` and `Validate` provide non-throwing validation flows, while `Parse` is reserved for exception-based paths.
- Framework-friendly core: `TypeConverter` support and DataAnnotations attributes are built in so the same types work in request validation and binding scenarios.
- Practical binding support: `TryParse(string?, IFormatProvider?, out T)` is available on all targets for ASP.NET Core Minimal API route and query parameter binding without extra configuration.
- Performance-aware validation: parsing and validation include `ReadOnlySpan<char>`-based paths for low-allocation usage.
- Testability by design: generators produce valid and intentionally invalid values, backed by broad unit-test coverage.
- Clear package boundaries: core parsing and validation live in the main package, while `System.Text.Json` converters are delivered in the separate `PolishIdentifiers.SystemTextJson` package.
- Multi-target strategy: the library targets `netstandard2.0` and `net10.0`; the `net10.0` build adds `IParsable<T>`, `ISpanParsable<T>`, and `DateOnly`-based PESEL members.

### In practice

- You get one core package for parsing, validation, formatting, generation, parameter binding, and request-model validation.
- You keep strong identifier types throughout application and domain code instead of falling back to untyped string handling.

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

Each generator can produce a single identifier or generate multiple identifiers at once with the `count` parameter. Batch overloads return `IReadOnlyList<T>` for valid identifiers and `IReadOnlyList<string>` for intentionally invalid generator outputs.

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
            <td>invalid characters, wrong checksum, wrong length, unrecognized format</td>
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

### Parse

```csharp
using PolishIdentifiers;

var nip = Nip.Parse("1234563218");

Console.WriteLine(nip);                              // 1234563218
Console.WriteLine(nip.ToString(NipFormat.VatEu));   // PL1234563218
```

### TryParse

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

// Other accepted NIP inputs: "1234563218", "123-456-32-18", "PL1234563218", "PL 1234563218"
if (!Nip.TryParse("PL 123-456-32-18", out var nip, out var error))
{
    Console.WriteLine($"Rejected: {error}");
    return;
}

Console.WriteLine(nip);                                   // 1234563218
Console.WriteLine(nip.ToString(NipFormat.DigitsOnly));    // 1234563218
Console.WriteLine(nip.ToString(NipFormat.Hyphenated));    // 123-456-32-18
Console.WriteLine(nip.ToString(NipFormat.VatEu));         // PL1234563218
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

### Validate

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

// Generate N identifiers at once — returns IReadOnlyList<T>
IReadOnlyList<Pesel> pesels = PeselGenerator.Generate(Gender.Female, new DateTime(1990, 6, 15), count: 10);
IReadOnlyList<Nip>   nips   = NipGenerator.Generate(count: 10);
IReadOnlyList<Regon> regons = RegonGenerator.Generate(RegonKind.Regon9, count: 10);

// Same for invalid batches
IReadOnlyList<string> badPesels = PeselGenerator.Invalid.WrongChecksum(count: 10);
IReadOnlyList<string> badNips   = NipGenerator.Invalid.WrongChecksum(count: 10);
IReadOnlyList<string> badRegons = RegonGenerator.Invalid.WrongChecksumRegon9(count: 10);
```

### MVC / Controllers

```csharp
using Microsoft.AspNetCore.Mvc;
using PolishIdentifiers;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();
app.MapGet("/companies/{regon}", (Regon regon) => Results.Ok(regon));

app.Run();

[ApiController]
[Route("api/identifiers")]
public sealed class IdentifiersController : ControllerBase
{
    [HttpGet("nips/{nip}")]
    public IActionResult GetNip(Nip nip) => Ok(nip);

    [HttpGet("persons")]
    public IActionResult Find([FromQuery] Pesel pesel) => Ok(pesel);
}
```

For route, query-string, and form binding, the types bind from a single string via `TypeConverter` / `TryParse`. For JSON request bodies, install the optional `PolishIdentifiers.SystemTextJson` package and register its converters.

## System.Text.Json integration

JSON converters for `Pesel`, `Nip`, and `Regon` are shipped in the separate `PolishIdentifiers.SystemTextJson` package. It provides `NipJsonConverter`, `PeselJsonConverter`, `RegonJsonConverter`, the `AddPolishIdentifiers()` extension method, and `PolishIdentifiersJsonOptions` for configuring NIP output formatting.

Polish identifiers are always serialized as JSON strings, never numbers or objects.

### Install

```bash
dotnet add package PolishIdentifiers.SystemTextJson
```

### Register and use

```csharp
using System.Text.Json;
using PolishIdentifiers;

var options = new JsonSerializerOptions().AddPolishIdentifiers(
    new PolishIdentifiersJsonOptions
    {
        NipOutputFormat = NipFormat.VatEu
    });

public sealed record CompanyDto
{
    public string Name { get; init; } = "";
    public Nip Nip { get; init; }
    public Nip? OptionalNip { get; init; }
}

var company = JsonSerializer.Deserialize<CompanyDto>(
    """{"Name":"Acme","Nip":"123-456-32-18"}""",
    options);

Console.WriteLine(company!.Nip); // 1234563218

try
{
    JsonSerializer.Deserialize<CompanyDto>(
        """{"Name":"Acme","Nip":"123-456-32-17"}""",
        options);
}
catch (JsonException ex) when (ex.InnerException is NipValidationException nipEx)
{
    Console.WriteLine(nipEx.Error); // InvalidChecksum
}
```

### Converter registration behavior

> `AddPolishIdentifiers()` checks only for the presence of its own converter types (`NipJsonConverter`, `PeselJsonConverter`, `RegonJsonConverter`). If you have registered a custom converter for `Nip` of a different type, this method will still add its own `NipJsonConverter` — both converters will be present in the list. `System.Text.Json` converter resolution is determined by position in `JsonSerializerOptions.Converters`. Register your custom converter after `AddPolishIdentifiers()` if you want it to take priority, or before it if the built-in converter should win.

### Error handling and privacy

- `null`, numbers, objects, arrays, empty strings, and whitespace-only strings throw `JsonException` with no inner exception
- invalid identifier values throw `JsonException` whose `InnerException` is the corresponding `*ValidationException`
- exception messages intentionally do **not** contain raw identifier values, which helps reduce accidental exposure of GDPR/RODO-sensitive data
- in application logs, prefer no raw PESEL/NIP/REGON value at all; if diagnostics are required, apply redaction or hashing at the application boundary

### Pitfalls

1. Missing JSON field for a non-nullable identifier lets `System.Text.Json` assign `default(Nip)` / `default(Pesel)` / `default(Regon)` without invoking the converter. The next serialization then throws. Use `Nip?`, `[JsonRequired]`, or required members as appropriate.
2. A second call to `AddPolishIdentifiers()` with a different `NipOutputFormat` is ignored; the first registration wins.
3. Custom converter ordering matters; see **Converter registration behavior** above.

