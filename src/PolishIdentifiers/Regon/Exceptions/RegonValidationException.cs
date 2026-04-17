namespace PolishIdentifiers;

/// <summary>
/// The exception thrown by <see cref="Regon.Parse(string)"/> and <see cref="Regon.Parse(System.ReadOnlySpan{char})"/>
/// when the input is not a valid REGON number.
/// </summary>
/// <remarks>
/// Prefer <see cref="Regon.TryParse(string?, out Regon, out RegonValidationError?)"/>,
/// <see cref="Regon.TryParse(string?, out Regon)"/>, or <see cref="Regon.Validate(string?)"/>
/// in performance-sensitive or high-volume scenarios to avoid the cost of exception handling.
/// For normal application-boundary code, the typed-error overload is recommended.
/// The exception message intentionally reports only the validation error and does not echo the raw REGON value,
/// which helps reduce accidental exposure in logs.
/// </remarks>
public class RegonValidationException : Exception
{
    /// <summary>
    /// Gets the first <see cref="RegonValidationError"/> that caused the failure.
    /// </summary>
    public RegonValidationError Error { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="RegonValidationException"/> with the specified error code.
    /// </summary>
    /// <param name="error">The validation rule that was violated.</param>
    public RegonValidationException(RegonValidationError error)
        : base($"REGON validation failed: {error}")
    {
        Error = error;
    }
}
