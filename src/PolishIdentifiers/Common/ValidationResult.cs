namespace PolishIdentifiers;

/// <summary>
/// Represents the outcome of a validation operation, carrying either a success state
/// or a structured error of type <typeparamref name="TError"/>.
/// </summary>
/// <typeparam name="TError">An enum type representing the possible validation errors.</typeparam>
/// <remarks>
/// Always obtain instances via <see cref="Valid"/> or <see cref="Failure(TError)"/>.
/// The default instance is not valid and will cause <see cref="Match{TResult}"/> to throw.
/// </remarks>
public readonly struct ValidationResult<TError>
    where TError : struct, Enum
{
    /// <summary>
    /// Gets a value indicating whether the validation succeeded.
    /// </summary>
    public bool IsValid { get; }

    /// <summary>
    /// Gets the validation error, or <see langword="null"/> when <see cref="IsValid"/> is <see langword="true"/>.
    /// </summary>
    public TError? Error { get; }

    private ValidationResult(bool isValid, TError? error)
    {
        IsValid = isValid;
        Error = error;
    }

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    /// <returns>A <see cref="ValidationResult{TError}"/> representing a valid state.</returns>
    public static ValidationResult<TError> Valid() => new(true, null);

    /// <summary>
    /// Creates a failed validation result with the specified error.
    /// </summary>
    /// <param name="error">The validation error that caused the failure.</param>
    /// <returns>A <see cref="ValidationResult{TError}"/> representing a failed state.</returns>
    public static ValidationResult<TError> Failure(TError error) => new(false, error);

    /// <summary>
    /// Projects the result into a single value by invoking either <paramref name="onValid"/>
    /// when the result is valid, or <paramref name="onError"/> with the error value when it is not.
    /// </summary>
    /// <typeparam name="TResult">The type of the projected value.</typeparam>
    /// <param name="onValid">The delegate invoked when the result is valid.</param>
    /// <param name="onError">The delegate invoked with the error value when the result is invalid.</param>
    /// <returns>The value returned by whichever delegate was invoked.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the struct was default-initialized (<c>IsValid</c> is <see langword="false"/>
    /// but <c>Error</c> is <see langword="null"/>). Always obtain instances via
    /// <see cref="Valid"/> or <see cref="Failure(TError)"/>.
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
