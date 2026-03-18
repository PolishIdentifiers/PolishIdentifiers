# Regon Generate

This file describes the public REGON generation entry points.

## Generate(RegonKind)

Framework support: `netstandard2.0`, `net10.0`

Use `Generate(RegonKind.Regon9)` when the test expects a 9-digit value.

Use `Generate(RegonKind.Regon14)` when the test expects a 14-digit value.

```csharp
using PolishIdentifiers;

Regon regon9 = RegonGenerator.Generate(RegonKind.Regon9);
Regon regon14 = RegonGenerator.Generate(RegonKind.Regon14);
```

## Invalid.WrongChecksumRegon9()

Framework support: `netstandard2.0`, `net10.0`

Use this helper when a test must fail with `RegonValidationError.InvalidChecksum` for a 9-digit value.

## Invalid.WrongChecksumRegon14()

Framework support: `netstandard2.0`, `net10.0`

Use this helper when a test must fail with `RegonValidationError.InvalidChecksum` for a 14-digit value whose embedded base remains valid.

## Invalid.WrongLength()

Framework support: `netstandard2.0`, `net10.0`

Use this helper when a test must fail with `RegonValidationError.InvalidLength`.

## Invalid.NonNumeric()

Framework support: `netstandard2.0`, `net10.0`

Use this helper when a test must fail with `RegonValidationError.InvalidCharacters`.

## Enterprise example

Framework support: `netstandard2.0`, `net10.0`

Use variant-specific generators in company and partner fixtures so tests remain explicit about whether they are exercising REGON-9 or REGON-14 behavior.

## Related docs

- [strong type guidance](./Regon.md)
- [validation guidance](./RegonValidate.md)