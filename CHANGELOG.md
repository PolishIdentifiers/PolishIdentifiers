# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [0.2.0] - 2026-03-10

### Added

#### `Nip` — strongly typed identifier

- `Nip` readonly struct backed by `ulong` (no string allocation at rest)
- Strict factories: `Nip.Parse(string)` / `Nip.Parse(ReadOnlySpan<char>)`, `Nip.TryParse(...)`, `Nip.Validate(...)`
- Formatted factories: `Nip.ParseFormatted(...)`, `Nip.TryParseFormatted(...)`, `Nip.ValidateFormatted(...)`
- Recognized formatted input patterns: canonical digits, hyphenated, `PL1234563218`, `PL 1234563218`, `PL 123-456-32-18`
- `NipValidationError` enum: `InvalidCharacters`, `InvalidLength`, `InvalidChecksum`, `UnrecognizedFormat`
- `NipValidationException` — wraps `NipValidationError`, thrown by `Parse` and `ParseFormatted`
- `Nip.IsDefault` — distinguishes a default struct instance from a parsed one
- `Nip.IssuingTaxOfficePrefix` — first three digits of the identifier
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

### Changed

#### Validation order

- `InvalidCharacters` is now reported **before** `InvalidLength` when both rules are violated (e.g. `"12X"`, `" 44051401458"`).  
  New order: **characters → length → date → checksum**.  
  Previously: length → characters → date → checksum.
- `Pesel.Validate` XML documentation updated to reflect the new order.
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

[0.2.0]: https://github.com/PolishIdentifiers/PolishIdentifiers/compare/v0.1.1...v0.2.0
[0.1.1]: https://github.com/PolishIdentifiers/PolishIdentifiers/compare/v0.1.0...v0.1.1
[0.1.0]: https://github.com/PolishIdentifiers/PolishIdentifiers/releases/tag/v0.1.0
