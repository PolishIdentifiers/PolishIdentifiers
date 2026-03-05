namespace PolishIdentifiers;

/// <summary>
/// The exception thrown by <see cref="Pesel.Parse(string)"/> and
/// <see cref="Pesel.Parse(System.ReadOnlySpan{char})"/> when the input does not
/// represent a valid PESEL number.
/// </summary>
/// <remarks>
/// Prefer <see cref="Pesel.TryParse(string?, out Pesel)"/> or <see cref="Pesel.Validate(string?)"/>
/// in performance-sensitive or high-volume scenarios to avoid the cost of exception handling.
/// </remarks>
public class PeselValidationException : Exception
{
    /// <summary>
    /// Gets the specific validation rule that was violated.
    /// </summary>
    public PeselValidationError Error { get; }

    /// <summary>
    /// Initializes a new instance with the specified validation error.
    /// </summary>
    /// <param name="error">The validation rule that caused the failure.</param>
    public PeselValidationException(PeselValidationError error)
        : base($"PESEL validation failed: {error}")
    {
        Error = error;
    }
}
