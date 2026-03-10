namespace PolishIdentifiers;

/// <summary>
/// Specifies the output format for <see cref="Nip.ToString(NipFormat)"/>.
/// </summary>
public enum NipFormat
{
    /// <summary>Ten consecutive digits: <c>1234563218</c>.</summary>
    DigitsOnly,

    /// <summary>Digits separated by hyphens: <c>123-456-32-18</c>.</summary>
    Hyphenated,

    /// <summary>EU VAT prefix followed by ten digits: <c>PL1234563218</c>.</summary>
    VatEu,
}
