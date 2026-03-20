# FAQ

## What identifiers are supported today?

The public package currently implements:

- `Pesel`
- `Nip`
- `Regon`

## Is this library only for validation?

No.

The package also supports:

- parsing into strong identifier types
- non-throwing parse flows with structured errors
- explicit NIP output formatting
- generators
- DataAnnotations validation attributes

## When should I use Parse, TryParse, or Validate?

- Use `Parse` when invalid input is exceptional.
- Use `TryParse` when invalid input is expected and should not throw.
- Use `Validate` when you only need to check validity and inspect the first public error.

## What input formats are supported?

- `Pesel`: canonical 11 digits only
- `Nip`: canonical digits plus the exact documented formatted representations
- `Regon`: canonical 9 or 14 digits only

See the dedicated identifier pages for the exact input contracts:

- [PESEL](https://github.com/PolishIdentifiers/PolishIdentifiers/blob/main/docs/pesel.md)
- [NIP](https://github.com/PolishIdentifiers/PolishIdentifiers/blob/main/docs/nip.md)
- [REGON](https://github.com/PolishIdentifiers/PolishIdentifiers/blob/main/docs/regon.md)

## Does checksum-valid mean assigned in real life?

No.

The library validates offline structure and checksum rules. It does not query registries or confirm that an identifier is assigned to a real person or business.

## Are generated values real identifiers?

No such guarantee is made.

Generators produce values that satisfy the public structural rules of the type. They should not be treated as proof that a value is assigned in the real world.

## What does default mean for these structs?

The default value of an identifier struct is not the same thing as a successfully parsed identifier.

- `Pesel.IsDefault`, `Nip.IsDefault`, and `Regon.IsDefault` tell you whether an instance came from `default` rather than from a valid parse or generator flow.
- Accessing domain properties on a default identifier throws `InvalidOperationException`.

`Regon` is slightly special internally because valid all-zero values must remain distinct from `default(Regon)`, but the public guidance is the same: treat `default` as uninitialized, not as a parsed business value.

## Does the package work on older .NET targets?

Yes. The package targets `netstandard2.0` and `net10.0`.

See [Framework support](https://github.com/PolishIdentifiers/PolishIdentifiers/blob/main/docs/framework-support.md) for the target-specific details.