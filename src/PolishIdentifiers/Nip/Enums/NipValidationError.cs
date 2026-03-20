namespace PolishIdentifiers;

/// <summary>
/// Identifies the specific rule violated when a NIP string fails validation.
/// </summary>
/// <remarks>
/// Errors are reported in check order: characters → length → format → checksum.
/// Only the first violated rule is returned; subsequent rules are not evaluated.
/// </remarks>
public enum NipValidationError
{
    /// <summary>
    /// The validated input contains one or more characters outside the supported public NIP character set:
    /// decimal digits, uppercase <c>P</c>, uppercase <c>L</c>, space, and hyphen.
    /// </summary>
    InvalidCharacters,

    /// <summary>
    /// The validated input does not satisfy the public NIP length requirements.
    /// This includes <see langword="null"/>, empty input, and digit-only input that does not consist of exactly 10 digits.
    /// </summary>
    InvalidLength,

    /// <summary>
    /// The check digit (10th digit) does not match the value computed
    /// from the preceding nine digits using the NIP weighting algorithm (mod 11),
    /// or the weighted sum modulo 11 equals 10 (no valid check digit exists for this combination).
    /// </summary>
    InvalidChecksum,

    /// <summary>
    /// The input uses only otherwise supported characters but does not match any of
    /// the documented public NIP text representations.
    /// </summary>
    UnrecognizedFormat,
}
