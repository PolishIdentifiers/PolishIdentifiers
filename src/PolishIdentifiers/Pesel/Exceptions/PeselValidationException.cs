namespace PolishIdentifiers;

public class PeselValidationException : Exception
{
    public PeselValidationError Error { get; }

    public PeselValidationException(PeselValidationError error)
        : base($"PESEL validation failed: {error}")
    {
        Error = error;
    }
}
