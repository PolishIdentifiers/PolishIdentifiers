# PESEL

`Pesel` represents a validated Polish personal identifier.

Available on: `netstandard2.0`, `net10.0`

`ReadOnlySpan<char>` overloads are available on both targets in this package. Official .NET API docs list `ReadOnlySpan<T>` under `.NET Standard 2.1`, and this library exposes the span-based API on `netstandard2.0` by referencing `System.Memory` for that target.

## Contents

[Accepted input](#accepted-input) | [What it validates](#what-it-validates) | [Output formatting](#output-formatting) | [Persistence](#persistence) | [Generator docs](#generator-docs) | [Properties](#properties) | [Methods](#methods) | [Enums](#enums) | [Exceptions](#exceptions)

## Accepted input

`Pesel` accepts only the canonical 11-digit representation.

Accepted examples:

- `44051401458`

Rejected input categories:

- any non-digit character
- any length other than 11
- whitespace, separators, prefixes, or formatted variants

## What it validates

`Pesel` validates:

- numeric-only input
- exact length of 11 digits
- encoded birth date rules, including century handling from 1800 through 2299
- checksum validity

Important implementation notes:

- century handling covers all supported PESEL ranges from 1800 through 2299
- the month digits encode the century, not just the calendar month
- [`BirthDate`](#property-birthdate) returns a midnight `DateTime`; [`BirthDateOnly`](#property-birthdateonly) is available on `net10.0` only
- `default(Pesel)` is not a valid parsed value; use [`IsDefault`](#property-isdefault) before accessing domain properties on a value that might be uninitialized

## Output formatting

`Pesel` has one canonical output form.

- use [`ToString()`](#method-tostring) for storage, logging, and wire formats
- use [`ToString("D11", null)`](#method-tostring-format) only when an API explicitly expects an `IFormattable` format token
- there are no public display-format variants

## Persistence

Store the canonical 11-digit string produced by `ToString()`.

- write `pesel.ToString()` to the database or serialized payload
- read with [`Pesel.Parse(...)`](#method-parse-string) when stored data is guaranteed to be valid
- read with [`Pesel.TryParse(...)`](#method-tryparse-string) when stored data may be malformed
- no built-in JSON serializer converters are included; use [`TryParse`](#method-tryparse-string) at the deserialization boundary
- for EF Core, use a value converter targeting `string`

## Generator docs

See [PeselGenerator](./pesel-generator.md).

## Properties

<a id="property-birthdate"></a>
### DateTime: BirthDate

Available on: `netstandard2.0`, `net10.0`

Returns the birth date encoded in the PESEL number.

```csharp
using PolishIdentifiers;

var pesel = Pesel.Parse("44051401458");

Console.WriteLine(pesel.BirthDate.ToString("yyyy-MM-dd"));
```

<a id="property-birthdateonly"></a>
### DateOnly: BirthDateOnly

Available on: `net10.0`

Returns the birth date as `DateOnly`. This property is gated by `#if NET10_0_OR_GREATER` and is not available on `netstandard2.0`.

```csharp
using PolishIdentifiers;

var pesel = Pesel.Parse("44051401458");

Console.WriteLine(pesel.BirthDateOnly);
```

On other targets, use [`BirthDate`](#property-birthdate) directly. If your consuming application targets a framework that exposes `DateOnly`, you can derive the equivalent value from `BirthDate`:

```csharp
using PolishIdentifiers;

var pesel = Pesel.Parse("44051401458");

var dateOnly = DateOnly.FromDateTime(pesel.BirthDate);
Console.WriteLine(dateOnly);
```

<a id="property-gender"></a>
### [Gender](#enum-gender): Gender

Available on: `netstandard2.0`, `net10.0`

Returns the sex as encoded in the PESEL civil registry.

```csharp
using PolishIdentifiers;

var pesel = Pesel.Parse("44051401458");

Console.WriteLine(pesel.Gender);
```

<a id="property-isdefault"></a>
### bool: IsDefault

Available on: `netstandard2.0`, `net10.0`

Indicates whether the value comes from `default` instead of a parse or generator flow.

```csharp
using PolishIdentifiers;

if (!Pesel.TryParse("44051401458", out var pesel, out _))
{
    return;
}

if (pesel.IsDefault)
{
    Console.WriteLine("Uninitialized value.");
}
```

## Age and other derivations

The `Pesel` type exposes properties that are direct structural decodes of the identifier digits: `BirthDate`, `BirthDateOnly`, and `Gender`. These values are read directly from the encoded PESEL fields.

Deriving further values — such as age — is the consumer's responsibility. The example below uses `DateOnly` in the consuming application; if your target framework does not expose `DateOnly`, keep the calculation in `DateTime`.

```csharp
using PolishIdentifiers;

var pesel = Pesel.Parse("44051401458");

// Pass today's date as a parameter for testability.
static int CalculateAge(Pesel pesel, DateOnly referenceDate)
{
    var birth = DateOnly.FromDateTime(pesel.BirthDate);
    int age = referenceDate.Year - birth.Year;
    if (referenceDate < birth.AddYears(age))
        age--;
    return age;
}

int age = CalculateAge(pesel, DateOnly.FromDateTime(DateTime.Today));
```

Passing `referenceDate` explicitly instead of reading `DateTime.Today` inside the method makes the function deterministic and testable.

> **Why this is not a library method:** `BirthDate` and `Gender` are decoded directly from specific digit positions in the PESEL number. Age requires external calendar state (today's date) and is not a structural property of the identifier itself.

## Methods

<a id="method-compareto-pesel"></a>
### int: CompareTo(Pesel)

Available on: `netstandard2.0`, `net10.0`

Compares two PESEL values by numeric value.

```csharp
using PolishIdentifiers;

var left = Pesel.Parse("44051401458");
var right = Pesel.Parse("02211312372");

Console.WriteLine(left.CompareTo(right));
```

<a id="method-equals-object"></a>
### bool: Equals(object?)

Available on: `netstandard2.0`, `net10.0`

Tests value equality against an `object` reference.

```csharp
using PolishIdentifiers;

var left = Pesel.Parse("44051401458");
object right = Pesel.Parse("44051401458");

Console.WriteLine(left.Equals(right));
```

<a id="method-equals-pesel"></a>
### bool: Equals(Pesel)

Available on: `netstandard2.0`, `net10.0`

Tests value equality against another `Pesel`.

```csharp
using PolishIdentifiers;

var left = Pesel.Parse("44051401458");
var right = Pesel.Parse("44051401458");

Console.WriteLine(left.Equals(right));
```

<a id="method-gethashcode"></a>
### int: GetHashCode()

Available on: `netstandard2.0`, `net10.0`

Returns the hash code of the underlying value.

```csharp
using PolishIdentifiers;

var pesel = Pesel.Parse("44051401458");

Console.WriteLine(pesel.GetHashCode());
```

<a id="method-tryparse-with-format-provider"></a>
### bool: TryParse(string?, IFormatProvider?, out Pesel)

Available on: `netstandard2.0`, `net10.0`

Enables ASP.NET Core Minimal API route and query parameter binding on both targets.
The `IFormatProvider` argument is ignored; the method delegates to `TryParse(string?, out Pesel)`.

```csharp
using PolishIdentifiers;

var app = WebApplication.Create(args);

// Works on netstandard2.0 and net10.0 targets
app.MapGet("/persons/{pesel}", (Pesel pesel) => pesel.BirthDate.ToShortDateString());

app.Run();
```

<a id="method-iparsable-parse"></a>
### Pesel: IParsable<Pesel>.Parse(string, IFormatProvider?)

Available on: `net10.0`

Enables `Pesel` in generic parse APIs through `IParsable<Pesel>`.

```csharp
using System;
using PolishIdentifiers;

static T ParseValue<T>(string input) where T : IParsable<T>
    => T.Parse(input, null);

var pesel = ParseValue<Pesel>("44051401458");
Console.WriteLine(pesel);
```

<a id="method-iparsable-tryparse"></a>
### bool: IParsable<Pesel>.TryParse(string?, IFormatProvider?, out Pesel)

Available on: `net10.0`

Enables non-throwing generic string parsing through `IParsable<Pesel>`.

```csharp
using System;
using PolishIdentifiers;

static bool TryParseValue<T>(string input, out T value) where T : IParsable<T>
    => T.TryParse(input, null, out value);

var ok = TryParseValue<Pesel>("44051401458", out var pesel);

Console.WriteLine(ok);
```

<a id="method-ispanparsable-parse"></a>
### Pesel: ISpanParsable<Pesel>.Parse(ReadOnlySpan<char>, IFormatProvider?)

Available on: `net10.0`

Enables generic span parsing through `ISpanParsable<Pesel>`.

```csharp
using System;
using PolishIdentifiers;

static T ParseValue<T>(ReadOnlySpan<char> input) where T : ISpanParsable<T>
    => T.Parse(input, null);

ReadOnlySpan<char> input = "44051401458".AsSpan();
var pesel = ParseValue<Pesel>(input);

Console.WriteLine(pesel);
```

<a id="method-ispanparsable-tryparse"></a>
### bool: ISpanParsable<Pesel>.TryParse(ReadOnlySpan<char>, IFormatProvider?, out Pesel)

Available on: `net10.0`

Enables non-throwing generic span parsing through `ISpanParsable<Pesel>`.

```csharp
using System;
using PolishIdentifiers;

static bool TryParseValue<T>(ReadOnlySpan<char> input, out T value) where T : ISpanParsable<T>
    => T.TryParse(input, null, out value);

ReadOnlySpan<char> input = "44051401458".AsSpan();
var ok = TryParseValue<Pesel>(input, out var pesel);

Console.WriteLine(ok);
```

<a id="method-parse-span"></a>
### Pesel: Parse(ReadOnlySpan<char>)

Available on: `netstandard2.0`, `net10.0`

Parses a span without requiring a string-only call site.

```csharp
using PolishIdentifiers;

ReadOnlySpan<char> input = "44051401458".AsSpan();
var pesel = Pesel.Parse(input);
```

<a id="method-parse-string"></a>
### Pesel: Parse(string)

Available on: `netstandard2.0`, `net10.0`

Parses a string and throws [`PeselValidationException`](#exception-peselvalidationexception) when invalid.

```csharp
using PolishIdentifiers;

var input = "44051401458";
var pesel = Pesel.Parse(input);
```

<a id="method-tostring"></a>
### string: ToString()

Available on: `netstandard2.0`, `net10.0`

Returns the canonical 11-digit representation.

```csharp
using PolishIdentifiers;

var pesel = Pesel.Parse("44051401458");
var value = pesel.ToString();

Console.WriteLine(value);
```

<a id="method-tostring-format"></a>
### string: ToString(string?, IFormatProvider?)

Available on: `netstandard2.0`, `net10.0`

Accepts `null`, `""`, `"G"`, `"g"`, `"D11"`, and `"d11"` (comparison is case-insensitive); all return canonical output.

```csharp
using PolishIdentifiers;

var pesel = Pesel.Parse("44051401458");
var value = pesel.ToString("D11", null);

Console.WriteLine(value);
```

<a id="method-tryparse-span"></a>
### bool: TryParse(ReadOnlySpan<char>, out Pesel)

Available on: `netstandard2.0`, `net10.0`

Attempts to parse a character span without throwing and returns only success or failure.

```csharp
using PolishIdentifiers;

ReadOnlySpan<char> input = "44051401458".AsSpan();

if (Pesel.TryParse(input, out var pesel))
{
    Console.WriteLine(pesel);
}
```

<a id="method-tryparse-span-error"></a>
### bool: TryParse(ReadOnlySpan<char>, out Pesel, out [PeselValidationError](#enum-peselvalidationerror)?)

Available on: `netstandard2.0`, `net10.0`

Attempts to parse a character span without throwing and returns the first public validation error on failure.

```csharp
using PolishIdentifiers;

ReadOnlySpan<char> input = "44051401459".AsSpan();

if (!Pesel.TryParse(input, out var pesel, out var error))
{
    Console.WriteLine(error);
}
```

<a id="method-tryparse-string"></a>
### bool: TryParse(string?, out Pesel)

Available on: `netstandard2.0`, `net10.0`

Attempts to parse a string without throwing and returns only success or failure.

```csharp
using PolishIdentifiers;

var input = "44051401458";

if (Pesel.TryParse(input, out var pesel))
{
    Console.WriteLine(pesel);
}
```

<a id="method-tryparse-string-error"></a>
### bool: TryParse(string?, out Pesel, out [PeselValidationError](#enum-peselvalidationerror)?)

Available on: `netstandard2.0`, `net10.0`

Attempts to parse a string without throwing and returns the first public validation error on failure.

```csharp
using PolishIdentifiers;

var input = "44051401459";

if (!Pesel.TryParse(input, out var pesel, out var error))
{
    Console.WriteLine(error);
}
```

<a id="method-validate-span"></a>
### ValidationResult<[PeselValidationError](#enum-peselvalidationerror)>: Validate(ReadOnlySpan<char>)

Available on: `netstandard2.0`, `net10.0`

Validates a character span without allocating a `Pesel` instance.

```csharp
using PolishIdentifiers;

ReadOnlySpan<char> input = "44051401458".AsSpan();
var result = Pesel.Validate(input);

Console.WriteLine(result.IsValid);
```

<a id="method-validate-string"></a>
### ValidationResult<[PeselValidationError](#enum-peselvalidationerror)>: Validate(string?)

Available on: `netstandard2.0`, `net10.0`

Validates a string without allocating a `Pesel` instance.

```csharp
using PolishIdentifiers;

var result = Pesel.Validate("44051401458");

Console.WriteLine(result.IsValid);
```

<a id="method-operator-ne"></a>
### bool: operator !=

Available on: `netstandard2.0`, `net10.0`

Tests value inequality.

```csharp
using PolishIdentifiers;

var left = Pesel.Parse("44051401458");
var right = Pesel.Parse("02211312372");

Console.WriteLine(left != right);
```

<a id="method-operator-eq"></a>
### bool: operator ==

Available on: `netstandard2.0`, `net10.0`

Tests value equality.

```csharp
using PolishIdentifiers;

var left = Pesel.Parse("44051401458");
var right = Pesel.Parse("44051401458");

Console.WriteLine(left == right);
```

## Enums

<a id="enum-gender"></a>
### Gender

Available on: `netstandard2.0`, `net10.0`

Represents the sex as encoded in the PESEL civil registry.

- `Female`: encoded as an even digit in the 10th PESEL position
- `Male`: encoded as an odd digit in the 10th PESEL position

```csharp
using PolishIdentifiers;

var pesel = Pesel.Parse("44051401458");

switch (pesel.Gender)
{
    case Gender.Female:
        Console.WriteLine("Female");
        break;
    case Gender.Male:
        Console.WriteLine("Male");
        break;
}
```

<a id="enum-peselvalidationerror"></a>
### PeselValidationError

Available on: `netstandard2.0`, `net10.0`

Identifies the first public validation error returned by [`TryParse`](#method-tryparse-string) and [`Validate`](#method-validate-string).

- `InvalidCharacters`: the input contains one or more characters that are not decimal digits
- `InvalidLength`: the input does not consist of exactly 11 characters
- `InvalidDate`: the encoded date is not valid within the supported 1800-2299 range
- `InvalidChecksum`: the final digit does not match the checksum computed from the preceding digits

```csharp
using PolishIdentifiers;

if (!Pesel.TryParse("44051401459", out _, out PeselValidationError? error))
{
    Console.WriteLine(error);
}
```

## Exceptions

<a id="exception-peselvalidationexception"></a>
### PeselValidationException

Available on: `netstandard2.0`, `net10.0`

Thrown by [`Pesel.Parse(string)`](#method-parse-string) and [`Pesel.Parse(ReadOnlySpan<char>)`](#method-parse-span) when the input does not represent a valid PESEL number.

Prefer [`Pesel.TryParse(string?, out Pesel, out PeselValidationError?)`](#method-tryparse-string-error) or [`Pesel.Validate(string?)`](#method-validate-string) in performance-sensitive or high-volume scenarios to avoid the cost of exception handling.

- `Error` — the [`PeselValidationError`](#enum-peselvalidationerror) value that caused the failure

```csharp
using PolishIdentifiers;

try
{
    var pesel = Pesel.Parse("invalid");
}
catch (PeselValidationException ex)
{
    Console.WriteLine(ex.Error);
}
```