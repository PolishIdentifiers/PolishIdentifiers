# Nip Validate

This file describes the public NIP validation entry points.

## Validate(string?)

Framework support: `netstandard2.0`, `net10.0`

Use `Validate(string?)` when the input contract requires canonical digits-only NIP input.

```csharp
using PolishIdentifiers;

ValidationResult<NipValidationError> result = Nip.Validate("1234563219");
Console.WriteLine(result.Error); // InvalidChecksum
```

## Validate(ReadOnlySpan<char>)

Framework support: `netstandard2.0`, `net10.0`

Use this overload in span-based parsing or validation pipelines.

## ValidateFormatted(string?)

Framework support: `netstandard2.0`, `net10.0`

Use `ValidateFormatted(string?)` when the boundary accepts one of the five supported formatted inputs.

```csharp
using PolishIdentifiers;

ValidationResult<NipValidationError> result = Nip.ValidateFormatted("PL 123-456-32-18");
Console.WriteLine(result.IsValid);
```

Use this for user-facing inputs and partner integrations that may include separators or the `PL` prefix.

## ValidateFormatted(ReadOnlySpan<char>)

Framework support: `netstandard2.0`, `net10.0`

Use this overload when the formatted input is already span-based.

## UnrecognizedFormat

Framework support: `netstandard2.0`, `net10.0`

`UnrecognizedFormat` is returned only by the formatted validation path. Use it when you need to distinguish unsupported formatting from checksum or length failures.

## ValidNipAttribute

Framework support: `netstandard2.0`, `net10.0`

Use `[ValidNip]` on input DTOs that carry string-based NIP values.

```csharp
using System.ComponentModel.DataAnnotations;
using PolishIdentifiers;

public sealed class CompanyInput
{
    [Required]
    [ValidNip]
    public string Nip { get; set; } = string.Empty;
}
```

`[ValidNip]` accepts the same canonical and recognized formatted inputs as `Nip.ValidateFormatted(...)`.

## Enterprise example

Framework support: `netstandard2.0`, `net10.0`

Use `ValidateFormatted(...)` to keep import contracts explicit instead of heuristically stripping any punctuation.

```csharp
using PolishIdentifiers;

public static string ValidateImportedNip(string input)
{
    ValidationResult<NipValidationError> result = Nip.ValidateFormatted(input);

    return result.Match(
        onValid: () => "ok",
        onError: error => $"invalid:{error}");
}
```

## Related docs

- [strong type guidance](./Nip.md)
- [generation guidance](./NipGenerate.md)