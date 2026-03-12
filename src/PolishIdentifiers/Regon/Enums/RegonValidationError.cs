namespace PolishIdentifiers;

/// <summary>
/// Identifies the first validation rule violated when parsing or validating a REGON number.
/// Rules are checked in order: <see cref="InvalidCharacters"/> → <see cref="InvalidLength"/> → <see cref="InvalidChecksum"/>.
/// </summary>
public enum RegonValidationError
{
    /// <summary>
    /// The input contains characters other than ASCII digits.
    /// This is the first rule checked.
    /// </summary>
    InvalidCharacters,

    /// <summary>
    /// The input length is neither 9 (REGON-9) nor 14 (REGON-14).
    /// Checked after all characters pass as digits.
    /// </summary>
    InvalidLength,

    /// <summary>
    /// The check digit is incorrect, or for a REGON-14, the embedded REGON-9 base is invalid.
    /// Checked last.
    /// </summary>
    InvalidChecksum,
}
