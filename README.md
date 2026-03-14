# PolishIdentifiers: PESEL, NIP, REGON

.NET library for validating, parsing, and generating Polish formal identifiers.

`PolishIdentifiers` provides three implemented strong types:

- `Pesel`
- `Nip`
- `Regon`

The library is built around one common API shape for every identifier:

- `Parse(...)` returns the strong type or throws a domain exception.
- `TryParse(...)` returns `false` for invalid input and never throws for domain errors.
- `Validate(...)` returns `ValidationResult<TError>` with a typed validation error enum.

The goal is simple: validate once at the boundary, then work with `Pesel`, `Nip`, or `Regon` instead of raw strings inside the domain model.

## Table of contents

- [Overview](#overview)
    - [What it does](#what-it-does)
    - [Package](#package)
- [API model](#api-model)
    - [Common API](#common-api)
    - [Implemented identifiers](#implemented-identifiers)
    - [Error model](#error-model)
    - [Identifier comparison](#identifier-comparison)
    - [Why use this over plain string validators](#why-use-this-over-plain-string-validators)
- [Usage examples](#usage-examples)
    - [PESEL](#pesel)
    - [NIP](#nip)
    - [REGON](#regon)
    - [DataAnnotations](#dataannotations)
    - [Boundary to domain example](#boundary-to-domain-example)
    - [Test data generation](#test-data-generation)
- [Technical notes](#technical-notes)
    - [Performance characteristics](#performance-characteristics)
    - [Technical characteristics](#technical-characteristics)
    - [Scope](#scope)
    - [License](#license)

## Overview

### What it does

- Strongly typed identifier value objects: `Pesel`, `Nip`, `Regon`
- Consistent `Parse` / `TryParse` / `Validate` API across identifiers
- Structured validation errors such as `InvalidChecksum` or `InvalidLength`
- `readonly struct` identifiers that do not represent an invalid value when created through the public factories
- Allocation-free validation path based on `ReadOnlySpan<char>`
- Built-in generators for valid and intentionally invalid test data
- Built-in DataAnnotations attributes for DTO and input-layer validation

### Package

| Item | Value |
|---|---|
| Package | `PolishIdentifiers` |
| Install | `dotnet add package PolishIdentifiers` |
| Target frameworks | `netstandard2.0`, `net10.0` |
| Implemented identifiers | `Pesel`, `Nip`, `Regon` |

Compatibility note: `Parse`, `TryParse`, and `Validate` are available on both target frameworks. On `net10.0`, the implemented identifiers also expose modern generic parsing interfaces in addition to the shared API.

## API model

### Common API

The same usage pattern applies to each implemented identifier.

```csharp
using PolishIdentifiers;

ValidationResult<PeselValidationError> validation = Pesel.Validate("44051401458");

if (!validation.IsValid)
{
    Console.WriteLine(validation.Error);
    return;
}

Pesel pesel = Pesel.Parse("44051401458");

if (Pesel.TryParse("44051401458", out Pesel parsed))
{
    Console.WriteLine(parsed);
}
```

Use `Validate` when you need a precise error category.
Use `TryParse` when you only need success or failure.
Use `Parse` when invalid input should fail immediately.

### Implemented identifiers

| Type | Identifier | Notes |
|---|---|---|
| `Pesel` | PESEL | 11 digits, exposes `BirthDateTime` and `Gender` |
| `Nip` | NIP | 10 digits, supports strict and formatted input paths |
| `Regon` | REGON | 9-digit and 14-digit variants, exposes `Kind` and `BaseRegon` |

### Error model

Validation is deterministic. The first failing rule is returned as a typed enum value.

Typical validation order is:

1. invalid characters
2. invalid length
3. identifier-specific structural rule
4. invalid checksum

Reference by type:

| Type | Error enum values |
|---|---|
| `Pesel` | `InvalidCharacters`, `InvalidLength`, `InvalidDate`, `InvalidChecksum` |
| `Nip` | `InvalidCharacters`, `InvalidLength`, `InvalidChecksum`, `UnrecognizedFormat` on formatted path |
| `Regon` | `InvalidCharacters`, `InvalidLength`, `InvalidChecksum` |

### Identifier comparison

| Type | Accepted input forms | Notable domain properties | Generator support |
|---|---|---|---|
| `Pesel` | Canonical 11-digit input | `BirthDateTime`, `Gender`, `IsDefault` | `Random()`, `ForBirthDate(...)`, `Invalid.WrongChecksum()`, `Invalid.WrongDate()`, `Invalid.WrongLength()`, `Invalid.NonNumeric()` |
| `Nip` | Canonical 10-digit input, plus explicit formatted path with 5 supported formats | `IssuingTaxOfficePrefix`, `IsDefault` | `Random()`, `Invalid.WrongChecksum()`, `Invalid.WrongLength()`, `Invalid.NonNumeric()` |
| `Regon` | Canonical 9-digit and 14-digit input | `Kind`, `IsMain`, `IsLocal`, `BaseRegon`, `IsDefault` | `Random()`, `RandomLocal()`, `Invalid.WrongChecksum()`, `Invalid.WrongChecksum14()`, `Invalid.WrongLength()`, `Invalid.NonNumeric()` |

### Why use this over plain string validators

Using a validator method over raw strings answers only one question: is this text valid right now.

Using `Pesel`, `Nip`, and `Regon` changes the shape of the application model:

- A strong type prevents passing arbitrary strings deeper into the domain after validation.
- `Validate(...)` returns a typed error enum, so callers can distinguish `InvalidChecksum` from `InvalidLength` without parsing exception messages.
- Invalid default struct instances are explicit domain-invalid states, which prevents accidental use of uninitialized values as if they were valid identifiers.
- `Nip` supports a separate formatted-input path instead of heuristic string cleanup, so accepted formats are explicit and testable.
- `Regon` models both REGON-9 and REGON-14 in one type and exposes the distinction through `Kind` and `BaseRegon`, instead of forcing callers to infer semantics from string length everywhere.

## Usage examples

### PESEL

```csharp
using PolishIdentifiers;

Pesel pesel = Pesel.Parse("44051401458");

Console.WriteLine(pesel);                // 44051401458
Console.WriteLine(pesel.BirthDateTime);  // 1944-05-14 00:00:00
Console.WriteLine(pesel.Gender);         // Male

ValidationResult<PeselValidationError> result = Pesel.Validate("440514X1458");
Console.WriteLine(result.Error);         // InvalidCharacters
```

### NIP

```csharp
using PolishIdentifiers;

Nip nip = Nip.Parse("1234563218");

Console.WriteLine(nip);                                 // 1234563218
Console.WriteLine(nip.IssuingTaxOfficePrefix);          // 123
Console.WriteLine(nip.ToString(NipFormat.Hyphenated));  // 123-456-32-18
Console.WriteLine(nip.ToString(NipFormat.VatEu));       // PL1234563218

Nip formatted = Nip.ParseFormatted("PL 123-456-32-18");
Console.WriteLine(formatted);                           // 1234563218

ValidationResult<NipValidationError> result = Nip.Validate("1234563219");
Console.WriteLine(result.Error);                        // InvalidChecksum
```

Supported formatted NIP inputs:

| Format | Example |
|---|---|
| Canonical | `1234563218` |
| Hyphenated | `123-456-32-18` |
| VAT EU without space | `PL1234563218` |
| VAT EU with space | `PL 1234563218` |
| VAT EU with hyphens | `PL 123-456-32-18` |

Inputs outside these formats return `NipValidationError.UnrecognizedFormat` from the formatted API path.

### REGON

```csharp
using PolishIdentifiers;

Regon regon9 = Regon.Parse("123456785");

Console.WriteLine(regon9);           // 123456785
Console.WriteLine(regon9.Kind);      // Main
Console.WriteLine(regon9.IsMain);    // True
Console.WriteLine(regon9.BaseRegon); // 123456785

Regon regon14 = Regon.Parse("12345678512347");

Console.WriteLine(regon14);           // 12345678512347
Console.WriteLine(regon14.Kind);      // Local
Console.WriteLine(regon14.IsLocal);   // True
Console.WriteLine(regon14.BaseRegon); // 123456785

ValidationResult<RegonValidationError> result = Regon.Validate("123456780");
Console.WriteLine(result.Error);      // InvalidChecksum
```

### DataAnnotations

The library also includes validation attributes for input models.

```csharp
using PolishIdentifiers.Nip.Attributes;
using PolishIdentifiers.Pesel.Attributes;
using PolishIdentifiers.Regon.Attributes;

public sealed class CompanyInput
{
    [ValidPesel]
    public string OwnerPesel { get; set; } = string.Empty;

    [ValidNip]
    public string Nip { get; set; } = string.Empty;

    [ValidRegon]
    public string Regon { get; set; } = string.Empty;
}
```

### Boundary to domain example

The intended usage is: accept raw strings at the boundary, validate and convert once, then keep strong types in the core model.

```csharp
using PolishIdentifiers;

public sealed class RegisterCompanyRequest
{
    public string OwnerPesel { get; init; } = string.Empty;
    public string Nip { get; init; } = string.Empty;
    public string Regon { get; init; } = string.Empty;
}

public sealed class Company
{
    public Company(Pesel ownerPesel, Nip nip, Regon regon)
    {
        OwnerPesel = ownerPesel;
        Nip = nip;
        Regon = regon;
    }

    public Pesel OwnerPesel { get; }
    public Nip Nip { get; }
    public Regon Regon { get; }
}

public static class CompanyMapper
{
    public static Company Create(RegisterCompanyRequest request)
    {
        ValidationResult<PeselValidationError> peselValidation = Pesel.Validate(request.OwnerPesel);
        ValidationResult<NipValidationError> nipValidation = Nip.ValidateFormatted(request.Nip);
        ValidationResult<RegonValidationError> regonValidation = Regon.Validate(request.Regon);

        if (!peselValidation.IsValid)
            throw new InvalidOperationException($"Invalid PESEL: {peselValidation.Error}");

        if (!nipValidation.IsValid)
            throw new InvalidOperationException($"Invalid NIP: {nipValidation.Error}");

        if (!regonValidation.IsValid)
            throw new InvalidOperationException($"Invalid REGON: {regonValidation.Error}");

        return new Company(
            Pesel.Parse(request.OwnerPesel),
            Nip.ParseFormatted(request.Nip),
            Regon.Parse(request.Regon));
    }
}
```

After conversion, the domain model no longer stores identifier strings.

### Test data generation

Each identifier has a generator for valid values and isolated invalid cases.

```csharp
using PolishIdentifiers;

Pesel validPesel = PeselGenerator.Random();
Nip validNip = NipGenerator.Random();
Regon validRegon = RegonGenerator.RandomLocal();

string invalidPesel = PeselGenerator.Invalid.WrongChecksum();
string invalidNip = NipGenerator.Invalid.WrongLength();
string invalidRegon = RegonGenerator.Invalid.WrongChecksum14();
```

## Technical notes

### Performance characteristics

The library makes a small set of concrete implementation choices that are relevant for hot-path validation:

- Validation operates on `ReadOnlySpan<char>`.
- Validation does not depend on regular expressions.
- Numeric identifiers are stored internally as numeric values rather than strings.
- `Validate(...)` returns a lightweight typed result instead of allocating a domain object.
- Implemented target frameworks are `netstandard2.0` and `net10.0`.

### Technical characteristics

- The shared `Parse`, `TryParse`, and `Validate` API is available on both target frameworks.
- `default(Pesel)`, `default(Nip)`, and `default(Regon)` are not valid domain instances.
- On `net10.0`, the implemented identifiers also expose modern generic parsing interfaces in addition to the shared API.

### Scope

This package focuses on deterministic, offline validation of Polish formal identifiers with checksum or structure-based verification.

It is not a registry lookup client and it does not attempt to confirm whether a syntactically valid number is assigned to a real person or business.

### License

MIT
