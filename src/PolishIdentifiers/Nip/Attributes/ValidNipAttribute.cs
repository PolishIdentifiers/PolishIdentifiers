using System.ComponentModel.DataAnnotations;

namespace PolishIdentifiers;

/// <summary>
/// Validates that a string or <see cref="Nip"/> value is a structurally correct NIP number.
/// </summary>
/// <remarks>
/// <para>
/// A <see langword="null"/> value is considered valid — combine with <see cref="RequiredAttribute"/>
/// when the field must also be present.
/// </para>
/// <para>
/// Intended for use on DTO properties at the application boundary (e.g. API request models).
/// For string values, accepts the same canonical and recognized formatted NIP inputs as
/// <see cref="Nip.ValidateFormatted(string?)"/>.
/// </para>
/// <para>
/// Domain types should use <see cref="Nip"/> directly instead of a string with this attribute.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class ValidNipAttribute : ValidationAttribute
{
    /// <summary>
    /// Initializes a new instance of <see cref="ValidNipAttribute"/>.
    /// </summary>
    public ValidNipAttribute()
        : base("The {0} field is not a valid NIP.")
    {
    }

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="value"/> is <see langword="null"/>,
    /// a valid NIP string in canonical or recognized formatted form, or a non-default <see cref="Nip"/> instance.
    /// </summary>
    public override bool IsValid(object? value) => value switch
    {
        null   => true,
        string s => Nip.ValidateFormatted(s).IsValid,
        Nip n  => !n.IsDefault,
        _      => false,
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
