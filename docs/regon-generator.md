# RegonGenerator

`RegonGenerator` creates valid REGON values and intentionally invalid REGON strings for tests and tooling.

Available on: `netstandard2.0`, `net10.0`

`RegonGenerator` supports both single-value generation and batch generation with `count` overloads. Batch methods return `IReadOnlyList<Regon>` for valid values and `IReadOnlyList<string>` for invalid strings.

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

<a id="method-generate-regonkind-count"></a>
### IReadOnlyList\<Regon\>: Generate([RegonKind](./regon.md#enum-regonkind), int count)

Available on: `netstandard2.0`, `net10.0`

Generates `count` valid REGONs of the specified kind. Returns an empty list when `count` is zero. Throws `ArgumentOutOfRangeException` when `kind` is unsupported or `count` is negative.

```csharp
using PolishIdentifiers;

var regon9s  = RegonGenerator.Generate(RegonKind.Regon9,  count: 10);
var regon14s = RegonGenerator.Generate(RegonKind.Regon14, count: 10);
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

<a id="method-invalid-nonnumeric-count"></a>
### IReadOnlyList\<string\>: Invalid.NonNumeric(int count)

Available on: `netstandard2.0`, `net10.0`

Returns `count` invalid REGON-9 strings, each containing a non-digit character. Returns an empty list when `count` is zero. Throws `ArgumentOutOfRangeException` when `count` is negative.

```csharp
using PolishIdentifiers;

var values = RegonGenerator.Invalid.NonNumeric(count: 10);
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

<a id="method-invalid-wrongchecksumregon14-count"></a>
### IReadOnlyList\<string\>: Invalid.WrongChecksumRegon14(int count)

Available on: `netstandard2.0`, `net10.0`

Returns `count` invalid REGON-14 strings, each with a valid embedded REGON-9 base but a wrong final check digit. Returns an empty list when `count` is zero. Throws `ArgumentOutOfRangeException` when `count` is negative.

```csharp
using PolishIdentifiers;

var values = RegonGenerator.Invalid.WrongChecksumRegon14(count: 10);
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

<a id="method-invalid-wrongchecksumregon9-count"></a>
### IReadOnlyList\<string\>: Invalid.WrongChecksumRegon9(int count)

Available on: `netstandard2.0`, `net10.0`

Returns `count` invalid REGON-9 strings, each failing checksum validation only. Returns an empty list when `count` is zero. Throws `ArgumentOutOfRangeException` when `count` is negative.

```csharp
using PolishIdentifiers;

var values = RegonGenerator.Invalid.WrongChecksumRegon9(count: 10);
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
<a id="method-invalid-wronglength-count"></a>
### IReadOnlyList\<string\>: Invalid.WrongLength(int count)

Available on: `netstandard2.0`, `net10.0`

Returns `count` invalid REGON strings, each failing length validation only. Returns an empty list when `count` is zero. Throws `ArgumentOutOfRangeException` when `count` is negative.

```csharp
using PolishIdentifiers;

var values = RegonGenerator.Invalid.WrongLength(count: 10);
```
