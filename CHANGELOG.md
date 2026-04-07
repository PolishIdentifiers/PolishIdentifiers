# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).


## [2.0.0] - 2026-04-07

### Breaking changes

- **[Breaking]** `Nip.IssuingTaxOfficePrefix` now returns `string` instead of `int`. The value is always exactly 3 characters, zero-padded (e.g. `"012"` for a NIP beginning with `0`). Any code that stored the result in an `int`, performed numeric comparisons, or used arithmetic on this property must be updated. This is a binary-breaking change.
- **[Breaking]** Renamed the public `out` parameter names on `Pesel.TryParse(...)`, `Nip.TryParse(...)`, and `Regon.TryParse(...)` from identifier-specific names (`pesel`, `nip`, `regon`) to `result`. This does not affect existing compiled binaries, Minimal API binding, or positional-call source code, but it is a source-breaking change for callers that use named arguments on those parameters. The break is caught at compile time — the compiler produces a named-argument mismatch error that identifies the affected call sites directly.

### Added

- Added `TypeConverter` support for `Pesel`, `Nip`, and `Regon`, enabling string conversion in ComponentModel-based scenarios.
- Added `PeselTypeConverter`, `NipTypeConverter`, and `RegonTypeConverter`.
- Added `TryParse(string?, IFormatProvider?, out T)` overloads for `Pesel`, `Nip`, and `Regon`, enabling ASP.NET Core Minimal API route and query parameter binding on all supported targets.
- Added `NipGenerator.Invalid.UnrecognizedFormat()` for generating NIP strings that use valid NIP characters but intentionally fail with `NipValidationError.UnrecognizedFormat`.

### Changed

- `Pesel`, `Nip`, and `Regon` now support broader ASP.NET Core binding scenarios, including MVC/controller binding via `TypeConverter` and Minimal API parameter binding via the new `TryParse` overloads.

## [1.0.0] - 2026-03-22

`Pesel`, `Nip`, and `Regon` are included in the package.

### Breaking changes

- **[Breaking]** Removed `Nip.ParseFormatted(...)`, `Nip.TryParseFormatted(...)`, and `Nip.ValidateFormatted(...)` — use `Nip.Parse(...)`, `Nip.TryParse(...)`, and `Nip.Validate(...)` instead
- **[Breaking]** Renamed `Pesel.BirthDateTime` to `Pesel.BirthDate`
- **[Breaking]** Renamed `PeselGenerator.Random()` to `PeselGenerator.Generate()`
- **[Breaking]** Replaced `PeselGenerator.ForBirthDate(DateTime)` / `PeselGenerator.ForBirthDate(DateOnly)` fluent builder (and `.Male()` / `.Female()` / `.WithGender(Gender)`) with `PeselGenerator.Generate(DateTime)`, `PeselGenerator.Generate(DateOnly)`, `PeselGenerator.Generate(Gender, DateTime)`, and `PeselGenerator.Generate(Gender, DateOnly)` overloads
- **[Breaking]** Renamed `NipGenerator.Random()` to `NipGenerator.Generate()`

### Added

#### `Regon` - strongly typed identifier

- `Regon` readonly struct covering both REGON-9 and REGON-14
- Strict factories: `Regon.Parse(string)` / `Regon.Parse(ReadOnlySpan<char>)`, `Regon.TryParse(...)`, `Regon.Validate(...)`
- `RegonValidationError` enum: `InvalidCharacters`, `InvalidLength`, `InvalidChecksum`
- `RegonValidationException` - wraps `RegonValidationError`, thrown by `Parse`
- `RegonKind`, `Regon.IsRegon9`, `Regon.IsRegon14`, and `Regon.BaseRegon9`
- `Regon.IsDefault` with dedicated initialization-state handling so valid all-zero REGON values remain distinct from `default(Regon)`
- `Regon.ToString()` and `IFormattable` support for canonical `D9` and `D14` output
- `IFormattable`, `IEquatable<Regon>`, `IComparable<Regon>`, `==` / `!=` operators
- `IParsable<Regon>`, `ISpanParsable<Regon>` (net10.0 only)

#### Validation and generation

- REGON checksum validation for both 9-digit and 14-digit variants
- Two-step REGON-14 validation: base REGON-9 validation first, then REGON-14 checksum validation
- Support for valid canonical all-zero REGON values: `000000000` and `00000000000000`
- `RegonGenerator.Generate(RegonKind)` - generates a valid REGON of the specified kind (Regon9 or Regon14)
- `RegonGenerator.Invalid.WrongChecksumRegon9()` - valid in all other respects, REGON-9 checksum digit is wrong
- `RegonGenerator.Invalid.WrongChecksumRegon14()` - embedded REGON-9 base stays valid while the REGON-14 checksum digit is wrong
- `RegonGenerator.Invalid.WrongLength()` - digit-only value with invalid length
- `RegonGenerator.Invalid.NonNumeric()` - contains a non-digit character

#### DataAnnotations

- `[ValidRegon]` attribute - validates `string`, `Regon`, and `Regon?`
- For string values, `[ValidRegon]` accepts canonical 9-digit and 14-digit REGON input
- `null` is treated as valid; compose with `[Required]` when the field is mandatory
- Error message: `"The {0} field is not a valid REGON."`
- Member name and display name propagated correctly to `ValidationResult`

#### Documentation and examples

