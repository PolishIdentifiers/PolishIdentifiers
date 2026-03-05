namespace PolishIdentifiers;

public readonly struct ValidationResult<TError>
    where TError : struct, Enum
{
    public bool IsValid { get; }
    public TError? Error { get; }

    private ValidationResult(bool isValid, TError? error)
    {
        IsValid = isValid;
        Error = error;
    }

    public static ValidationResult<TError> Valid() => new(true, null);
    public static ValidationResult<TError> Failure(TError error) => new(false, error);

    /// <summary>
    /// Projects the result into a single value by invoking either <paramref name="onValid"/>
    /// when the result is valid, or <paramref name="onError"/> with the error value when it is not.
    /// </summary>
    public TResult Match<TResult>(Func<TResult> onValid, Func<TError, TResult> onError)
        => IsValid ? onValid() : onError(Error!.Value);
}
