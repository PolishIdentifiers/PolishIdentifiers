# PolishIdentifiers

> Stop stringly-typing your domain. Strongly typed, zero-allocation .NET primitives for PESEL and NIP.

[![CI](https://github.com/PolishIdentifiers/PolishIdentifiers/actions/workflows/ci.yml/badge.svg)](https://github.com/PolishIdentifiers/PolishIdentifiers/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue)](LICENSE)
![Targets: netstandard2.0 · net10.0](https://img.shields.io/badge/targets-netstandard2.0%20%7C%20net10.0-informational)

---

Primitive obsession is a code smell. Using a `string` to represent a Polish identifier in your domain model is a bug waiting to happen. Did someone validate the length? Is the checksum correct? Has it been sanitized?

**PolishIdentifiers** fixes this. Instead of a basic `bool IsValid(...)` function, it gives you production-grade, tightly constrained `readonly struct` domain primitives that **cannot hold an invalid value**. 

### Why choose this library?

- **No Invalid State:** `Pesel` and `Nip` are constructed through `Parse` or `TryParse`. Once you have an instance, it's guaranteed mathematically correct.
- **Zero Allocation on the Hot Path:** Validations run entirely on `ReadOnlySpan<char>`. No regex, no intermediate string allocations, and no boxing. Memory footprint at rest? An 8-byte `ulong`. 
- **Actionable Validation:** Get structured errors (e.g., `InvalidChecksum`, `InvalidLength`) via the lightweight `ValidationResult` API. You never have to guess *why* a validation failed.
- **Built-in `[DataAnnotations]`:** Validate directly at your DTO / API boundary with `[ValidPesel]` and `[ValidNip]`. No extra NuGet packages needed.
- **Battle-Tested:** Hundreds of deterministic unit tests spanning edge cases like 19th-century leap years, malformed input handling, and rigorous thread safety.
- **Deterministic Test Generators:** Need a faulty PESEL with just the wrong checksum for a unit test? `PeselGenerator.Invalid.WrongChecksum()` creates it instantly. Same for NIP.

---

## ⚡ Quick Start

Install via NuGet:

```bash
dotnet add package PolishIdentifiers
```

### 1. Extract Domain Information Safely

```csharp
// Unambiguous and Exception-Free
if (Pesel.TryParse("44051401458", out var pesel))
{
    Console.WriteLine(pesel.BirthDateTime); // Full 1800-2299 range supported
    Console.WriteLine(pesel.Gender);        // Gender.Male / Gender.Female
}

// NIP fully supports both STRICT and FORMATTED real-world input
var nip = Nip.ParseFormatted("PL 123-456-32-18");
Console.WriteLine(nip.IssuingTaxOfficePrefix);     // 123
Console.WriteLine(nip.ToString(NipFormat.VatEu));  // "PL1234563218"
```

### 2. Actionable, Explicit Validations 

No allocations, no exceptions. Just an explicit typed enum pointing to the exact problem.

```csharp
var badNip = Nip.Validate("1234563219");
Console.WriteLine(badNip.Error); // NipValidationError.InvalidChecksum

var badPesel = Pesel.Validate("440514X1458");
Console.WriteLine(badPesel.Error); // PeselValidationError.InvalidCharacters
```

### 3. API Boundary & DTO Validation
Stop bad data at the front door using ASP.NET Core bindings and model validation. 

```csharp
public class RegisterCompanyRequest
{
    [Required]
    [ValidNip]   // Accepts strings, `Nip?`, handles multiple real-world formats
    public string Nip { get; set; }

    [Required]
    [ValidPesel] // Accepts strings, `Pesel?`
    public string OwnerPesel { get; set; }
}
```

### 4. Flawless Unit Testing Setup

The library features built-in generators that ensure you aren't hunting for correct or incorrect test payloads manually. They mutate **one exact rule at a time**, preventing test-contamination.

```csharp
Pesel validPesel = PeselGenerator.Random();
Nip validNip     = NipGenerator.Random();

// Pin-pointed edge-case violations. Returns strings for testing endpoints!
string badDate     = PeselGenerator.Invalid.WrongDate();     
string badLength   = NipGenerator.Invalid.WrongLength();     
string badChecksum = NipGenerator.Invalid.WrongChecksum();   
```

---

## The Philosophy

- **Boundary vs Domain:** Use `[ValidPesel]` / `[ValidNip]` on data structures receiving strings (e.g. from JSON). Map internally to strong types like `Pesel` and `Nip` so your core application never worries about validation again.
- **Offline Only:** The scope of this library bounds itself strictly to mathematical/algorithmic check validation. No unreliable external HTTP queries, database roundtrips, or runtime states are required.
- **Zero-Dependency Architecture:** Minimal core containing solely `System.Memory` wrappers for legacy targets (`netstandard2.0`). Fully taps into high-performance `ISpanParsable<T>` additions out-of-the-box for `net10.0`.

---

## License

[MIT](LICENSE)
