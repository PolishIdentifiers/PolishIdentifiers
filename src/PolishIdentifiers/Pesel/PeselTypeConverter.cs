using System.ComponentModel;
using System.Globalization;

namespace PolishIdentifiers;

/// <summary>
/// Converts <see cref="Pesel"/> values to and from <see cref="string"/>.
/// </summary>
/// <remarks>
/// Registered on <see cref="Pesel"/> via <c>[TypeConverter]</c>. Used by
/// ComponentModel-based consumers such as WinForms/WPF data binding and
/// <c>ConfigurationBinder</c>. On modern .NET, ASP.NET Core can also bind
/// <see cref="Pesel"/> through its parsing interfaces.
/// <para>
/// <c>CanConvertFrom</c> and <c>CanConvertTo</c> explicitly support <see cref="string"/>
/// and otherwise defer to <see cref="TypeConverter"/> base behavior.
/// </para>
/// <para>
/// <c>ConvertFrom</c> accepts the same canonical 11-digit input as <see cref="Pesel.Parse(string)"/>.
/// On failure a <see cref="FormatException"/> is thrown with the domain
/// <see cref="PeselValidationException"/> as <see cref="Exception.InnerException"/>.
/// Non-string inputs fall back to <see cref="TypeConverter"/> base behavior.
/// </para>
/// <para>
/// When converting a non-default <see cref="Pesel"/> to <see cref="string"/>,
/// <c>ConvertTo</c> calls <see cref="Pesel.ToString()"/> and returns the canonical
/// 11-digit string. Passing a default instance when converting to <see cref="string"/>
/// throws <see cref="InvalidOperationException"/> propagated from <see cref="Pesel.ToString()"/>.
/// Other conversions fall back to <see cref="TypeConverter"/> base behavior; in particular,
/// <c>ConvertTo(null, typeof(string))</c> returns <see cref="string.Empty"/>.
/// </para>
/// </remarks>
public sealed class PeselTypeConverter : TypeConverter
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
                return Pesel.Parse(s);
            }
            catch (PeselValidationException ex)
            {
                throw new FormatException(ex.Message, ex);
            }
        }

        return base.ConvertFrom(context, culture, value);
    }

    /// <inheritdoc />
    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (destinationType == typeof(string) && value is Pesel pesel)
            return pesel.ToString();

        return base.ConvertTo(context, culture, value, destinationType);
    }
}
