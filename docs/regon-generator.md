# RegonGenerator

`RegonGenerator` creates valid REGON values and intentionally invalid REGON strings for tests and tooling.

Available on: `netstandard2.0`, `net10.0`

## Contents

### Methods

[Regon: Generate(RegonKind)](#method-generate) | [string: Invalid.NonNumeric()](#method-invalid-nonnumeric) | [string: Invalid.WrongChecksumRegon14()](#method-invalid-wrongchecksumregon14) | [string: Invalid.WrongChecksumRegon9()](#method-invalid-wrongchecksumregon9) | [string: Invalid.WrongLength()](#method-invalid-wronglength)

## Methods

<a id="method-generate"></a>
### Regon: Generate([RegonKind](./regon.md#enum-regonkind))

Available on: `netstandard2.0`, `net10.0`

Generates a valid REGON-9 or REGON-14.

```csharp
using PolishIdentifiers;

var regon9 = RegonGenerator.Generate(RegonKind.Regon9);
var regon14 = RegonGenerator.Generate(RegonKind.Regon14);

Console.WriteLine(regon9);
Console.WriteLine(regon14);
```

<a id="method-invalid-nonnumeric"></a>
### string: Invalid.NonNumeric()

Available on: `netstandard2.0`, `net10.0`

Returns an invalid REGON string that fails character validation only.

```csharp
using PolishIdentifiers;

var value = RegonGenerator.Invalid.NonNumeric();
Console.WriteLine(value);
```

<a id="method-invalid-wrongchecksumregon14"></a>
### string: Invalid.WrongChecksumRegon14()

Available on: `netstandard2.0`, `net10.0`

Returns an invalid REGON-14 string with a valid base and an invalid final checksum.

```csharp
using PolishIdentifiers;

var value = RegonGenerator.Invalid.WrongChecksumRegon14();
Console.WriteLine(value);
```

<a id="method-invalid-wrongchecksumregon9"></a>
### string: Invalid.WrongChecksumRegon9()

Available on: `netstandard2.0`, `net10.0`

Returns an invalid REGON-9 string that fails checksum validation only.

```csharp
using PolishIdentifiers;

var value = RegonGenerator.Invalid.WrongChecksumRegon9();
Console.WriteLine(value);
```

<a id="method-invalid-wronglength"></a>
### string: Invalid.WrongLength()

Available on: `netstandard2.0`, `net10.0`

Returns an invalid REGON string that fails length validation only.

```csharp
using PolishIdentifiers;

var value = RegonGenerator.Invalid.WrongLength();
Console.WriteLine(value);
```