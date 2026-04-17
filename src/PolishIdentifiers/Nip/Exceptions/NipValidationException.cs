namespace PolishIdentifiers;

/// <summary>
/// The exception thrown by the <see cref="Nip"/> parsing APIs when the input does not
/// represent a valid NIP number.
/// </summary>
/// <remarks>
/// Prefer <see cref="Nip.TryParse(string?, out Nip, out NipValidationError?)"/>,
/// <see cref="Nip.TryParse(string?, out Nip)"/>, or <see cref="Nip.Validate(string?)"/>
/// in performance-sensitive or high-volume scenarios to avoid the cost of exception handling.
/// For normal application-boundary code, the typed-error overload is recommended.
/// The exception message intentionally reports only the validation error and does not echo the raw NIP value,
/// which helps reduce accidental exposure in logs.
/// </remarks>
public class NipValidationException : Exception
{
    /// <summary>
    /// Gets the specific validation rule that was violated.
    /// </summary>
    public NipValidationError Error { get; }

    /// <summary>
    /// Initializes a new instance with the specified validation error.
    /// </summary>
    /// <param name="error">The validation rule that caused the failure.</param>
    public NipValidationException(NipValidationError error)
        : base($"NIP validation failed: {error}")
    {
        Error = error;
    }
}
