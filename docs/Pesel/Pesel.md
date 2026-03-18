# Pesel

This file describes the `Pesel` strong type API.

## Parse(string)

Framework support: `netstandard2.0`, `net10.0`

Use `Parse(string)` when the caller already expects valid input and an invalid value should fail immediately.

```csharp
using PolishIdentifiers;

Pesel pesel = Pesel.Parse("44051401458");
Console.WriteLine(pesel);
```

In service code, this fits best after validation has already happened earlier in the request pipeline.

## Parse(ReadOnlySpan<char>)

Framework support: `netstandard2.0`, `net10.0`

Use `Parse(ReadOnlySpan<char>)` when the input already exists as span-based data and you want to avoid converting it back to `string` first.

```csharp
using PolishIdentifiers;

ReadOnlySpan<char> input = "44051401458".AsSpan();
Pesel pesel = Pesel.Parse(input);
```

This is useful in low-allocation parsing code and text-processing pipelines.

## TryParse(string?, out Pesel)

Framework support: `netstandard2.0`, `net10.0`

Use `TryParse(string?, out Pesel)` when invalid input is part of normal control flow and exceptions would be noise.

```csharp
using PolishIdentifiers;

if (Pesel.TryParse("44051401458", out Pesel pesel))
{
    Console.WriteLine($"Birth date: {pesel.BirthDateTime:yyyy-MM-dd}");
}
```

This is the usual choice for imports, user input, and queue consumers.

## TryParse(ReadOnlySpan<char>, out Pesel)

Framework support: `netstandard2.0`, `net10.0`

Use the span overload for the same reason as the string overload when your input source is already span-based.

## BirthDateTime

Framework support: `netstandard2.0`, `net10.0`

Use `BirthDateTime` when the application wants the date encoded in the PESEL as a `DateTime`.

```csharp
using PolishIdentifiers;

Pesel pesel = Pesel.Parse("44051401458");
Console.WriteLine(pesel.BirthDateTime);
```

This is a good fit when the rest of the model still uses `DateTime` for historical or platform reasons.

## Gender

Framework support: `netstandard2.0`, `net10.0`

Use `Gender` when the workflow needs the gender encoded in the PESEL.

```csharp
using PolishIdentifiers;

Pesel pesel = Pesel.Parse("44051401458");
Console.WriteLine(pesel.Gender);
```

## BirthDateOnly

Framework support: `net10.0`

Use `BirthDateOnly` when the application model prefers `DateOnly` instead of a midnight `DateTime`.

```csharp
using PolishIdentifiers;

Pesel pesel = Pesel.Parse("44051401458");
DateOnly birthDate = pesel.BirthDateOnly;
```

This is the cleaner choice in `.NET 10` applications that already model dates without time.

## ToString()

Framework support: `netstandard2.0`, `net10.0`

Use `ToString()` when you need the canonical 11-digit representation.

```csharp
using PolishIdentifiers;

Pesel pesel = Pesel.Parse("44051401458");
Console.WriteLine(pesel.ToString());
```

## ToString(string?, IFormatProvider?)

Framework support: `netstandard2.0`, `net10.0`

Use this overload only when you are integrating with general formatting APIs. For most application code, `ToString()` is simpler.

## IParsable<Pesel> and ISpanParsable<Pesel>

Framework support: `net10.0`

Use the generic parsing interfaces when shared framework or infrastructure code expects `IParsable<T>` or `ISpanParsable<T>`.

```csharp
using PolishIdentifiers;

Pesel fromString = IParsable<Pesel>.Parse("44051401458", provider: null);
Pesel fromSpan = ISpanParsable<Pesel>.Parse("44051401458".AsSpan(), provider: null);
```

This is useful in generic parsing helpers and configuration pipelines.

## Enterprise example

Framework support: `netstandard2.0`, `net10.0`

Use `Pesel` in the domain model after boundary validation.

```csharp
using PolishIdentifiers;

public sealed class Person
{
    public Person(Pesel pesel)
    {
        Pesel = pesel;
    }

    public Pesel Pesel { get; }
}
```

This avoids carrying raw strings deeper into the application.

## Related docs

- [validation guidance](./PeselValidate.md)
- [generation guidance](./PeselGenerate.md)