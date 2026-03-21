# FAQ

## What identifiers are supported today?

The public package currently implements:

- `Pesel`
- `Nip`
- `Regon`

## What identifiers are planned for future releases?

The repository includes internal contracts for identifiers that are documented but not yet implemented:

- `Nrb` — Polish bank account number (NRB / 26-digit IBAN)
- `PolishIdCardNumber` — Polish national identity card number
- `PolishPassportNumber` — Polish passport number
- `LandRegisterNumber` — land register (księga wieczysta) number

These are not yet available as public types. No timeline is guaranteed.

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

## Which identifiers should I use for a person contractor vs. a company contractor?

Polish business practice uses different identifier combinations depending on whether the contractor is a natural person or a legal entity.

**Natural person contractor:**

- `Pesel` — primary personal identifier; encodes birth date and gender
- `Nip` — tax identifier; a natural person running a business is issued a NIP
- Polish ID card or passport number (not yet implemented in this library)

**Legal entity contractor (company, partnership, association):**

- `Nip` — tax identifier; required on all invoices
- `Regon` — statistical registration number; typically REGON-9 for the main entity
- `Regon` (REGON-14) — if you are dealing with a specific registered local unit or branch

A common onboarding rule: if REGON is provided and is REGON-14, use `regon.BaseRegon9` to resolve the parent entity. If REGON is absent, treat the contractor as a natural person with NIP only.

## Does the library tell me if a NIP belongs to a person or a company?

No. NIP does not encode the type of entity it was issued to. The same 10-digit format is used for natural persons and legal entities alike. The library cannot determine entity type from a NIP value alone.

Use the presence of REGON or a combination of Pesel + NIP as your disambiguating signal in application logic. Document this rule in your domain model rather than relying on the identifier type to decide it for you.

## How do I deduplicate records that might have the same NIP in different formats?

Parse each NIP value with `Nip.TryParse` before inserting or comparing. Two `Nip` values are equal if they encode the same 10-digit number, regardless of the input format. Use a `Dictionary<Nip, T>` or `HashSet<Nip>` to group or deduplicate by value:

```csharp
using PolishIdentifiers;

var seen = new HashSet<Nip>();

foreach (var raw in importedNipValues)
{
    if (!Nip.TryParse(raw, out var nip, out _))
        continue; // log or accumulate the error

    if (!seen.Add(nip))
        Console.WriteLine($"Duplicate NIP: {nip}");
}
```

This works because `Nip` is a value type with correct equality semantics. `Nip.Parse("123-456-32-18")` and `Nip.Parse("1234563218")` produce equal values.

## How should I validate a large batch of identifiers efficiently?

Use the static `Validate` method instead of `TryParse` when you only need to check validity and do not need the parsed identifier instance. `Validate` allocates no object and does not throw:

```csharp
using PolishIdentifiers;

foreach (var raw in importedNipValues)
{
    var result = Nip.Validate(raw);
    if (!result.IsValid)
        Console.WriteLine($"Invalid NIP '{raw}': {result.Error}");
}
```

`Validate` is the zero-allocation path for high-volume validation loops.