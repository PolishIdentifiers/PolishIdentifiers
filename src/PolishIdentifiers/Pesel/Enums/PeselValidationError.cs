namespace PolishIdentifiers;

/// <summary>
/// Identifies the specific rule violated when a PESEL string fails validation.
/// </summary>
/// <remarks>
/// Errors are reported in check order: length → characters → date → checksum.
/// Only the first violated rule is returned; subsequent rules are not evaluated.
/// </remarks>
public enum PeselValidationError
{
    /// <summary>The input does not consist of exactly 11 characters.</summary>
    InvalidLength,

    /// <summary>The input contains one or more characters that are not decimal digits (0–9).</summary>
    InvalidCharacters,

    /// <summary>
    /// The encoded date segment does not represent a valid calendar date
    /// within the supported range of 1800–2299.
    /// </summary>
    InvalidDate,

    /// <summary>
    /// The check digit (11th digit) does not match the value computed
    /// from the preceding ten digits using the PESEL weighting algorithm.
    /// </summary>
    InvalidChecksum,
}
