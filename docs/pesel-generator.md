# PeselGenerator

`PeselGenerator` creates valid PESEL values and intentionally invalid PESEL strings for tests and tooling.

Available on: `netstandard2.0`, `net10.0`

## Contents

### Methods

[Pesel: Generate()](#method-generate) | [Pesel: Generate(DateOnly)](#method-generate-dateonly) | [Pesel: Generate(DateTime)](#method-generate-datetime) | [Pesel: Generate(Gender)](#method-generate-gender) | [Pesel: Generate(Gender, DateOnly)](#method-generate-gender-dateonly) | [Pesel: Generate(Gender, DateTime)](#method-generate-gender-datetime) | [string: Invalid.NonNumeric()](#method-invalid-nonnumeric) | [string: Invalid.WrongChecksum()](#method-invalid-wrongchecksum) | [string: Invalid.WrongDate()](#method-invalid-wrongdate) | [string: Invalid.WrongLength()](#method-invalid-wronglength)

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

<a id="method-generate-dateonly"></a>
### Pesel: Generate(DateOnly)

Available on: `net10.0`

Generates a valid PESEL for a given `DateOnly` with a random gender.

```csharp
using PolishIdentifiers;

var pesel = PeselGenerator.Generate(new DateOnly(1980, 1, 1));
Console.WriteLine(pesel);
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

<a id="method-generate-gender"></a>
### Pesel: Generate([Gender](./pesel.md#enum-gender))

Available on: `netstandard2.0`, `net10.0`

Generates a valid PESEL with a random birth date and the specified gender.

```csharp
using PolishIdentifiers;

var pesel = PeselGenerator.Generate(Gender.Female);
Console.WriteLine(pesel);
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

<a id="method-generate-gender-datetime"></a>
### Pesel: Generate([Gender](./pesel.md#enum-gender), DateTime)

Available on: `netstandard2.0`, `net10.0`

Generates a valid PESEL for a given `DateTime` and gender.

```csharp
using PolishIdentifiers;

var pesel = PeselGenerator.Generate(Gender.Male, new DateTime(1980, 1, 1));
Console.WriteLine(pesel);
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

<a id="method-invalid-wrongchecksum"></a>
### string: Invalid.WrongChecksum()

Available on: `netstandard2.0`, `net10.0`

Returns an invalid PESEL string that fails checksum validation only.

```csharp
using PolishIdentifiers;

var value = PeselGenerator.Invalid.WrongChecksum();
Console.WriteLine(value);
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

<a id="method-invalid-wronglength"></a>
### string: Invalid.WrongLength()

Available on: `netstandard2.0`, `net10.0`

Returns an invalid PESEL string that fails length validation only.

```csharp
using PolishIdentifiers;

var value = PeselGenerator.Invalid.WrongLength();
Console.WriteLine(value);
```