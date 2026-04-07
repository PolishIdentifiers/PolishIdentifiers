# NipGenerator

`NipGenerator` creates valid NIP values and intentionally invalid NIP strings for tests and tooling.

Available on: `netstandard2.0`, `net10.0`

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

<a id="method-invalid-nonnumeric"></a>
### string: Invalid.NonNumeric()

Available on: `netstandard2.0`, `net10.0`

Returns an invalid NIP string that fails character validation only.

```csharp
using PolishIdentifiers;

var value = NipGenerator.Invalid.NonNumeric();
Console.WriteLine(value);
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

<a id="method-invalid-wronglength"></a>
### string: Invalid.WrongLength()

Available on: `netstandard2.0`, `net10.0`

Returns an invalid NIP string that fails length validation only.

```csharp
using PolishIdentifiers;

var value = NipGenerator.Invalid.WrongLength();
Console.WriteLine(value);
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