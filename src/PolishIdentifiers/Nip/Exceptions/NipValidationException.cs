namespace PolishIdentifiers;

/// <summary>
/// The exception thrown by <see cref="Nip.Parse(string)"/> and
/// <see cref="Nip.Parse(System.ReadOnlySpan{char})"/> when the input does not
/// represent a valid NIP number.
/// </summary>
/// <remarks>
/// Prefer <see cref="Nip.TryParse(string?, out Nip)"/> or <see cref="Nip.Validate(string?)"/>
/// in performance-sensitive or high-volume scenarios to avoid the cost of exception handling.
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
