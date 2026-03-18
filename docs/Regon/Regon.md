# Regon

This file describes the `Regon` strong type API.

## Parse(string)

Framework support: `netstandard2.0`, `net10.0`

Use `Parse(string)` when the caller already expects a valid 9-digit or 14-digit REGON and invalid values should throw.

## Parse(ReadOnlySpan<char>)

Framework support: `netstandard2.0`, `net10.0`

Use this overload in span-based parsing code.

## TryParse(string?, out Regon)

Framework support: `netstandard2.0`, `net10.0`

Use `TryParse` when invalid REGON input is part of normal control flow.

## TryParse(ReadOnlySpan<char>, out Regon)

Framework support: `netstandard2.0`, `net10.0`

Use this overload when the source data is already span-based.

## Kind

Framework support: `netstandard2.0`, `net10.0`

Use `Kind` when the workflow needs to distinguish `Regon9` from `Regon14` explicitly.

```csharp
using PolishIdentifiers;

Regon regon = Regon.Parse("12345678512347");
Console.WriteLine(regon.Kind);
```

## IsRegon9

Framework support: `netstandard2.0`, `net10.0`

Use `IsRegon9` when you need a simple branch without comparing the enum value directly.

## IsRegon14

Framework support: `netstandard2.0`, `net10.0`

Use `IsRegon14` for the same reason when the workflow is specific to the 14-digit variant.

## BaseRegon9

Framework support: `netstandard2.0`, `net10.0`

Use `BaseRegon` when downstream logic needs the embedded REGON-9 base from a REGON-14 value.

```csharp
using PolishIdentifiers;

Regon regon = Regon.Parse("12345678512347");
Console.WriteLine(regon.BaseRegon);
```

This is useful for grouping, reporting, and workflows centered on the base entity rather than the branch-level variant.

## ToString()

Framework support: `netstandard2.0`, `net10.0`

Use `ToString()` when you need canonical output.

## ToString(string?, IFormatProvider?)

Framework support: `netstandard2.0`, `net10.0`

Use this overload only when integrating with general formatting infrastructure.

## IParsable<Regon> and ISpanParsable<Regon>

Framework support: `net10.0`

Use the generic parsing interfaces in shared `.NET 10` infrastructure code.

## Enterprise example

Framework support: `netstandard2.0`, `net10.0`

Keep `Regon` in the domain model so variant information remains explicit and does not depend on repeated string-length checks.

## Related docs

- [validation guidance](./RegonValidate.md)
- [generation guidance](./RegonGenerate.md)