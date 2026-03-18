# Validation and Behavior

This page documents the shared behavior that applies across the public identifier types: `Pesel`, `Nip`, and `Regon`.

## Parse, TryParse, and Validate

All implemented identifiers expose the same core validation model:

- `Parse(...)` returns the strong type or throws a domain-specific validation exception
- `TryParse(...)` returns `false` and sets the out parameter to `default` on invalid input
- `Validate(...)` returns `ValidationResult<TError>` instead of throwing

`Nip` also exposes a separate formatted-input path:

- `ParseFormatted(...)`
- `TryParseFormatted(...)`
- `ValidateFormatted(...)`

Use the formatted path only when the input contract explicitly allows one of the recognized display formats.

## Null handling

String-based methods do not all treat `null` the same way.

| API family | `null` behavior |
|---|---|
| `Parse(string)` | Throws `ArgumentNullException` |
| `TryParse(string?, out ...)` | Returns `false`, out=`default` |
| `Validate(string?)` | Returns a failure result |
| `Nip.ParseFormatted(string)` | Throws `ArgumentNullException` |
| `Nip.TryParseFormatted(string?, out ...)` | Returns `false`, out=`default` |
| `Nip.ValidateFormatted(string?)` | Returns `NipValidationError.UnrecognizedFormat` |

For strict validation:

- `Pesel.Validate(null)` returns `PeselValidationError.InvalidLength`
- `Nip.Validate(null)` returns `NipValidationError.InvalidLength`
- `Regon.Validate(null)` returns `RegonValidationError.InvalidLength`

## ValidationResult<TError>

`ValidationResult<TError>` is the non-throwing result type used by all public `Validate(...)` methods.

Surface:

- `IsValid`
- `Error`
- `Match(onValid, onError)`

Important behavior:

- Successful results have `IsValid == true` and `Error == null`
- Failed results have `IsValid == false` and a typed enum value in `Error`
- The default struct value is not a valid success result
- Calling `Match(...)` on a default-initialized `ValidationResult<TError>` throws `InvalidOperationException`

## Validation order

Validation is deterministic and first-error-wins.

`Pesel`

1. `InvalidCharacters`
2. `InvalidLength`
3. `InvalidDate`
4. `InvalidChecksum`

`Nip`

1. `InvalidCharacters`
2. `InvalidLength`
3. `InvalidChecksum`

`Regon`

1. `InvalidCharacters`
2. `InvalidLength`
3. `InvalidChecksum`

For formatted NIP input, recognition happens before strict NIP validation. If the input does not match one of the supported formatted shapes, the result is `NipValidationError.UnrecognizedFormat`.

## Default value semantics

All identifier types are `readonly struct` value objects. Their default values are intentionally invalid.

- `default(Pesel).IsDefault == true`
- `default(Nip).IsDefault == true`
- `default(Regon).IsDefault == true`

Implications:

- Domain properties throw `InvalidOperationException` on a default value
- Formatting methods throw `InvalidOperationException` on a default value
- DataAnnotations attributes treat default strong-type values as invalid

`Nip` and `Regon` distinguish default values from valid all-zero identifiers.

- `Nip.Parse("0000000000")` is valid and `IsDefault == false`
- `Regon.Parse("000000000")` is valid and `IsDefault == false`
- `Regon.Parse("00000000000000")` is valid and `IsDefault == false`

`Pesel` does not have an equivalent all-zero valid value because the encoded date would be invalid.

## DataAnnotations

The package includes three validation attributes:

- `[ValidPesel]`
- `[ValidNip]`
- `[ValidRegon]`

Shared behavior:

- `null` is treated as valid
- Combine with `[Required]` when presence is mandatory
- The attributes accept the corresponding strong type as well as string input
- Unsupported input types are invalid

Identifier-specific note:

- `[ValidNip]` accepts the same recognized formatted string inputs as `Nip.ValidateFormatted(...)`
- `[ValidPesel]` and `[ValidRegon]` validate strict canonical string input only

## Important edge cases

### `Pesel`

- Encoded birth dates are validated, not only length and checksum
- Supported encoded birth years are 1800-2299
- `BirthDateTime` always represents midnight
- `BirthDateOnly` is available only on `net10.0`

### `Nip`

- The checksum uses modulo 11
- If the weighted sum modulo 11 equals `10`, the value is invalid
- Formatted input is strict and case-sensitive
- Lowercase `pl`, extra spaces, unsupported separators, and leading or trailing whitespace are rejected

Recognized formatted NIP inputs:

- `1234563218`
- `123-456-32-18`
- `PL1234563218`
- `PL 1234563218`
- `PL 123-456-32-18`

### `Regon`

- Both `Regon9` and `Regon14` use modulo 11 checksums
- If the checksum remainder equals `10`, the expected check digit is `0`
- `Regon14` validation first checks the embedded `Regon9` base, then the 14-digit checksum
- `BaseRegon` returns the current value for `Regon9`, or the embedded base for `Regon14`
