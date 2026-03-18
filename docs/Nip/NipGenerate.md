# Nip Generate

This file describes the public NIP generation entry points.

## Generate()

Framework support: `netstandard2.0`, `net10.0`

Use `Generate()` when you need any valid NIP for tests, fixtures, or demos.

```csharp
using PolishIdentifiers;

Nip nip = NipGenerator.Generate();
Console.WriteLine(nip);
```

## Invalid.WrongChecksum()

Framework support: `netstandard2.0`, `net10.0`

Use this helper when a test must fail with `NipValidationError.InvalidChecksum`.

## Invalid.WrongLength()

Framework support: `netstandard2.0`, `net10.0`

Use this helper when a test must fail with `NipValidationError.InvalidLength`.

## Invalid.NonNumeric()

Framework support: `netstandard2.0`, `net10.0`

Use this helper when a test must fail with `NipValidationError.InvalidCharacters`.

## Enterprise example

Framework support: `netstandard2.0`, `net10.0`

Use generated values in supplier, invoice, and tax-related fixtures where a valid NIP is required but the exact value is not important.

```csharp
using PolishIdentifiers;

public sealed class SupplierFixture
{
    public SupplierFixture()
    {
        Nip = NipGenerator.Generate();
    }

    public Nip Nip { get; }
}
```

## Related docs

- [strong type guidance](./Nip.md)
- [validation guidance](./NipValidate.md)