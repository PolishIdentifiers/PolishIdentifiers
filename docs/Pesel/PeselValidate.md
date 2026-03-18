# Pesel Validate

This file describes the public PESEL validation entry points.

## Validate(string?)

Framework support: `netstandard2.0`, `net10.0`

Use `Validate(string?)` when the caller needs a typed error and the input is available as `string`.

```csharp
using PolishIdentifiers;

ValidationResult<PeselValidationError> result = Pesel.Validate("440514X1458");
Console.WriteLine(result.Error); // InvalidCharacters
```

This is the best choice for APIs, forms, and import jobs that must report why validation failed.

## Validate(ReadOnlySpan<char>)

Framework support: `netstandard2.0`, `net10.0`

Use the span overload when validation happens in low-allocation parsing flows.

## ValidationResult<TError>

Framework support: `netstandard2.0`, `net10.0`

Use `ValidationResult<TError>` when you need to branch on success or a typed error without exceptions.

```csharp
using PolishIdentifiers;

ValidationResult<PeselValidationError> result = Pesel.Validate("44051401458");

string outcome = result.Match(
    onValid: () => "ok",
    onError: error => $"invalid:{error}");
```

## ValidPeselAttribute

Framework support: `netstandard2.0`, `net10.0`

Use `[ValidPesel]` on DTO or input-model properties at the application boundary.

```csharp
using System.ComponentModel.DataAnnotations;
using PolishIdentifiers;

public sealed class PersonInput
{
    [Required]
    [ValidPesel]
    public string Pesel { get; set; } = string.Empty;
}
```

Combine it with `[Required]` when the field must be present, because `[ValidPesel]` treats `null` as valid.

## Validation order

Framework support: `netstandard2.0`, `net10.0`

The public validation contract returns the first failing rule in this order:

1. `InvalidCharacters`
2. `InvalidLength`
3. `InvalidDate`
4. `InvalidChecksum`

Use this ordering when mapping validation errors to API responses or UI messages.

## Enterprise example

Framework support: `netstandard2.0`, `net10.0`

Validate first when the caller needs a machine-readable error rather than an exception.

```csharp
using PolishIdentifiers;

public static string ValidateForApi(string input)
{
    ValidationResult<PeselValidationError> result = Pesel.Validate(input);

    return result.Match(
        onValid: () => "ok",
        onError: error => $"invalid:{error}");
}
```

## Related docs

- [strong type guidance](./Pesel.md)
- [generation guidance](./PeselGenerate.md)