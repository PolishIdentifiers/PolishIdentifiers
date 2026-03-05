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
    /// <exception cref="InvalidOperationException">
    /// Thrown when the struct was default-initialized (<c>IsValid</c> is <see langword="false"/>
    /// but <c>Error</c> is <see langword="null"/>). Always obtain instances via
    /// <see cref="Valid"/> or <see cref="Failure"/>.
    /// </exception>
    public TResult Match<TResult>(Func<TResult> onValid, Func<TError, TResult> onError)
    {
        if (IsValid)
            return onValid();

        if (Error is null)
            throw new InvalidOperationException(
                $"ValidationResult<{typeof(TError).Name}> is in an invalid state: " +
                "IsValid is false but Error is null. " +
                "Always obtain instances via Valid() or Failure().");

        return onError(Error.Value);
    }
}
