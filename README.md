# PolishIdentifiers

> .NET library for PESEL validation, parsing and generation.
> Strongly typed identifier — not just a validation function.

[![CI](https://github.com/PolishIdentifiers/PolishIdentifiers/actions/workflows/ci.yml/badge.svg)](https://github.com/PolishIdentifiers/PolishIdentifiers/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue)](LICENSE)
![Targets: netstandard2.0 · net10.0](https://img.shields.io/badge/targets-netstandard2.0%20%7C%20net10.0-informational)

---

Most validation libraries give you a `bool IsValid(string pesel)` and leave the rest to you. **PolishIdentifiers** gives you a proper domain type — a `readonly struct` that can only hold a valid value and carries structured error information.

## Why bother with a library?

- **Prevent invalid state** — `Pesel` can only be constructed via `Parse` / `TryParse`. There is no way to accidentally store a bad PESEL in your domain model.
- **Structured errors** — validation returns a typed enum (`PeselValidationError`), not a boolean or a magic string.
- **Zero allocation on the hot path** — validation runs on `ReadOnlySpan<char>`. No intermediate strings, no regex, no boxing.
- **Covers the full spec** — PESEL encodes birth dates from **1800 to 2299** across five century ranges. Most open-source implementations only cover 1900–2099. This one doesn't cut corners.
- **DataAnnotations out of the box** — `[ValidPesel]` plugs directly into ASP.NET model validation, MVC, minimal APIs and the `Validator` class. Works with both `string` and the `Pesel` struct. No extra packages — it's part of the core library.
- **Production-grade quality** — 330+ unit tests covering every error code, all 5 century ranges, leap year edge cases, culture invariance, and thread safety. The full spec, not just the happy path.

---

## Validate — structured error, no exceptions

```csharp
var result = Pesel.Validate("44051401458");
result.IsValid // true
result.Error   // null

var bad = Pesel.Validate("44051401457");
bad.Error // PeselValidationError.InvalidChecksum
```

Validation order: `InvalidCharacters` → `InvalidLength` → `InvalidDate` → `InvalidChecksum`.

## TryParse — exception-free parsing

```csharp
if (Pesel.TryParse(input, out var pesel))
{
    Console.WriteLine(pesel.BirthDateTime); // 1944-05-14
    Console.WriteLine(pesel.Gender);        // Gender.Male
}
```

## Parse — throw on invalid input

```csharp
try
{
    var pesel = Pesel.Parse("44051401457");
}
catch (PeselValidationException ex)
{
    Console.WriteLine(ex.Error); // PeselValidationError.InvalidChecksum
}
```

## Use it in your domain model

```csharp
// Domain entity — always strongly typed, never a string
public class Person
{
    public Pesel Pesel { get; }
}
```

## DataAnnotations — validate at the boundary

Use `[ValidPesel]` on DTOs and API models. The attribute follows standard DataAnnotations conventions: `null` is treated as valid (compose with `[Required]` when the field is mandatory), and it works with both `string` and the `Pesel` struct.

```csharp
// DTO / API request model — string is fine here, it's the boundary
public class RegisterRequest
{
    [Required]
    [ValidPesel]
    public string? Pesel { get; set; }
}

// Works with the strongly-typed struct too
public class PersonDto
{
    [ValidPesel]
    public Pesel Pesel { get; set; }
}
```

The `[ValidPesel]` attribute composes naturally with ASP.NET model binding:

```csharp
// Controller — ModelState.IsValid is false when the PESEL is invalid
[HttpPost]
public IActionResult Register(RegisterRequest request)
{
    if (!ModelState.IsValid)
        return ValidationProblem();

    var pesel = Pesel.Parse(request.Pesel!); // safe — already validated
    // ...
}
```

> **Rule of thumb:** use `[ValidPesel]` on DTOs and input models at the API boundary.
> Inside the domain, use the `Pesel` struct directly — it can only hold a valid value.

---

## Match on validation result

```csharp
var message = Pesel.Validate(input)
    .Match(
        onValid: ()  => "ok",
        onError: e   => $"invalid: {e}");
```

---

## Test data generation

`PeselGenerator` is built-in. Each `Invalid.*` method violates **exactly one rule**, making unit tests surgical:

```csharp
Pesel valid  = PeselGenerator.Random();
Pesel male   = PeselGenerator.ForBirthDate(new DateTime(1990, 5, 14)).Male();
Pesel female = PeselGenerator.ForBirthDate(new DateTime(1990, 5, 14)).Female();

// Each breaks exactly one validation rule
string badChecksum = PeselGenerator.Invalid.WrongChecksum();
string badDate     = PeselGenerator.Invalid.WrongDate();
string tooShort    = PeselGenerator.Invalid.WrongLength();
string nonNumeric  = PeselGenerator.Invalid.NonNumeric();
```

---

## Quality

The library takes correctness seriously. The PESEL implementation is backed by **330+ unit tests** covering:

- Every validation error code with multiple concrete inputs
- All 9 possible check digits — each tested individually
- All 5 century ranges (1800–2299), including boundary dates of each
- Leap year edge cases: valid (2000-02-29, 2004-02-29) and invalid (1900-02-29, 1800-02-29, 2100-02-29, 2200-02-29)
- February 29 in centuries divisible by 100 but not 400 (non-Gregorian leap years)
- 31st day in 30-day months (April, June, September)
- Whitespace, null, empty string, all-zeros, all-nines inputs
- Deterministic validation order with explicit tests for which error is reported when multiple rules fail
- `ToString` is culture-invariant — produces ASCII digits even under `ar-SA` or `fa-IR` locale
- Parse → ToString → Parse round-trip fidelity, including leading zeros
- Thread-safety under parallel load (500 concurrent operations)
- All `Invalid.*` generator outputs verified to fail on exactly one rule each, independently of the validator
- `[ValidPesel]` attribute: `string`, `Pesel` struct, nullable struct, `null`, unknown types, `[Required]` composability, error message shape, member name propagation

---

## License

MIT
