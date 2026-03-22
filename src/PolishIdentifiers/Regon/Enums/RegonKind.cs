namespace PolishIdentifiers;

/// <summary>
/// Distinguishes between the two structural variants of a REGON number.
/// </summary>
public enum RegonKind
{
    /// <summary>
    /// A 9-digit REGON number.
    /// </summary>
    Regon9,

    /// <summary>
    /// A 14-digit REGON number.
    /// The first 9 digits form the embedded REGON-9 base.
    /// </summary>
    Regon14,
}
