# Nip

This file describes the `Nip` strong type API.

## Parse(string)

Framework support: `netstandard2.0`, `net10.0`

Use `Parse(string)` when the input contract already guarantees canonical 10-digit input and invalid data should throw.

## Parse(ReadOnlySpan<char>)

Framework support: `netstandard2.0`, `net10.0`

Use this overload in span-based or low-allocation parsing code.

## ParseFormatted(string)

Framework support: `netstandard2.0`, `net10.0`

Use `ParseFormatted(string)` when the boundary accepts one of the five supported display formats.

```csharp
using PolishIdentifiers;

Nip nip = Nip.ParseFormatted("PL 123-456-32-18");
Console.WriteLine(nip);
```

Use this only when the transport contract explicitly allows formatted NIP input.

## ParseFormatted(ReadOnlySpan<char>)

Framework support: `netstandard2.0`, `net10.0`

Use this overload for the same scenario when the source is already span-based.

## TryParse(string?, out Nip)

Framework support: `netstandard2.0`, `net10.0`

Use `TryParse` for canonical machine input when invalid values are part of normal control flow.

## TryParseFormatted(string?, out Nip)

Framework support: `netstandard2.0`, `net10.0`

Use `TryParseFormatted` for user-facing or partner-facing inputs that may contain approved formatting.

## IssuingTaxOfficePrefix

Framework support: `netstandard2.0`, `net10.0`

Use this property when the workflow needs the original issuing tax office prefix encoded in the first three digits.

```csharp
using PolishIdentifiers;

Nip nip = Nip.Parse("1234563218");
Console.WriteLine(nip.IssuingTaxOfficePrefix);
```

Do not treat this as the taxpayer's current office.

## ToString()

Framework support: `netstandard2.0`, `net10.0`

Use `ToString()` when you need canonical digits-only output.

## ToString(NipFormat)

Framework support: `netstandard2.0`, `net10.0`

Use `ToString(NipFormat)` when the consumer needs a specific representation.

```csharp
using PolishIdentifiers;

Nip nip = Nip.Parse("1234563218");

Console.WriteLine(nip.ToString(NipFormat.DigitsOnly));
Console.WriteLine(nip.ToString(NipFormat.Hyphenated));
Console.WriteLine(nip.ToString(NipFormat.VatEu));
```

## ToString(string?, IFormatProvider?)

Framework support: `netstandard2.0`, `net10.0`

Use this overload only when integrating with generic formatting APIs.

## IParsable<Nip> and ISpanParsable<Nip>

Framework support: `net10.0`

Use these interfaces in generic parsing infrastructure on `.NET 10`.

```csharp
using PolishIdentifiers;

Nip nip = IParsable<Nip>.Parse("1234563218", provider: null);
```

## Enterprise example

Framework support: `netstandard2.0`, `net10.0`

Normalize formatted input once at the boundary, then keep canonical `Nip` values in the domain.

```csharp
using PolishIdentifiers;

public sealed class Supplier
{
    public Supplier(Nip nip)
    {
        Nip = nip;
    }

    public Nip Nip { get; }
}
```

## Related docs

- [validation guidance](./NipValidate.md)
- [generation guidance](./NipGenerate.md)