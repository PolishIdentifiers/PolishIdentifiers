namespace PolishIdentifiers;

/// <summary>
/// The exception thrown by <see cref="Regon.Parse(string)"/> and <see cref="Regon.Parse(System.ReadOnlySpan{char})"/>
/// when the input is not a valid REGON number.
/// </summary>
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
