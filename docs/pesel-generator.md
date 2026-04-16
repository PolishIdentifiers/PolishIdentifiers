# PeselGenerator

`PeselGenerator` creates valid PESEL values and intentionally invalid PESEL strings for tests and tooling.

Available on: `netstandard2.0`, `net10.0`

`PeselGenerator` supports both single-value generation and batch generation with `count` overloads. Batch methods return `IReadOnlyList<Pesel>` for valid values and `IReadOnlyList<string>` for invalid strings.

## Methods

<a id="method-generate"></a>
### Pesel: Generate()

Available on: `netstandard2.0`, `net10.0`

Generates a valid PESEL with a random birth date and gender.

```csharp
using PolishIdentifiers;

var pesel = PeselGenerator.Generate();
Console.WriteLine(pesel);
```

<a id="method-generate-count"></a>
### IReadOnlyList\<Pesel\>: Generate(int count)

Available on: `netstandard2.0`, `net10.0`

Generates `count` valid PESELs with random birth dates and genders. Returns an empty list when `count` is zero. Throws `ArgumentOutOfRangeException` when `count` is negative.

```csharp
using PolishIdentifiers;

var pesels = PeselGenerator.Generate(count: 10);
foreach (var pesel in pesels)
    Console.WriteLine(pesel);
```

<a id="method-generate-dateonly"></a>
### Pesel: Generate(DateOnly)

Available on: `net10.0`

Generates a valid PESEL for a given `DateOnly` with a random gender.

```csharp
using PolishIdentifiers;

var pesel = PeselGenerator.Generate(new DateOnly(1980, 1, 1));
Console.WriteLine(pesel);
```

<a id="method-generate-dateonly-count"></a>
### IReadOnlyList\<Pesel\>: Generate(DateOnly, int count)

Available on: `net10.0`

Generates `count` valid PESELs for the given `DateOnly` with random genders. Throws `ArgumentOutOfRangeException` when the year is outside 1800–2299 or `count` is negative.

```csharp
using PolishIdentifiers;

var pesels = PeselGenerator.Generate(new DateOnly(1980, 1, 1), count: 10);
```

<a id="method-generate-datetime"></a>
### Pesel: Generate(DateTime)

Available on: `netstandard2.0`, `net10.0`

Generates a valid PESEL for a given `DateTime` with a random gender.

```csharp
using PolishIdentifiers;

var pesel = PeselGenerator.Generate(new DateTime(1980, 1, 1));
Console.WriteLine(pesel);
```

<a id="method-generate-datetime-count"></a>
### IReadOnlyList\<Pesel\>: Generate(DateTime, int count)

Available on: `netstandard2.0`, `net10.0`

Generates `count` valid PESELs for persons born on the given date, with random genders. Throws `ArgumentOutOfRangeException` when the year is outside 1800–2299 or `count` is negative.

```csharp
using PolishIdentifiers;

var pesels = PeselGenerator.Generate(new DateTime(1980, 1, 1), count: 10);
```

<a id="method-generate-gender"></a>
### Pesel: Generate([Gender](./pesel.md#enum-gender))

Available on: `netstandard2.0`, `net10.0`

Generates a valid PESEL with a random birth date and the specified gender.

```csharp
using PolishIdentifiers;

var pesel = PeselGenerator.Generate(Gender.Female);
Console.WriteLine(pesel);
```

<a id="method-generate-gender-count"></a>
### IReadOnlyList\<Pesel\>: Generate([Gender](./pesel.md#enum-gender), int count)

Available on: `netstandard2.0`, `net10.0`

Generates `count` valid PESELs with random birth dates and the specified gender. Throws `ArgumentOutOfRangeException` when `gender` is unsupported or `count` is negative.

```csharp
using PolishIdentifiers;

var pesels = PeselGenerator.Generate(Gender.Male, count: 10);
```

<a id="method-generate-gender-dateonly"></a>
### Pesel: Generate([Gender](./pesel.md#enum-gender), DateOnly)

Available on: `net10.0`

Generates a valid PESEL for a given `DateOnly` and gender.

```csharp
using PolishIdentifiers;

var pesel = PeselGenerator.Generate(Gender.Male, new DateOnly(1980, 1, 1));
Console.WriteLine(pesel);
```

<a id="method-generate-gender-dateonly-count"></a>
### IReadOnlyList\<Pesel\>: Generate([Gender](./pesel.md#enum-gender), DateOnly, int count)

Available on: `net10.0`

