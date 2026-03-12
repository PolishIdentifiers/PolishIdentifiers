namespace PolishIdentifiers;

/// <summary>
/// Distinguishes between the two structural variants of a REGON number.
/// </summary>
public enum RegonKind
{
    /// <summary>
    /// A 9-digit REGON number identifying the primary entity (company, institution).
    /// </summary>
    Main,

    /// <summary>
    /// A 14-digit REGON number identifying a local unit (branch, outlet).
    /// The first 9 digits form the REGON-9 of the parent entity.
    /// </summary>
    Local,
}
