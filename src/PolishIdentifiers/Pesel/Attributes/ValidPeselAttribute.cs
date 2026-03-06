using System.ComponentModel.DataAnnotations;

namespace PolishIdentifiers;

/// <summary>
/// Validates that a string or <see cref="Pesel"/> value is a structurally correct PESEL number.
/// </summary>
/// <remarks>
/// <para>
/// A <see langword="null"/> value is considered valid — combine with <see cref="System.ComponentModel.DataAnnotations.RequiredAttribute"/>
/// when the field must also be present.
/// </para>
/// <para>
/// Intended for use on DTO properties at the application boundary (e.g. API request models).
/// Domain types should use <see cref="Pesel"/> directly instead of a string with this attribute.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class ValidPeselAttribute : ValidationAttribute
{
    public ValidPeselAttribute()
        : base("The {0} field is not a valid PESEL.")
    {
    }

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="value"/> is <see langword="null"/>,
    /// a valid PESEL string, or a non-default <see cref="Pesel"/> instance.
    /// </summary>
    public override bool IsValid(object? value) => value switch
    {
        null     => true,
        string s => Pesel.Validate(s).IsValid,
        Pesel p  => !p.IsDefault,
        _        => false,
    };

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
