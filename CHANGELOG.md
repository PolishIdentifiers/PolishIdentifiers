# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [0.1.1] - 2026-03-07

### Changed

#### Validation order

- `InvalidCharacters` is now reported **before** `InvalidLength` when both rules are violated (e.g. `"12X"`, `" 44051401458"`).  
  New order: **characters → length → date → checksum**.  
  Previously: length → characters → date → checksum.
- `Pesel.Validate` XML documentation updated to reflect the new order.

#### `PeselValidationError` enum

- Members reordered in source to match check order (`InvalidCharacters` first, `InvalidLength` second).
- **Explicit integer values added** to all members — future source reorders will not silently change the wire format:
  - `InvalidCharacters = 0`
  - `InvalidLength = 1`
  - `InvalidDate = 2`
  - `InvalidChecksum = 3`

> **Breaking change (integer values):** in `v0.1.0` the implicit assignments were `InvalidLength = 0` and `InvalidCharacters = 1`. In `v0.1.1` these are swapped. Consumers who persisted `PeselValidationError` as a raw integer (JSON with `JsonNumberEnumConverter`, database column, logs) need to re-map old stored values.

---

## [0.1.0] - 2026-03-06

First public release. PESEL support only — NIP, REGON and NRB are planned for v0.4.

### Added

#### `Pesel` — strongly typed identifier

- `Pesel` readonly struct backed by `ulong` (no string allocation at rest)
- `Pesel.Parse(string)` / `Pesel.Parse(ReadOnlySpan<char>)` — throws `PeselValidationException` on invalid input
- `Pesel.TryParse(string?, out Pesel)` / `Pesel.TryParse(ReadOnlySpan<char>, out Pesel)` — exception-free parsing
- `Pesel.Validate(string?)` / `Pesel.Validate(ReadOnlySpan<char>)` — returns `ValidationResult<PeselValidationError>` without allocating a `Pesel`
- `Pesel.BirthDateTime` — date of birth decoded from the PESEL, valid for years 1800–2299
- `Pesel.BirthDateOnly` — date of birth as `DateOnly` (net10.0 only)
- `Pesel.Gender` — `Gender.Male` or `Gender.Female` decoded from digit 10
- `Pesel.IsDefault` — distinguishes a default struct instance from a parsed one
- `Pesel.ToString()` — always 11 ASCII digits, culture-invariant, leading zeros preserved
- `IFormattable` — supports `null`/`"G"`/`"g"`/`"D11"`/`"d11"` format strings; throws `FormatException` for unsupported formats
- `IEquatable<Pesel>`, `IComparable<Pesel>`, `==` / `!=` operators
- `IParsable<Pesel>`, `ISpanParsable<Pesel>` (net10.0 only)
- `default(Pesel)` is explicitly invalid — all domain properties throw `InvalidOperationException` when accessed on a default instance

#### Validation

- `PeselValidationError` enum: `InvalidLength`, `InvalidCharacters`, `InvalidDate`, `InvalidChecksum`
- Validation order: length → characters → date → checksum (deterministic, first-error-wins)
- Full century encoding: 1800–1899 (+80), 1900–1999 (+0), 2000–2099 (+20), 2100–2199 (+40), 2200–2299 (+60)
- Correct Gregorian leap year handling including centuries divisible by 100 but not 400
- `PeselValidationException` — wraps `PeselValidationError`, thrown by `Parse`
- `ValidationResult<TError>` — `readonly struct` with `IsValid`, `Error`, and `Match(onValid, onError)`

#### DataAnnotations

- `[ValidPesel]` attribute — validates `string`, `Pesel` struct, and `Pesel?`
- `null` is treated as valid; compose with `[Required]` when the field is mandatory
- Error message: `"The {0} field is not a valid PESEL."`
- Member name and display name propagated correctly to `ValidationResult`

#### Generator

- `PeselGenerator.Random()` — generates a random valid PESEL
- `PeselGenerator.ForBirthDate(DateTime)` — fluent builder; supports years 1800–2299
- `PeselGenerator.ForBirthDate(DateOnly)` — DateOnly overload (net10.0 only)
- `.Male()` / `.Female()` / `.WithGender(Gender)` — gender selection
- `PeselGenerator.Invalid.WrongChecksum()` — valid in all other respects, checksum digit is wrong
- `PeselGenerator.Invalid.WrongDate()` — correct checksum, encoded month outside all valid century ranges
- `PeselGenerator.Invalid.WrongLength()` — too short or too long
- `PeselGenerator.Invalid.NonNumeric()` — contains a non-digit character

#### Targets

- `netstandard2.0` — compatible with .NET Framework 4.6.1+ and all legacy runtimes
- `net10.0` — full modern API surface including `DateOnly`, `IParsable<T>`, `ISpanParsable<T>`

[0.1.1]: https://github.com/PolishIdentifiers/PolishIdentifiers/compare/v0.1.0...v0.1.1
[0.1.0]: https://github.com/PolishIdentifiers/PolishIdentifiers/releases/tag/v0.1.0
