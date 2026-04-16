# NipGenerator

`NipGenerator` creates valid NIP values and intentionally invalid NIP strings for tests and tooling.

Available on: `netstandard2.0`, `net10.0`

`NipGenerator` supports both single-value generation and batch generation with `count` overloads. Batch methods return `IReadOnlyList<Nip>` for valid values and `IReadOnlyList<string>` for invalid strings.

## Methods

<a id="method-generate"></a>
### Nip: Generate()

Available on: `netstandard2.0`, `net10.0`

Generates a valid NIP.

```csharp
using PolishIdentifiers;

var nip = NipGenerator.Generate();
Console.WriteLine(nip);
```

<a id="method-generate-count"></a>
### IReadOnlyList\<Nip\>: Generate(int count)

Available on: `netstandard2.0`, `net10.0`

Generates `count` valid NIPs. Returns an empty list when `count` is zero. Throws `ArgumentOutOfRangeException` when `count` is negative.

```csharp
using PolishIdentifiers;

var nips = NipGenerator.Generate(count: 10);
foreach (var nip in nips)
    Console.WriteLine(nip);
```

<a id="method-invalid-nonnumeric"></a>
### string: Invalid.NonNumeric()

Available on: `netstandard2.0`, `net10.0`

Returns an invalid NIP string that fails character validation only.

```csharp
using PolishIdentifiers;

var value = NipGenerator.Invalid.NonNumeric();
Console.WriteLine(value);
```

<a id="method-invalid-nonnumeric-count"></a>
### IReadOnlyList\<string\>: Invalid.NonNumeric(int count)

Available on: `netstandard2.0`, `net10.0`

Returns `count` invalid NIP strings, each failing character validation only. Returns an empty list when `count` is zero. Throws `ArgumentOutOfRangeException` when `count` is negative.

```csharp
using PolishIdentifiers;

var values = NipGenerator.Invalid.NonNumeric(count: 10);
```

<a id="method-invalid-wrongchecksum"></a>
### string: Invalid.WrongChecksum()

Available on: `netstandard2.0`, `net10.0`

Returns an invalid NIP string that fails checksum validation only.

```csharp
using PolishIdentifiers;

var value = NipGenerator.Invalid.WrongChecksum();
Console.WriteLine(value);
```

<a id="method-invalid-wrongchecksum-count"></a>
### IReadOnlyList\<string\>: Invalid.WrongChecksum(int count)

Available on: `netstandard2.0`, `net10.0`

Returns `count` invalid NIP strings, each failing checksum validation only. Returns an empty list when `count` is zero. Throws `ArgumentOutOfRangeException` when `count` is negative.

```csharp
using PolishIdentifiers;

var values = NipGenerator.Invalid.WrongChecksum(count: 10);
```

<a id="method-invalid-wronglength"></a>
### string: Invalid.WrongLength()

Available on: `netstandard2.0`, `net10.0`

Returns an invalid NIP string that fails length validation only.

```csharp
using PolishIdentifiers;

var value = NipGenerator.Invalid.WrongLength();
Console.WriteLine(value);
```

<a id="method-invalid-wronglength-count"></a>
### IReadOnlyList\<string\>: Invalid.WrongLength(int count)

Available on: `netstandard2.0`, `net10.0`

Returns `count` invalid NIP strings, each failing length validation only. Returns an empty list when `count` is zero. Throws `ArgumentOutOfRangeException` when `count` is negative.

```csharp
using PolishIdentifiers;

var values = NipGenerator.Invalid.WrongLength(count: 10);
```

<a id="method-invalid-unrecognizedformat"></a>
### string: Invalid.UnrecognizedFormat()

Available on: `netstandard2.0`, `net10.0`

Returns an invalid NIP string that uses only valid NIP characters but does not match any documented NIP text representation.

```csharp
using PolishIdentifiers;

var value = NipGenerator.Invalid.UnrecognizedFormat();
Console.WriteLine(value);
```

<a id="method-invalid-unrecognizedformat-count"></a>
### IReadOnlyList\<string\>: Invalid.UnrecognizedFormat(int count)

Available on: `netstandard2.0`, `net10.0`

Returns `count` invalid NIP strings, each failing format recognition only. Returns an empty list when `count` is zero. Throws `ArgumentOutOfRangeException` when `count` is negative.

```csharp
using PolishIdentifiers;

var values = NipGenerator.Invalid.UnrecognizedFormat(count: 10);
```