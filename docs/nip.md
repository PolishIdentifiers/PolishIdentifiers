# NIP

`Nip` represents a validated Polish tax identifier.

Available on: `netstandard2.0`, `net10.0`

`ReadOnlySpan<char>` overloads are available on both targets in this package. Official .NET API docs list `ReadOnlySpan<T>` under `.NET Standard 2.1`, and this library exposes the span-based API on `netstandard2.0` by referencing `System.Memory` for that target.

## Contents

[Accepted input](#accepted-input) | [What it validates](#what-it-validates) | [Output formatting](#output-formatting) | [Persistence](#persistence) | [Generator docs](#generator-docs) | [Properties](#properties) | [Methods](#methods) | [Enums](#enums)

## Accepted input

Accepted public text representations are exactly:

1. `1234563218`
2. `123-456-32-18`
3. `PL1234563218`
4. `PL 1234563218`
5. `PL 123-456-32-18`

Formatted NIP support is explicit format recognition, not heuristic cleanup. The parser accepts only the documented public representations above and does not normalize arbitrary user input into one of them.

That means lowercase prefixes, unsupported separators, extra internal spaces, and leading or trailing whitespace are rejected instead of being silently cleaned up.

## What it validates

`Nip` validates:

- allowed characters for the documented public input contract
- length and layout rules for the exact supported representations
- checksum validity

Important implementation notes:

- lowercase prefixes, unsupported separators, extra spaces, and leading or trailing whitespace are rejected
- `IssuingTaxOfficePrefix` is historical metadata from the number itself; it is not a current routing or registry lookup signal
- when the weighted checksum modulo 11 equals 10, the input is invalid; the check digit is not remapped to `0`
- `default(Nip)` is not a valid parsed value; use `IsDefault` before accessing domain properties on a value that might be uninitialized

## Output formatting

`Nip` supports explicit output formatting through `NipFormat`.

- use `ToString()` or `ToString(NipFormat.DigitsOnly)` for canonical storage and wire formats
- use `ToString(NipFormat.Hyphenated)` when you need a conventional human-readable display form
- use `ToString(NipFormat.VatEu)` when you need the `PL`-prefixed VAT-EU form

## Persistence

Store the canonical 10-digit string produced by `ToString()`.

- write `nip.ToString()` to the database or serialized payload
- read with `Nip.Parse(...)` when stored data is guaranteed to be valid
- read with `Nip.TryParse(...)` when stored data may be malformed
- format only when producing output, not when persisting
- no built-in JSON serializer converters are included; use `TryParse` at the deserialization boundary
- for EF Core, use a value converter targeting `string`

## Generator docs

See [NipGenerator](./nip-generator.md).

## Properties

<a id="property-issuingtaxofficeprefix"></a>
### int: IssuingTaxOfficePrefix

Available on: `netstandard2.0`, `net10.0`

Returns the first three digits of the NIP.

```csharp
using PolishIdentifiers;

var nip = Nip.Parse("1234563218");

Console.WriteLine(nip.IssuingTaxOfficePrefix);
```

<a id="property-isdefault"></a>
### bool: IsDefault

Available on: `netstandard2.0`, `net10.0`

Indicates whether the value comes from `default` instead of a parse or generator flow.

```csharp
using PolishIdentifiers;

if (!Nip.TryParse("1234563218", out var nip, out _))
{
    return;
}

if (nip.IsDefault)
{
    Console.WriteLine("Uninitialized value.");
}
```

## Methods

<a id="method-compareto-nip"></a>
### int: CompareTo(Nip)

Available on: `netstandard2.0`, `net10.0`

Compares two NIP values by numeric value.

```csharp
using PolishIdentifiers;

var left = Nip.Parse("1234563218");
var right = Nip.Parse("8567346215");

Console.WriteLine(left.CompareTo(right));
```

<a id="method-equals-object"></a>
### bool: Equals(object?)

Available on: `netstandard2.0`, `net10.0`

Tests value equality against an `object` reference.

```csharp
using PolishIdentifiers;

var left = Nip.Parse("1234563218");
object right = Nip.Parse("1234563218");

Console.WriteLine(left.Equals(right));
```

<a id="method-equals-nip"></a>
### bool: Equals(Nip)

Available on: `netstandard2.0`, `net10.0`

Tests value equality against another `Nip`.

```csharp
using PolishIdentifiers;

var left = Nip.Parse("1234563218");
var right = Nip.Parse("1234563218");

Console.WriteLine(left.Equals(right));
```

<a id="method-gethashcode"></a>
### int: GetHashCode()

Available on: `netstandard2.0`, `net10.0`

Returns the hash code of the underlying value.

```csharp
using PolishIdentifiers;

var nip = Nip.Parse("1234563218");

Console.WriteLine(nip.GetHashCode());
```

<a id="method-iparsable-parse"></a>
### Nip: IParsable<Nip>.Parse(string, IFormatProvider?)

Available on: `net10.0`

Enables `Nip` in generic parse APIs through `IParsable<Nip>`.

```csharp
using System;
using PolishIdentifiers;

static T ParseValue<T>(string input) where T : IParsable<T>
    => T.Parse(input, null);

var nip = ParseValue<Nip>("1234563218");
Console.WriteLine(nip);
```

<a id="method-iparsable-tryparse"></a>
### bool: IParsable<Nip>.TryParse(string?, IFormatProvider?, out Nip)

Available on: `net10.0`

Enables non-throwing generic string parsing through `IParsable<Nip>`.

```csharp
using System;
using PolishIdentifiers;

static bool TryParseValue<T>(string input, out T value) where T : IParsable<T>
    => T.TryParse(input, null, out value);

var ok = TryParseValue<Nip>("1234563218", out var nip);

Console.WriteLine(ok);
```

<a id="method-ispanparsable-parse"></a>
### Nip: ISpanParsable<Nip>.Parse(ReadOnlySpan<char>, IFormatProvider?)

Available on: `net10.0`

Enables generic span parsing through `ISpanParsable<Nip>`.

```csharp
using System;
using PolishIdentifiers;

static T ParseValue<T>(ReadOnlySpan<char> input) where T : ISpanParsable<T>
    => T.Parse(input, null);

ReadOnlySpan<char> input = "1234563218".AsSpan();
var nip = ParseValue<Nip>(input);

Console.WriteLine(nip);
```

<a id="method-ispanparsable-tryparse"></a>
### bool: ISpanParsable<Nip>.TryParse(ReadOnlySpan<char>, IFormatProvider?, out Nip)

Available on: `net10.0`

Enables non-throwing generic span parsing through `ISpanParsable<Nip>`.

```csharp
using System;
using PolishIdentifiers;

static bool TryParseValue<T>(ReadOnlySpan<char> input, out T value) where T : ISpanParsable<T>
    => T.TryParse(input, null, out value);

ReadOnlySpan<char> input = "1234563218".AsSpan();
var ok = TryParseValue<Nip>(input, out var nip);

Console.WriteLine(ok);
```

<a id="method-parse-span"></a>
### Nip: Parse(ReadOnlySpan<char>)

Available on: `netstandard2.0`, `net10.0`

Parses a span and applies the same public NIP rules.

```csharp
using PolishIdentifiers;

ReadOnlySpan<char> input = "1234563218".AsSpan();
var nip = Nip.Parse(input);
```

<a id="method-parse-string"></a>
### Nip: Parse(string)

Available on: `netstandard2.0`, `net10.0`

Parses a string and throws `NipValidationException` when invalid.

```csharp
using PolishIdentifiers;

var input = "1234563218";
var nip = Nip.Parse(input);
```

<a id="method-tostring"></a>
### string: ToString()

Available on: `netstandard2.0`, `net10.0`

Returns the canonical 10-digit representation.

```csharp
using PolishIdentifiers;

var nip = Nip.Parse("1234563218");
var value = nip.ToString();

Console.WriteLine(value);
```

<a id="method-tostring-nipformat"></a>
### string: ToString([NipFormat](#enum-nipformat))

Available on: `netstandard2.0`, `net10.0`

Formats the output as digits-only, hyphenated, or VAT EU.

```csharp
using PolishIdentifiers;

var nip = Nip.Parse("1234563218");
var value = nip.ToString(NipFormat.Hyphenated);

Console.WriteLine(value);
```

<a id="method-tostring-format"></a>
### string: ToString(string?, IFormatProvider?)

Available on: `netstandard2.0`, `net10.0`

Accepts `null`, `""`, `"G"`, and `"D10"`; all return canonical output.

```csharp
using PolishIdentifiers;

var nip = Nip.Parse("1234563218");
var value = nip.ToString("D10", null);

Console.WriteLine(value);
```

<a id="method-tryparse-span"></a>
### bool: TryParse(ReadOnlySpan<char>, out Nip)

Available on: `netstandard2.0`, `net10.0`

Attempts to parse a character span without throwing and returns only success or failure.

```csharp
using PolishIdentifiers;

ReadOnlySpan<char> input = "1234563218".AsSpan();

if (Nip.TryParse(input, out var nip))
{
    Console.WriteLine(nip);
}
```

<a id="method-tryparse-span-error"></a>
### bool: TryParse(ReadOnlySpan<char>, out Nip, out [NipValidationError](#enum-nipvalidationerror)?)

Available on: `netstandard2.0`, `net10.0`

Attempts to parse a character span without throwing and returns the first public validation error on failure.

```csharp
using PolishIdentifiers;

ReadOnlySpan<char> input = "1234563219".AsSpan();

if (!Nip.TryParse(input, out var nip, out var error))
{
    Console.WriteLine(error);
}
```

<a id="method-tryparse-string"></a>
### bool: TryParse(string?, out Nip)

Available on: `netstandard2.0`, `net10.0`

Attempts to parse a string without throwing and returns only success or failure.

```csharp
using PolishIdentifiers;

var input = "1234563218";

if (Nip.TryParse(input, out var nip))
{
    Console.WriteLine(nip);
}
```

<a id="method-tryparse-string-error"></a>
### bool: TryParse(string?, out Nip, out [NipValidationError](#enum-nipvalidationerror)?)

Available on: `netstandard2.0`, `net10.0`

Attempts to parse a string without throwing and returns the first public validation error on failure.

```csharp
using PolishIdentifiers;

var input = "1234563219";

if (!Nip.TryParse(input, out var nip, out var error))
{
    Console.WriteLine(error);
}
```

<a id="method-validate-span"></a>
### ValidationResult<[NipValidationError](#enum-nipvalidationerror)>: Validate(ReadOnlySpan<char>)

Available on: `netstandard2.0`, `net10.0`

Validates a character span without allocating a `Nip` instance.

```csharp
using PolishIdentifiers;

ReadOnlySpan<char> input = "1234563218".AsSpan();
var result = Nip.Validate(input);

Console.WriteLine(result.IsValid);
```

<a id="method-validate-string"></a>
### ValidationResult<[NipValidationError](#enum-nipvalidationerror)>: Validate(string?)

Available on: `netstandard2.0`, `net10.0`

Validates a string without allocating a `Nip` instance.

```csharp
using PolishIdentifiers;

var result = Nip.Validate("1234563218");

Console.WriteLine(result.IsValid);
```

<a id="method-operator-ne"></a>
### bool: operator !=

Available on: `netstandard2.0`, `net10.0`

Tests value inequality.

```csharp
using PolishIdentifiers;

var left = Nip.Parse("1234563218");
var right = Nip.Parse("8567346215");

Console.WriteLine(left != right);
```

<a id="method-operator-eq"></a>
### bool: operator ==

Available on: `netstandard2.0`, `net10.0`

Tests value equality.

```csharp
using PolishIdentifiers;

var left = Nip.Parse("1234563218");
var right = Nip.Parse("1234563218");

Console.WriteLine(left == right);
```

## Enums

<a id="enum-nipformat"></a>
### NipFormat

Available on: `netstandard2.0`, `net10.0`

Specifies the output format for `Nip.ToString(NipFormat)`.

- `DigitsOnly`: ten consecutive digits such as `1234563218`
- `Hyphenated`: digits separated by hyphens such as `123-456-32-18`
- `VatEu`: `PL` prefix followed by ten digits such as `PL1234563218`

```csharp
using PolishIdentifiers;

var nip = Nip.Parse("1234563218");

Console.WriteLine(nip.ToString(NipFormat.DigitsOnly));
Console.WriteLine(nip.ToString(NipFormat.Hyphenated));
Console.WriteLine(nip.ToString(NipFormat.VatEu));
```

<a id="enum-nipvalidationerror"></a>
### NipValidationError

Available on: `netstandard2.0`, `net10.0`

Identifies the first public validation error returned by `TryParse` and `Validate`.

- `InvalidCharacters`: the input contains characters outside digits, uppercase `P`, uppercase `L`, space, and hyphen
- `InvalidLength`: the input is `null`, empty, or digit-only with a length other than 10
- `InvalidChecksum`: the final digit does not match the checksum, or the weighted sum modulo 11 equals 10
- `UnrecognizedFormat`: the input uses otherwise allowed characters but does not match one of the supported NIP text representations

```csharp
using PolishIdentifiers;

if (!Nip.TryParse("1234563219", out _, out NipValidationError? error))
{
    Console.WriteLine(error);
}
```