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

---

## Validate — structured error, no exceptions

```csharp
var result = Pesel.Validate("44051401458");
result.IsValid // true
result.Error   // null

var bad = Pesel.Validate("44051401457");
bad.Error // PeselValidationError.InvalidChecksum
```

Validation order: `InvalidLength` → `InvalidCharacters` → `InvalidDate` → `InvalidChecksum`.

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

## License

MIT
