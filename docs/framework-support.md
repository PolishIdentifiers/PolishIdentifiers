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