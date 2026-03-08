namespace PolishIdentifiers;

/// <summary>
/// Identifies the specific rule violated when a NIP string fails validation.
/// </summary>
/// <remarks>
/// Errors are reported in check order: characters → length → checksum.
/// For the <c>*Formatted</c> path, <see cref="UnrecognizedFormat"/> is reported
/// when the input does not match any of the five supported input formats.
/// Only the first violated rule is returned; subsequent rules are not evaluated.
/// </remarks>
public enum NipValidationError
{
    /// <summary>The input contains one or more characters that are not decimal digits (0–9) after normalization.</summary>
    InvalidCharacters,

    /// <summary>The input does not consist of exactly 10 characters after normalization.</summary>
    InvalidLength,

    /// <summary>
    /// The check digit (10th digit) does not match the value computed
    /// from the preceding nine digits using the NIP weighting algorithm (mod 11),
    /// or the weighted sum modulo 11 equals 10 (no valid check digit exists for this combination).
    /// </summary>
    InvalidChecksum,

    /// <summary>
    /// The input does not match any of the recognized formatted NIP patterns.
    /// Only returned by the <c>*Formatted</c> methods.
    /// </summary>
    UnrecognizedFormat,
}