- Added comprehensive usage and reference documentation for all three identifiers: `docs/pesel.md`, `docs/nip.md`, `docs/regon.md`, `docs/pesel-generator.md`, `docs/nip-generator.md`, `docs/regon-generator.md`
- Added `docs/faq.md` covering common questions and edge cases
- Added `docs/framework-support.md` documenting `netstandard2.0` vs `net10.0` API differences
- Added standalone runnable example projects for `Pesel`, `Nip`, and `Regon` under `examples/`

### Changed

#### Unified parse ergonomics

- Added typed-error overloads for all implemented identifiers:
  - `TryParse(string?, out T result, out TError? error)`
  - `TryParse(ReadOnlySpan<char>, out T result, out TError? error)`
- `Nip.Parse(...)`, `Nip.TryParse(...)`, and `Nip.Validate(...)` now accept both canonical input and the exact documented supported formatted NIP representations
- `NipValidationError.UnrecognizedFormat` now applies on the unified NIP path for otherwise allowed-character layouts that do not match a documented accepted representation
- Updated README, FAQ, examples, and canonical contracts to reflect the new recommended parse flow and public API shape

- Migrated the PESEL, NIP, and REGON unit test suites to Shouldly assertions
- Extracted shared checksum weight definitions into `PeselChecksumWeights`, `NipChecksumWeights`, and `RegonChecksumWeights` so generators and validators use the same constants

## [0.2.0] - 2026-03-10

### Added

#### `Nip` — strongly typed identifier

- `Nip` readonly struct backed by `ulong` (no string allocation at rest)
- Strict factories: `Nip.Parse(string)` / `Nip.Parse(ReadOnlySpan<char>)`, `Nip.TryParse(...)`, `Nip.Validate(...)`
- Formatted factories: `Nip.ParseFormatted(...)`, `Nip.TryParseFormatted(...)`, `Nip.ValidateFormatted(...)`
- Recognized formatted input patterns: canonical digits, hyphenated, `PL1234563218`, `PL 1234563218`, `PL 123-456-32-18`
- `NipValidationError` enum: `InvalidCharacters`, `InvalidLength`, `UnrecognizedFormat`, `InvalidChecksum`
- `NipValidationException` — wraps `NipValidationError`, thrown by `Parse` and `ParseFormatted`
- `Nip.IsDefault` — distinguishes a default struct instance from a parsed one
- `Nip.IssuingTaxOfficePrefix` — first three digits, identifying the tax office that originally issued the NIP rather than the taxpayer's current competent office
- `Nip.ToString()` — always returns the 10-digit canonical form
- `Nip.ToString(NipFormat)` with `DigitsOnly`, `Hyphenated`, and `VatEu`
- `IFormattable`, `IEquatable<Nip>`, `IComparable<Nip>`, `==` / `!=` operators
- `IParsable<Nip>`, `ISpanParsable<Nip>` (net10.0 only)
- `default(Nip)` is explicitly invalid — domain properties and formatting throw `InvalidOperationException` when accessed on a default instance

#### Validation and generation

- NIP checksum validation using the official mod 11 weighting algorithm (`6, 5, 7, 2, 3, 4, 5, 6, 7`)
- `checksum == 10` is treated as `InvalidChecksum` (no fallback conversion to `0`)
- Validation order for strict NIP input: characters → length → checksum
- `NipGenerator.Random()` — generates a random valid NIP
- `NipGenerator.Invalid.WrongChecksum()` — valid in all other respects, checksum digit is wrong
- `NipGenerator.Invalid.WrongLength()` — digit-only value created from a valid NIP by removing or appending 1–3 trailing digits
- `NipGenerator.Invalid.NonNumeric()` — contains a non-digit character

#### DataAnnotations

- `[ValidNip]` attribute — validates `string`, `Nip`, and `Nip?`
- For string values, `[ValidNip]` accepts the same canonical and recognized formatted inputs as `Nip.ValidateFormatted(...)`
- `null` is treated as valid; compose with `[Required]` when the field is mandatory
- Error message: `"The {0} field is not a valid NIP."`
- Member name and display name propagated correctly to `ValidationResult`

### Changed

- Introduced explicit format recognition for formatted NIP input instead of heuristic normalization; non-recognized patterns now return `UnrecognizedFormat`
- `PeselGenerator.Invalid.WrongLength()` now generates digit-only values by removing or appending digits to a valid PESEL, introducing random length variance for better test coverage.


---

## [0.1.1] - 2026-03-07

### Breaking changes

- **[Breaking]** `PeselValidationError.InvalidCharacters` is now integer value `0` and `PeselValidationError.InvalidLength` is now `1`; both values swapped relative to 0.1.0. Code that compared or stored these enum values as integers must be updated.


### Changed

#### Validation order

- `InvalidCharacters` is now reported **before** `InvalidLength` when both rules are violated (e.g. `"12X"`, `" 44051401458"`).  
  New order: **characters → length → date → checksum**.  
  Previously: length → characters → date → checksum.
- `Pesel.Validate` XML documentation updated to reflect the new order.
---

## [0.1.0] - 2026-03-06

First public release. PESEL support only.

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

[2.0.0]: https://github.com/PolishIdentifiers/PolishIdentifiers/compare/v1.0.0...v2.0.0
[1.0.0]: https://github.com/PolishIdentifiers/PolishIdentifiers/compare/v0.2.0...v1.0.0
[0.2.0]: https://github.com/PolishIdentifiers/PolishIdentifiers/compare/v0.1.1...v0.2.0
[0.1.1]: https://github.com/PolishIdentifiers/PolishIdentifiers/compare/v0.1.0...v0.1.1
[0.1.0]: https://github.com/PolishIdentifiers/PolishIdentifiers/releases/tag/v0.1.0