Generates `count` valid PESELs for the given `DateOnly` and gender. Throws `ArgumentOutOfRangeException` when the year is outside 1800–2299, `gender` is unsupported, or `count` is negative.

```csharp
using PolishIdentifiers;

var pesels = PeselGenerator.Generate(Gender.Male, new DateOnly(1980, 1, 1), count: 10);
```

<a id="method-generate-gender-datetime"></a>
### Pesel: Generate([Gender](./pesel.md#enum-gender), DateTime)

Available on: `netstandard2.0`, `net10.0`

Generates a valid PESEL for a given `DateTime` and gender.

```csharp
using PolishIdentifiers;

var pesel = PeselGenerator.Generate(Gender.Male, new DateTime(1980, 1, 1));
Console.WriteLine(pesel);
```

<a id="method-generate-gender-datetime-count"></a>
### IReadOnlyList\<Pesel\>: Generate([Gender](./pesel.md#enum-gender), DateTime, int count)

Available on: `netstandard2.0`, `net10.0`

Generates `count` valid PESELs for the given birth date and gender. Throws `ArgumentOutOfRangeException` when the year is outside 1800–2299, `gender` is unsupported, or `count` is negative.

```csharp
using PolishIdentifiers;

var pesels = PeselGenerator.Generate(Gender.Male, new DateTime(1980, 1, 1), count: 10);
```

<a id="method-invalid-nonnumeric"></a>
### string: Invalid.NonNumeric()

Available on: `netstandard2.0`, `net10.0`

Returns an invalid PESEL string that fails character validation only.

```csharp
using PolishIdentifiers;

var value = PeselGenerator.Invalid.NonNumeric();
Console.WriteLine(value);
```

<a id="method-invalid-nonnumeric-count"></a>
### IReadOnlyList\<string\>: Invalid.NonNumeric(int count)

Available on: `netstandard2.0`, `net10.0`

Returns `count` invalid PESEL strings, each failing character validation only. Returns an empty list when `count` is zero. Throws `ArgumentOutOfRangeException` when `count` is negative.

```csharp
using PolishIdentifiers;

var values = PeselGenerator.Invalid.NonNumeric(count: 10);
```

<a id="method-invalid-wrongchecksum"></a>
### string: Invalid.WrongChecksum()

Available on: `netstandard2.0`, `net10.0`

Returns an invalid PESEL string that fails checksum validation only.

```csharp
using PolishIdentifiers;

var value = PeselGenerator.Invalid.WrongChecksum();
Console.WriteLine(value);
```

<a id="method-invalid-wrongchecksum-count"></a>
### IReadOnlyList\<string\>: Invalid.WrongChecksum(int count)

Available on: `netstandard2.0`, `net10.0`

Returns `count` invalid PESEL strings, each failing checksum validation only. Returns an empty list when `count` is zero. Throws `ArgumentOutOfRangeException` when `count` is negative.

```csharp
using PolishIdentifiers;

var values = PeselGenerator.Invalid.WrongChecksum(count: 10);
```

<a id="method-invalid-wrongdate"></a>
### string: Invalid.WrongDate()

Available on: `netstandard2.0`, `net10.0`

Returns an invalid PESEL string that fails date validation only.

```csharp
using PolishIdentifiers;

var value = PeselGenerator.Invalid.WrongDate();
Console.WriteLine(value);
```

<a id="method-invalid-wrongdate-count"></a>
### IReadOnlyList\<string\>: Invalid.WrongDate(int count)

Available on: `netstandard2.0`, `net10.0`

Returns `count` invalid PESEL strings, each failing date validation only. Returns an empty list when `count` is zero. Throws `ArgumentOutOfRangeException` when `count` is negative.

```csharp
using PolishIdentifiers;

var values = PeselGenerator.Invalid.WrongDate(count: 10);
```

<a id="method-invalid-wronglength"></a>
### string: Invalid.WrongLength()

Available on: `netstandard2.0`, `net10.0`

Returns an invalid PESEL string that fails length validation only.

```csharp
using PolishIdentifiers;

var value = PeselGenerator.Invalid.WrongLength();
Console.WriteLine(value);
```

<a id="method-invalid-wronglength-count"></a>
### IReadOnlyList\<string\>: Invalid.WrongLength(int count)

Available on: `netstandard2.0`, `net10.0`

Returns `count` invalid PESEL strings, each failing length validation only. Returns an empty list when `count` is zero. Throws `ArgumentOutOfRangeException` when `count` is negative.

```csharp
using PolishIdentifiers;

var values = PeselGenerator.Invalid.WrongLength(count: 10);
```