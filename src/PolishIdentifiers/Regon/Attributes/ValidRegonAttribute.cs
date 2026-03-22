using System.ComponentModel.DataAnnotations;

namespace PolishIdentifiers;

/// <summary>
/// Validates that a string or <see cref="Regon"/> value is a structurally correct REGON number.
/// </summary>
/// <remarks>
/// <para>
/// A <see langword="null"/> value is considered valid — combine with <see cref="RequiredAttribute"/>
/// when the field must also be present.
/// </para>
/// <para>
/// Intended for use on DTO properties at the application boundary (e.g. API request models).
/// For string values, accepts canonical strict REGON input (9 or 14 digits) as
/// validated by <see cref="Regon.Validate(string?)"/>.
/// </para>
/// <para>
/// Domain types should use <see cref="Regon"/> directly instead of a string with this attribute.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class ValidRegonAttribute : ValidationAttribute
{
    /// <summary>
    /// Initializes a new instance of <see cref="ValidRegonAttribute"/>.
    /// </summary>
    public ValidRegonAttribute()
        : base("The {0} field is not a valid REGON.")
    {
    }

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="value"/> is <see langword="null"/>,
    /// a valid REGON string (9 or 14 digits), or a non-default <see cref="Regon"/> instance.
    /// </summary>
    public override bool IsValid(object? value) => value switch
    {
        null => true,
        string s => Regon.Validate(s).IsValid,
        Regon r => !r.IsDefault,
        _ => false,
    };

    /// <inheritdoc />
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (IsValid(value))
            return ValidationResult.Success;

        var members = validationContext.MemberName is { } name
            ? new[] { name }
            : null;

        return new ValidationResult(FormatErrorMessage(validationContext.DisplayName), members);
    }
}
