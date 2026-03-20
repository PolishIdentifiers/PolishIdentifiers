# REGON

`Regon` represents a validated Polish statistical identifier and supports both REGON-9 and REGON-14 through one public type.

Available on: `netstandard2.0`, `net10.0`

`ReadOnlySpan<char>` overloads are available on both targets in this package. Official .NET API docs list `ReadOnlySpan<T>` under `.NET Standard 2.1`, and this library exposes the span-based API on `netstandard2.0` by referencing `System.Memory` for that target.

## Contents

[Accepted input](#accepted-input) | [What it validates](#what-it-validates) | [Output formatting](#output-formatting) | [Persistence](#persistence) | [Generator docs](#generator-docs) | [Properties](#properties) | [Methods](#methods) | [Enums](#enums)

## Accepted input

`Regon` accepts only canonical digit input of length 9 or 14.

Accepted examples:

- `123456785`
- `12345678512347`

Rejected input categories:

- any non-digit character
- any length other than 9 or 14
- whitespace, separators, prefixes, or formatted variants

## What it validates

`Regon` validates:

- numeric-only input
- exact supported lengths of 9 or 14 digits
- checksum validity for REGON-9 and REGON-14

Important implementation notes:

- a 14-digit REGON is validated in two steps: first the embedded 9-digit base, then the 14-digit checksum
- for REGON, `sum % 11 == 10` maps to check digit `0`
- `BaseRegon9` returns the embedded 9-digit base for REGON-14 and returns the current value for REGON-9
- `default(Regon)` is distinct from valid all-zero REGON values; use `IsDefault` to detect an uninitialized instance

## Output formatting

`Regon` has canonical output only.

- use `ToString()` for storage, logging, and wire formats
- use `ToString("D9", null)` or `ToString("D14", null)` only when an API explicitly expects an `IFormattable` format token and you already know the variant
- there are no public display-format variants comparable to `NipFormat`

## Persistence

Store the canonical string produced by `ToString()`.

- write `regon.ToString()` to the database or serialized payload
- read with `Regon.Parse(...)` when stored data is guaranteed to be valid
- read with `Regon.TryParse(...)` when stored data may be malformed
- keep the full canonical value so the REGON-9 versus REGON-14 distinction is preserved
- no built-in JSON serializer converters are included; use `TryParse` at the deserialization boundary
- for EF Core, use a value converter targeting `string`

## Generator docs

See [RegonGenerator](./regon-generator.md).

## Properties

<a id="property-baseregon9"></a>
### Regon: BaseRegon9

Available on: `netstandard2.0`, `net10.0`

Returns the embedded 9-digit base.

```csharp
using PolishIdentifiers;

var regon = Regon.Parse("12345678512347");

Console.WriteLine(regon.BaseRegon9);
```

<a id="property-isdefault"></a>
### bool: IsDefault

Available on: `netstandard2.0`, `net10.0`

Indicates whether the value comes from `default` instead of a parse or generator flow.

```csharp
using PolishIdentifiers;

if (!Regon.TryParse("12345678512347", out var regon, out _))
{
    return;
}

if (regon.IsDefault)
{
    Console.WriteLine("Uninitialized value.");
}
```

<a id="property-isregon14"></a>
### bool: IsRegon14

Available on: `netstandard2.0`, `net10.0`

Indicates whether the value is the 14-digit REGON variant.

```csharp
using PolishIdentifiers;

var regon = Regon.Parse("12345678512347");

Console.WriteLine(regon.IsRegon14);
```

<a id="property-isregon9"></a>
### bool: IsRegon9

Available on: `netstandard2.0`, `net10.0`

Indicates whether the value is the 9-digit REGON variant.

```csharp
using PolishIdentifiers;

var regon = Regon.Parse("123456785");

Console.WriteLine(regon.IsRegon9);
```

<a id="property-kind"></a>
### [RegonKind](#enum-regonkind): Kind

Available on: `netstandard2.0`, `net10.0`

Returns whether the value is REGON-9 or REGON-14.

```csharp
using PolishIdentifiers;

var regon = Regon.Parse("12345678512347");

Console.WriteLine(regon.Kind);
```

## Methods

<a id="method-compareto-regon"></a>
### int: CompareTo(Regon)

Available on: `netstandard2.0`, `net10.0`

Compares two REGON values by initialization state, numeric value, and kind.

```csharp
using PolishIdentifiers;

var left = Regon.Parse("123456785");
var right = Regon.Parse("12345678512347");

Console.WriteLine(left.CompareTo(right));
```

<a id="method-equals-object"></a>
### bool: Equals(object?)

Available on: `netstandard2.0`, `net10.0`

Tests value equality against an `object` reference.

```csharp
using PolishIdentifiers;

var left = Regon.Parse("12345678512347");
object right = Regon.Parse("12345678512347");

Console.WriteLine(left.Equals(right));
```

<a id="method-equals-regon"></a>
### bool: Equals(Regon)

Available on: `netstandard2.0`, `net10.0`

Tests value equality, including kind and initialization state.

```csharp
using PolishIdentifiers;

var left = Regon.Parse("12345678512347");
var right = Regon.Parse("12345678512347");

Console.WriteLine(left.Equals(right));
```

<a id="method-gethashcode"></a>
### int: GetHashCode()

Available on: `netstandard2.0`, `net10.0`

Returns the hash code of the underlying value.

```csharp
using PolishIdentifiers;

var regon = Regon.Parse("12345678512347");

Console.WriteLine(regon.GetHashCode());
```

<a id="method-iparsable-parse"></a>
### Regon: IParsable<Regon>.Parse(string, IFormatProvider?)

Available on: `net10.0`

Enables `Regon` in generic parse APIs through `IParsable<Regon>`.

```csharp
using System;
using PolishIdentifiers;

static T ParseValue<T>(string input) where T : IParsable<T>
    => T.Parse(input, null);

var regon = ParseValue<Regon>("12345678512347");
Console.WriteLine(regon);
```

<a id="method-iparsable-tryparse"></a>
### bool: IParsable<Regon>.TryParse(string?, IFormatProvider?, out Regon)

Available on: `net10.0`

Enables non-throwing generic string parsing through `IParsable<Regon>`.

```csharp
using System;
using PolishIdentifiers;

static bool TryParseValue<T>(string input, out T value) where T : IParsable<T>
    => T.TryParse(input, null, out value);

var ok = TryParseValue<Regon>("12345678512347", out var regon);

Console.WriteLine(ok);
```

<a id="method-ispanparsable-parse"></a>
### Regon: ISpanParsable<Regon>.Parse(ReadOnlySpan<char>, IFormatProvider?)

Available on: `net10.0`

Enables generic span parsing through `ISpanParsable<Regon>`.

```csharp
using System;
using PolishIdentifiers;

static T ParseValue<T>(ReadOnlySpan<char> input) where T : ISpanParsable<T>
    => T.Parse(input, null);

ReadOnlySpan<char> input = "12345678512347".AsSpan();
var regon = ParseValue<Regon>(input);

Console.WriteLine(regon);
```

<a id="method-ispanparsable-tryparse"></a>
### bool: ISpanParsable<Regon>.TryParse(ReadOnlySpan<char>, IFormatProvider?, out Regon)

Available on: `net10.0`

Enables non-throwing generic span parsing through `ISpanParsable<Regon>`.

```csharp
using System;
using PolishIdentifiers;

static bool TryParseValue<T>(ReadOnlySpan<char> input, out T value) where T : ISpanParsable<T>
    => T.TryParse(input, null, out value);

ReadOnlySpan<char> input = "12345678512347".AsSpan();
var ok = TryParseValue<Regon>(input, out var regon);

Console.WriteLine(ok);
```

<a id="method-parse-span"></a>
### Regon: Parse(ReadOnlySpan<char>)

Available on: `netstandard2.0`, `net10.0`

Parses a span without changing the public validation rules.

```csharp
using PolishIdentifiers;

ReadOnlySpan<char> input = "12345678512347".AsSpan();
var regon = Regon.Parse(input);
```

<a id="method-parse-string"></a>
### Regon: Parse(string)

Available on: `netstandard2.0`, `net10.0`

Parses a string and throws `RegonValidationException` when invalid.

```csharp
using PolishIdentifiers;

var input = "12345678512347";
var regon = Regon.Parse(input);
```

<a id="method-tostring"></a>
### string: ToString()

Available on: `netstandard2.0`, `net10.0`

Returns the canonical 9-digit or 14-digit representation.

```csharp
using PolishIdentifiers;

var regon = Regon.Parse("12345678512347");
var value = regon.ToString();

Console.WriteLine(value);
```

<a id="method-tostring-format"></a>
### string: ToString(string?, IFormatProvider?)

Available on: `netstandard2.0`, `net10.0`

Accepts `null`, `""`, `"G"`, `"D9"`, and `"D14"` when they match the variant.

```csharp
using PolishIdentifiers;

var regon = Regon.Parse("12345678512347");
var value = regon.ToString("D14", null);

Console.WriteLine(value);
```

<a id="method-tryparse-span"></a>
### bool: TryParse(ReadOnlySpan<char>, out Regon)

Available on: `netstandard2.0`, `net10.0`

Attempts to parse a character span without throwing and returns only success or failure.

```csharp
using PolishIdentifiers;

ReadOnlySpan<char> input = "12345678512347".AsSpan();

if (Regon.TryParse(input, out var regon))
{
    Console.WriteLine(regon);
}
```

<a id="method-tryparse-span-error"></a>
### bool: TryParse(ReadOnlySpan<char>, out Regon, out [RegonValidationError](#enum-regonvalidationerror)?)

Available on: `netstandard2.0`, `net10.0`

Attempts to parse a character span without throwing and returns the first public validation error on failure.

```csharp
using PolishIdentifiers;

ReadOnlySpan<char> input = "12345678512348".AsSpan();

if (!Regon.TryParse(input, out var regon, out var error))
{
    Console.WriteLine(error);
}
```

<a id="method-tryparse-string"></a>
### bool: TryParse(string?, out Regon)

Available on: `netstandard2.0`, `net10.0`

Attempts to parse a string without throwing and returns only success or failure.

```csharp
using PolishIdentifiers;

var input = "12345678512347";

if (Regon.TryParse(input, out var regon))
{
    Console.WriteLine(regon);
}
```

<a id="method-tryparse-string-error"></a>
### bool: TryParse(string?, out Regon, out [RegonValidationError](#enum-regonvalidationerror)?)

Available on: `netstandard2.0`, `net10.0`

Attempts to parse a string without throwing and returns the first public validation error on failure.

```csharp
using PolishIdentifiers;

var input = "12345678512348";

if (!Regon.TryParse(input, out var regon, out var error))
{
    Console.WriteLine(error);
}
```

<a id="method-validate-span"></a>
### ValidationResult<[RegonValidationError](#enum-regonvalidationerror)>: Validate(ReadOnlySpan<char>)

Available on: `netstandard2.0`, `net10.0`

Validates a character span without allocating a `Regon` instance.

```csharp
using PolishIdentifiers;

ReadOnlySpan<char> input = "12345678512347".AsSpan();
var result = Regon.Validate(input);

Console.WriteLine(result.IsValid);
```

<a id="method-validate-string"></a>
### ValidationResult<[RegonValidationError](#enum-regonvalidationerror)>: Validate(string?)

Available on: `netstandard2.0`, `net10.0`

Validates a string without allocating a `Regon` instance.

```csharp
using PolishIdentifiers;

var result = Regon.Validate("12345678512347");

Console.WriteLine(result.IsValid);
```

<a id="method-operator-ne"></a>
### bool: operator !=

Available on: `netstandard2.0`, `net10.0`

Tests value inequality.

```csharp
using PolishIdentifiers;

var left = Regon.Parse("123456785");
var right = Regon.Parse("12345678512347");

Console.WriteLine(left != right);
```

<a id="method-operator-eq"></a>
### bool: operator ==

Available on: `netstandard2.0`, `net10.0`

Tests value equality.

```csharp
using PolishIdentifiers;

var left = Regon.Parse("12345678512347");
var right = Regon.Parse("12345678512347");

Console.WriteLine(left == right);
```

## Enums

<a id="enum-regonkind"></a>
### RegonKind

Available on: `netstandard2.0`, `net10.0`

Distinguishes between the two structural variants of a REGON number.

- `Regon9`: a 9-digit REGON number
- `Regon14`: a 14-digit REGON number; the first 9 digits form the embedded REGON-9 base

```csharp
using PolishIdentifiers;

var regon9 = RegonGenerator.Generate(RegonKind.Regon9);
var regon14 = RegonGenerator.Generate(RegonKind.Regon14);

Console.WriteLine(regon9.Kind);
Console.WriteLine(regon14.Kind);
```

<a id="enum-regonvalidationerror"></a>
### RegonValidationError

Available on: `netstandard2.0`, `net10.0`

Identifies the first public validation error returned by `TryParse` and `Validate`.

- `InvalidCharacters`: the input contains a non-digit character
- `InvalidLength`: the input length is neither 9 nor 14
- `InvalidChecksum`: the checksum is wrong, or for REGON-14 the embedded REGON-9 base is invalid

```csharp
using PolishIdentifiers;

if (!Regon.TryParse("12345678512348", out _, out RegonValidationError? error))
{
    Console.WriteLine(error);
}
```