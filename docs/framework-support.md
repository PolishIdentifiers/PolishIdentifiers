# Framework Support

## Target frameworks

The public package targets:

- `netstandard2.0`
- `net10.0`

The goal is to keep the core identifier API usable in older .NET-compatible application stacks while exposing newer .NET parsing interfaces on the modern target.

## Available across both targets

The common surface available on both targets includes:

- `Pesel`, `Nip`, and `Regon`
- `Parse`, `TryParse`, and `Validate`
- `TryParse(string?, IFormatProvider?, out T)` — ASP.NET Core Minimal API binding convention
- typed validation exceptions
- structured validation results and identifier-specific error enums
- DataAnnotations attributes
- generators
- NIP formatting support
- `ReadOnlySpan<char>` parsing and validation overloads
- canonical `ToString()` output on each identifier type

## net10.0-specific additions

On `net10.0`, the implemented identifiers also support generic parsing interfaces and PESEL date-only members.

### Common `net10.0` additions

| Member group | Types |
|---|---|
| `IParsable<T>` | `Pesel`, `Nip`, `Regon` |
| `ISpanParsable<T>` | `Pesel`, `Nip`, `Regon` |

### PESEL-specific `net10.0` additions

| Member | Availability |
|---|---|
| `Pesel.BirthDateOnly` | `net10.0` only |
| `PeselGenerator.Generate(DateOnly)` | `net10.0` only |
| `PeselGenerator.Generate(Gender, DateOnly)` | `net10.0` only |

All other public `Pesel`, `Nip`, and `Regon` members are available on both `netstandard2.0` and `net10.0`.

## Span-based APIs

Where relevant, the library exposes `ReadOnlySpan<char>` overloads for parsing and validation.

This matters most for higher-throughput or allocation-sensitive code paths. For normal business application usage, the string overloads remain the most direct starting point.

## Compatibility guidance

- Choose the common `Parse`, `TryParse`, and `Validate` APIs if you want code that behaves consistently across supported target frameworks.
- Use the `net10.0`-only additions when your application already targets `net10.0` and benefits from those interfaces or `DateOnly`-specific convenience APIs.

## ASP.NET Core Minimal API parameter binding

ASP.NET Core MVC and Minimal APIs use different mechanisms to bind route and query parameters to custom types.

| Scenario | Mechanism | Available on |
|---|---|---|
| MVC `[FromQuery]`, `[FromRoute]`, `[FromForm]` | `TypeConverter` or `TryParse` | `netstandard2.0`, `net10.0` |
| MVC controller binding on modern ASP.NET Core | `IParsable<T>.TryParse` | `net10.0` only |
| Minimal API route/query/header binding | public static `TryParse(...)` or `BindAsync` | `netstandard2.0`, `net10.0` |

According to Microsoft Learn:

- MVC model binding treats a custom type as a simple type when it can convert it from a single string using `TypeConverter` or `TryParse`.
- MVC controller binding on modern .NET also supports `IParsable<TSelf>.TryParse`.
- Minimal APIs bind custom route, query, and header values through a valid public static `TryParse` method, `BindAsync`, or `IBindableFromHttpContext<TSelf>`.

All three identifier types expose `public static bool TryParse(string? value, IFormatProvider? _, out T result)` on both targets, so Minimal API route and query parameter binding works without any additional configuration:

```csharp
using PolishIdentifiers;

var app = WebApplication.Create(args);

app.MapGet("/companies/{nip}", (Nip nip) => nip.ToString());
app.MapGet("/persons/{pesel}", (Pesel pesel) => pesel.BirthDate.ToShortDateString());
app.MapGet("/entities/{regon}", (Regon regon) => regon.Kind.ToString());

app.Run();
```

`TypeConverter` is registered on all identifier types and continues to serve MVC binding on both targets.

This does not by itself cover JSON request-body binding. For `[FromBody]` MVC parameters or Minimal API body binding, ASP.NET Core uses input formatters and JSON serialization rather than `TypeConverter` or `TryParse`. Those scenarios require a JSON converter if direct identifier serialization and deserialization is needed.