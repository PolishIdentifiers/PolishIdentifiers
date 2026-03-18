# Pesel Generate

This file describes the public PESEL generation entry points.

## Generate()

Framework support: `netstandard2.0`, `net10.0`

Use `Generate()` when you need any valid PESEL for tests, fixtures, demos, or tooling.

```csharp
using PolishIdentifiers;

Pesel pesel = PeselGenerator.Generate();
Console.WriteLine(pesel);
```

## Generate(Gender)

Framework support: `netstandard2.0`, `net10.0`

Use this overload when the scenario depends on the encoded gender but the exact birth date does not matter.

## Generate(DateTime)

Framework support: `netstandard2.0`, `net10.0`

Use this overload when the test depends on the encoded birth date but not on the encoded gender.

## Generate(Gender, DateTime)

Framework support: `netstandard2.0`, `net10.0`

Use this overload when the fixture must carry both a known birth date and a known gender.

```csharp
using PolishIdentifiers;

Pesel pesel = PeselGenerator.Generate(Gender.Female, new DateTime(1990, 5, 14));
```

This is the most explicit overload for deterministic business fixtures.

## Generate(DateOnly)

Framework support: `net10.0`

Use this overload when your `.NET 10` code already models dates as `DateOnly`.

## Generate(Gender, DateOnly)

Framework support: `net10.0`

Use this overload when the fixture needs both a known gender and a `DateOnly` birth date.

## Invalid.WrongChecksum()

Framework support: `netstandard2.0`, `net10.0`

Use this helper when a test must fail with `PeselValidationError.InvalidChecksum`.

## Invalid.WrongDate()

Framework support: `netstandard2.0`, `net10.0`

Use this helper when a test must fail with `PeselValidationError.InvalidDate`.

## Invalid.WrongLength()

Framework support: `netstandard2.0`, `net10.0`

Use this helper when a test must fail with `PeselValidationError.InvalidLength`.

## Invalid.NonNumeric()

Framework support: `netstandard2.0`, `net10.0`

Use this helper when a test must fail with `PeselValidationError.InvalidCharacters`.

## Enterprise example

Framework support: `netstandard2.0`, `net10.0`

Use the generator for test fixtures that still need meaningful domain data.

```csharp
using PolishIdentifiers;

public sealed class PersonFixture
{
    public PersonFixture()
    {
        Pesel = PeselGenerator.Generate(Gender.Male, new DateTime(1988, 11, 23));
    }

    public Pesel Pesel { get; }
}
```

## Related docs

- [strong type guidance](./Pesel.md)
- [validation guidance](./PeselValidate.md)