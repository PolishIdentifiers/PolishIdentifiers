namespace PolishIdentifiers;

public class PeselValidationException : Exception
{
    public PeselValidationError? Error { get; }

    public PeselValidationException()
        : base("PESEL validation failed.")
    {
    }

    public PeselValidationException(string message)
        : base(message)
    {
    }

    public PeselValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public PeselValidationException(PeselValidationError error)
        : base($"PESEL validation failed: {error}")
    {
        Error = error;
    }
}
