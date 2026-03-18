# Documentation

This folder contains the repository documentation for the currently implemented identifiers:

- `Pesel`
- `Nip`
- `Regon`

The docs are organized by developer task:

- Start with the root [`README.md`](../README.md) for fast orientation and a compact API overview
- Read [`Common/ValidationAndBehavior.md`](./Common/ValidationAndBehavior.md) for shared parsing, validation, null-handling, default-value, and DataAnnotations behavior
- Use the identifier pages for API details and examples that are specific to one identifier

## How the docs are organized

Each implemented identifier has three reference guides:

- the strong type itself
- validation guidance
- generation guidance

This keeps quick orientation, task guidance, and type-specific reference separate instead of mixing them into one long page.

## Shared references

- [validation and behavior](./Common/ValidationAndBehavior.md)

## Identifier guides

### Pesel

- [strong type guide](./Pesel/Pesel.md)
- [validation guide](./Pesel/PeselValidate.md)
- [generation guide](./Pesel/PeselGenerate.md)

### Nip

- [strong type guide](./Nip/Nip.md)
- [validation guide](./Nip/NipValidate.md)
- [generation guide](./Nip/NipGenerate.md)

### Regon

- [strong type guide](./Regon/Regon.md)
- [validation guide](./Regon/RegonValidate.md)
- [generation guide](./Regon/RegonGenerate.md)

## Framework notes

The package targets `netstandard2.0` and `net10.0`.

- Most public APIs are available on both target frameworks
- `.NET 10` adds `IParsable<T>` and `ISpanParsable<T>` for all implemented identifiers
- `.NET 10` adds `Pesel.BirthDateOnly`
- `.NET 10` adds `PeselGenerator.Generate(DateOnly)` and `PeselGenerator.Generate(Gender, DateOnly)`

## Scope notes

- The package does not expose public `PeselValidator`, `NipValidator`, or `RegonValidator` types
- Validation is performed through the public `Validate(...)` methods and DataAnnotations attributes
- The repository may contain unreleased documentation and API changes beyond the latest published package; check [`CHANGELOG.md`](../CHANGELOG.md) when release status matters
