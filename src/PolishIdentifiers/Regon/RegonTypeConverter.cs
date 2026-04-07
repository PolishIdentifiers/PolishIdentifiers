using System.ComponentModel;
using System.Globalization;

namespace PolishIdentifiers;

/// <summary>
/// Converts <see cref="Regon"/> values to and from <see cref="string"/>.
/// </summary>
/// <remarks>
/// Registered on <see cref="Regon"/> via <c>[TypeConverter]</c>. Used by
/// ComponentModel-based consumers such as WinForms/WPF data binding and
/// <c>ConfigurationBinder</c>. On modern .NET, ASP.NET Core can also bind
/// <see cref="Regon"/> through its parsing interfaces.
/// <para>
/// <c>CanConvertFrom</c> and <c>CanConvertTo</c> explicitly support <see cref="string"/>
/// and otherwise defer to <see cref="TypeConverter"/> base behavior.
/// </para>
/// <para>
/// <c>ConvertFrom</c> accepts the same canonical 9-digit or 14-digit input as
/// <see cref="Regon.Parse(string)"/>. On failure a <see cref="FormatException"/> is
/// thrown with the domain <see cref="RegonValidationException"/> as
/// <see cref="Exception.InnerException"/>. Non-string inputs fall back to
/// <see cref="TypeConverter"/> base behavior.
/// </para>
/// <para>
/// When converting a non-default <see cref="Regon"/> to <see cref="string"/>,
/// <c>ConvertTo</c> calls <see cref="Regon.ToString()"/> and returns 9 digits for a
/// REGON-9 or 14 digits for a REGON-14, preserving the variant on round-trip.
/// Passing a default instance when converting to <see cref="string"/> throws
/// <see cref="InvalidOperationException"/> propagated from <see cref="Regon.ToString()"/>.
/// Other conversions fall back to <see cref="TypeConverter"/> base behavior; in particular,
/// <c>ConvertTo(null, typeof(string))</c> returns <see cref="string.Empty"/>.
/// </para>
/// </remarks>
public sealed class RegonTypeConverter : TypeConverter
{
    /// <inheritdoc />
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    /// <inheritdoc />
    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
        => destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

    /// <inheritdoc />
    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string s)
        {
            try
            {
                return Regon.Parse(s);
            }
            catch (RegonValidationException ex)
            {
                throw new FormatException(ex.Message, ex);
            }
        }

        return base.ConvertFrom(context, culture, value);
    }

    /// <inheritdoc />
    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (destinationType == typeof(string) && value is Regon regon)
            return regon.ToString();

        return base.ConvertTo(context, culture, value, destinationType);
    }
}
