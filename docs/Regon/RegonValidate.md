# Regon Validate

This file describes the public REGON validation entry points.

## Validate(string?)

Framework support: `netstandard2.0`, `net10.0`

Use `Validate(string?)` when the caller needs a typed validation result instead of an exception.

```csharp
using PolishIdentifiers;

ValidationResult<RegonValidationError> result = Regon.Validate("123456780");
Console.WriteLine(result.Error); // InvalidChecksum
```

## Validate(ReadOnlySpan<char>)

Framework support: `netstandard2.0`, `net10.0`

Use this overload in span-based parsing and validation code.

## ValidRegonAttribute

Framework support: `netstandard2.0`, `net10.0`

Use `[ValidRegon]` on DTO properties or parameters that still carry REGON values as strings.

```csharp
using System.ComponentModel.DataAnnotations;
using PolishIdentifiers;

public sealed class CompanyInput
{
    [Required]
    [ValidRegon]
    public string Regon { get; set; } = string.Empty;
}
```

## Validation contract for REGON-14

Framework support: `netstandard2.0`, `net10.0`

For a 14-digit REGON, the public validation contract also checks the embedded REGON-9 base as part of checksum validation.

Use this knowledge when interpreting failures in import reports or API responses.

## Enterprise example

Framework support: `netstandard2.0`, `net10.0`

Validate first when imports or APIs must return machine-readable failures without constructing the domain type yet.

## Related docs

- [strong type guidance](./Regon.md)
- [generation guidance](./RegonGenerate.md)