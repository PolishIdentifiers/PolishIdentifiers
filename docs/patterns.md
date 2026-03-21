# Patterns

This page collects recommended patterns for common real-world scenarios. All examples use the current public API and compile against both `netstandard2.0` and `net10.0` unless noted.

## Contents

[Persistence](#persistence) | [Deduplication](#deduplication) | [Import loop with typed errors](#import-loop-with-typed-errors) | [Person vs. company contractor decision guide](#person-vs-company-contractor-decision-guide)

---

## Persistence

Store the canonical string produced by `ToString()`. Parse it back on retrieval.

The three canonical forms:

| Type | Canonical storage form | Example |
|---|---|---|
| `Pesel` | 11 digits | `44051401458` |
| `Nip` | 10 digits | `1234563218` |
| `Regon` | 9 or 14 digits | `123456785` / `12345678512347` |

**Writing to storage:**

```csharp
using PolishIdentifiers;

var nip = Nip.Parse("PL 123-456-32-18");

// Always store the canonical 10-digit form
string stored = nip.ToString(); // "1234563218"
```

**Reading from trusted storage** (the value was validated before it was written):

```csharp
using PolishIdentifiers;

// Parse throws NipValidationException on malformed stored data
var nip = Nip.Parse(storedString);
```

**Reading from untrusted or imported data** (external systems, user uploads):

```csharp
using PolishIdentifiers;

if (!Nip.TryParse(rawString, out var nip, out var error))
{
    // 'error' is the first NipValidationError: InvalidCharacters, InvalidLength,
    // UnrecognizedFormat, or InvalidChecksum
    logger.LogWarning("Stored NIP is malformed: {Error}", error);
    return;
}
```

The same pattern applies to `Pesel` and `Regon`.

---

## Deduplication

`Nip`, `Pesel`, and `Regon` are value types with correct equality semantics. Two instances encoding the same identifier are equal regardless of the input format they were parsed from.

Use a `HashSet<T>` or `Dictionary<T, TValue>` as the deduplication container.

**Deduplicating NIP values from a mixed-format import:**

```csharp
using PolishIdentifiers;

var seen = new HashSet<Nip>();
var duplicates = new List<string>();

foreach (var raw in importedNipValues)
{
    // Normalize common user-input noise before parsing
    var normalized = raw.Trim().ToUpperInvariant();

    if (!Nip.TryParse(normalized, out var nip, out _))
        continue; // skip or log the invalid entry

    if (!seen.Add(nip))
        duplicates.Add(raw);
}
```

`Nip.Parse("123-456-32-18")` and `Nip.Parse("1234563218")` and `Nip.Parse("PL1234563218")` all produce the same value. The `HashSet<Nip>` will correctly identify and skip duplicates regardless of which input format each record used.

**Using NIP as a dictionary key for a company index:**

```csharp
using PolishIdentifiers;

var companyIndex = new Dictionary<Nip, CompanyRecord>();

foreach (var record in importedRecords)
{
    if (!Nip.TryParse(record.Nip.Trim().ToUpperInvariant(), out var nip, out _))
        continue;

    companyIndex[nip] = record; // last record for a given NIP wins
}
```

---

## Import loop with typed errors

For bulk validation of import data without allocating a strong type for every row, use the static `Validate` method. It is allocation-free and returns a `ValidationResult<TError>` with the first structured error on failure.

```csharp
using PolishIdentifiers;

var errors = new List<(int Row, string Value, NipValidationError Error)>();

for (int i = 0; i < rows.Count; i++)
{
    var result = Nip.Validate(rows[i].Nip);
    if (!result.IsValid)
        errors.Add((i + 1, rows[i].Nip, result.Error!.Value));
}
```

When you need both the parsed value and the error on failure, use `TryParse`:

```csharp
using PolishIdentifiers;

foreach (var row in rows)
{
    if (!Nip.TryParse(row.Nip.Trim().ToUpperInvariant(), out var nip, out var error))
    {
        row.MarkInvalid($"NIP: {error}");
        continue;
    }

    row.NormalizedNip = nip.ToString();
}
```

Use `Validate` when throughput matters and the parsed value is not needed. Use `TryParse` when both the value and the failure reason are required in a single call.

---

## Person vs. company contractor decision guide

Polish business practice uses different identifier combinations depending on the type of contractor.

### Natural person contractor

A natural person running a business (sole trader, self-employed):

| Identifier | Required | Notes |
|---|---|---|
| `Nip` | Yes | Tax identifier; required on invoices |
| `Pesel` | Usually | Personal identifier; required for some employment and tax forms |
| Polish ID card number | Sometimes | Required for formal identity verification; not yet in this library |

```csharp
using PolishIdentifiers;

// Validate NIP and PESEL at the form boundary
if (!Nip.TryParse(form.Nip.Trim().ToUpperInvariant(), out var nip, out var nipError))
    errors.Add($"NIP: {nipError}");

if (!Pesel.TryParse(form.Pesel, out var pesel, out var peselError))
    errors.Add($"PESEL: {peselError}");
```

### Legal entity contractor

A company, partnership, association, or other registered organization:

| Identifier | Required | Notes |
|---|---|---|
| `Nip` | Yes | Tax identifier; required on invoices |
| `Regon` | Usually | Statistical registration number; REGON-9 for the main entity |
| `Regon` (REGON-14) | Sometimes | If dealing with a specific registered branch or local unit |

```csharp
using PolishIdentifiers;

// Validate NIP and REGON at the form boundary
if (!Nip.TryParse(form.Nip.Trim().ToUpperInvariant(), out var nip, out var nipError))
    errors.Add($"NIP: {nipError}");

if (form.Regon is not null
    && !Regon.TryParse(form.Regon, out var regon, out var regonError))
{
    errors.Add($"REGON: {regonError}");
}

// If REGON is REGON-14, resolve the parent company
if (regon.IsRegon14)
{
    var parentRegon = regon.BaseRegon9;
    // look up or create the parent entity by parentRegon
}
```

### Disambiguation signal

NIP does not encode whether it was issued to a person or a company. Use the **presence of REGON** as the main signal:

- REGON present → likely a legal entity; expect REGON-9 or REGON-14
- REGON absent, PESEL present → likely a natural person
- Neither REGON nor PESEL → NIP only; entity type cannot be determined from identifiers alone

Document this rule in your domain model explicitly rather than relying on the identifier type to decide it.
