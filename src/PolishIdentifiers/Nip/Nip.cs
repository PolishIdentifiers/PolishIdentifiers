using System.ComponentModel;

namespace PolishIdentifiers;

/// <summary>
/// Represents a validated Polish tax identification number (NIP — Numer Identyfikacji Podatkowej).
/// </summary>
/// <remarks>
/// Instances are obtained through the parsing APIs or through <see cref="NipGenerator"/>.
/// Public parsing accepts the canonical 10-digit representation and the exact documented formatted NIP forms.
/// The default instance is not valid; accessing domain properties on it
/// throws <see cref="InvalidOperationException"/>. Use <see cref="IsDefault"/> to check before accessing.
/// </remarks>
[TypeConverter(typeof(NipTypeConverter))]
#if NET10_0_OR_GREATER
public readonly struct Nip
    : IEquatable<Nip>, IComparable<Nip>, IFormattable,
      IParsable<Nip>, ISpanParsable<Nip>
#else
public readonly struct Nip : IEquatable<Nip>, IComparable<Nip>, IFormattable
#endif
{
    private readonly ulong _value;
    private readonly bool _isInitialized;

    internal Nip(ulong value)
    {
        _value = value;
        _isInitialized = true;
    }

    // --- Factories ---

    /// <summary>
    /// Parses the string representation of a NIP number.
    /// </summary>
    /// <param name="value">
    /// A NIP string in canonical digits or one of the documented supported formatted representations.
    /// </param>
    /// <returns>A valid <see cref="Nip"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="NipValidationException">
    /// Thrown when <paramref name="value"/> is not a valid NIP number.
    /// The <see cref="NipValidationException.Error"/> property indicates the first validation rule violated.
    /// </exception>
    public static Nip Parse(string value)
    {
        if (value is null) throw new ArgumentNullException(nameof(value));
        return Parse(value.AsSpan());
    }

    /// <summary>
    /// Parses the span representation of a NIP number.
    /// </summary>
    /// <param name="value">
    /// A NIP span in canonical digits or one of the documented supported formatted representations.
    /// </param>
    /// <returns>A valid <see cref="Nip"/> instance.</returns>
    /// <exception cref="NipValidationException">
    /// Thrown when <paramref name="value"/> is not a valid NIP number.
    /// The <see cref="NipValidationException.Error"/> property indicates the first validation rule violated.
    /// </exception>
    public static Nip Parse(ReadOnlySpan<char> value)
    {
        if (!NipValidator.TryParseCore(value, out var parsedValue, out var error))
            throw new NipValidationException(error);

        return new Nip(parsedValue);
    }

    /// <summary>
    /// Attempts to parse the string representation of a NIP number without throwing exceptions.
    /// </summary>
    /// <param name="value">
    /// A NIP string in canonical digits or one of the documented supported formatted representations,
    /// or <see langword="null"/>.
    /// </param>
    /// <param name="result">
    /// When this method returns <see langword="true"/>, contains the parsed <see cref="Nip"/>;
    /// otherwise, <see langword="default"/>.
    /// </param>
    /// <returns><see langword="true"/> if <paramref name="value"/> was successfully parsed; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(string? value, out Nip result)
    {
        if (value is null) { result = default; return false; }
        return TryParse(value.AsSpan(), out result);
    }

    /// <summary>
    /// Attempts to parse the string representation of a NIP number without throwing exceptions
    /// and returns the first validation error when parsing fails.
    /// </summary>
    /// <param name="value">
    /// A NIP string in canonical digits or one of the documented supported formatted representations,
    /// or <see langword="null"/>.
    /// </param>
    /// <param name="result">
    /// When this method returns <see langword="true"/>, contains the parsed <see cref="Nip"/>;
    /// otherwise, <see langword="default"/>.
    /// </param>
    /// <param name="error">
    /// When this method returns <see langword="false"/>, contains the first <see cref="NipValidationError"/>
    /// encountered; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns><see langword="true"/> if <paramref name="value"/> was successfully parsed; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(string? value, out Nip result, out NipValidationError? error)
    {
        if (value is null)
        {
            result = default;
            error = NipValidationError.InvalidLength;
            return false;
        }

        return TryParse(value.AsSpan(), out result, out error);
    }

    /// <summary>
    /// Attempts to parse the string representation of a NIP number without throwing exceptions.
    /// This overload is recognised by ASP.NET Core Minimal APIs for route and query parameter binding
    /// on both <c>netstandard2.0</c> and <c>net10.0</c> targets.
    /// </summary>
    /// <param name="value">
    /// A NIP string in canonical digits or one of the documented supported formatted representations,
    /// or <see langword="null"/>.
    /// </param>
    /// <param name="_">Not used. Exists to satisfy the Minimal API binding convention.</param>
    /// <param name="result">
    /// When this method returns <see langword="true"/>, contains the parsed <see cref="Nip"/>;
    /// otherwise, <see langword="default"/>.
    /// </param>
    /// <returns><see langword="true"/> if <paramref name="value"/> was successfully parsed; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(string? value, IFormatProvider? _, out Nip result)
        => TryParse(value, out result);

    /// <summary>
    /// Attempts to parse the span representation of a NIP number without throwing exceptions.
    /// </summary>
    /// <param name="value">
    /// A NIP span in canonical digits or one of the documented supported formatted representations.
    /// </param>
    /// <param name="result">
    /// When this method returns <see langword="true"/>, contains the parsed <see cref="Nip"/>;
    /// otherwise, <see langword="default"/>.
    /// </param>
    /// <returns><see langword="true"/> if <paramref name="value"/> was successfully parsed; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(ReadOnlySpan<char> value, out Nip result)
    {
        return TryParse(value, out result, out _);
    }

    /// <summary>
    /// Attempts to parse the span representation of a NIP number without throwing exceptions
    /// and returns the first validation error when parsing fails.
    /// </summary>
    /// <param name="value">
    /// A NIP span in canonical digits or one of the documented supported formatted representations.
    /// </param>
    /// <param name="result">
    /// When this method returns <see langword="true"/>, contains the parsed <see cref="Nip"/>;
    /// otherwise, <see langword="default"/>.
    /// </param>
    /// <param name="error">
    /// When this method returns <see langword="false"/>, contains the first <see cref="NipValidationError"/>
    /// encountered; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns><see langword="true"/> if <paramref name="value"/> was successfully parsed; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(ReadOnlySpan<char> value, out Nip result, out NipValidationError? error)
    {
        if (!NipValidator.TryParseCore(value, out var parsedValue, out var actualError))
        {
            result = default;
            error = actualError;
            return false;
        }

        result = new Nip(parsedValue);
        error = null;
        return true;
    }

    /// <summary>
    /// Validates a string against all NIP rules without throwing exceptions or allocating a <see cref="Nip"/> instance.
    /// </summary>
    /// <param name="value">The string to validate, or <see langword="null"/>.</param>
    /// <returns>
    /// A <see cref="ValidationResult{TError}"/> indicating whether the value is valid, and if not,
    /// the first <see cref="NipValidationError"/> encountered. Validation order:
    /// characters → length → format → checksum.
    /// </returns>
    public static ValidationResult<NipValidationError> Validate(string? value)
        => NipValidator.Validate(value);

    /// <summary>
    /// Validates a character span against all NIP rules without throwing exceptions or allocating a <see cref="Nip"/> instance.
    /// </summary>
    /// <param name="value">The span to validate.</param>
    /// <returns>
    /// A <see cref="ValidationResult{TError}"/> indicating whether the value is valid, and if not,
    /// the first <see cref="NipValidationError"/> encountered. Validation order:
    /// characters → length → format → checksum.
    /// </returns>
    public static ValidationResult<NipValidationError> Validate(ReadOnlySpan<char> value)
        => NipValidator.Validate(value);

#if NET10_0_OR_GREATER
    // --- IParsable<Nip> / ISpanParsable<Nip> ---

    static Nip IParsable<Nip>.Parse(string s, IFormatProvider? _)
        => Parse(s);

    static bool IParsable<Nip>.TryParse(string? s, IFormatProvider? _, out Nip result)
        => TryParse(s, out result);

    static Nip ISpanParsable<Nip>.Parse(ReadOnlySpan<char> s, IFormatProvider? _)
        => Parse(s);

    static bool ISpanParsable<Nip>.TryParse(ReadOnlySpan<char> s, IFormatProvider? _, out Nip result)
        => TryParse(s, out result);
#endif

    // --- Domain properties ---

    /// <summary>
    /// Gets a value indicating whether this instance was obtained via <c>default</c>
    /// rather than through one of the <c>Parse</c>, <c>TryParse</c>, or <see cref="NipGenerator"/> methods.
    /// Accessing any domain property on a default instance throws <see cref="InvalidOperationException"/>.
    /// </summary>
    public bool IsDefault => !_isInitialized;

    private void ThrowIfDefault()
    {
        if (!_isInitialized)
            throw new InvalidOperationException(
                "Cannot access properties on a default Nip instance. Use Parse or TryParse.");
    }

    /// <summary>
    /// Gets the issuing tax office prefix encoded in the first three digits of the NIP.
    /// This identifies the tax office that originally issued the NIP.
    /// It does not identify the taxpayer's current competent tax office.
    /// </summary>
    /// <value>
    /// A 3-character string containing the zero-padded first three digits of the NIP,
    /// for example <c>"012"</c> for a NIP beginning with <c>0</c>.
    /// </value>
    /// <exception cref="InvalidOperationException">Thrown when accessed on a default instance.</exception>
    public string IssuingTaxOfficePrefix
    {
        get
        {
            ThrowIfDefault();
            return (_value / 10_000_000UL).ToString("D3", System.Globalization.CultureInfo.InvariantCulture);
        }
    }

    // --- Standard overrides ---

    /// <summary>
    /// Returns the 10-digit canonical string representation of the NIP number.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when called on a default instance.</exception>
    public override string ToString()
    {
        ThrowIfDefault();
        return _value.ToString("D10", System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Returns the NIP formatted according to the specified <see cref="NipFormat"/>.
    /// </summary>
    /// <param name="format">The output format to use.</param>
    /// <returns>A string representation in the requested format.</returns>
    /// <exception cref="InvalidOperationException">Thrown when called on a default instance.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="format"/> is not a supported <see cref="NipFormat"/> value.</exception>
    public string ToString(NipFormat format)
    {
        ThrowIfDefault();
        return NipFormatter.Format(_value, format);
    }

    /// <summary>
    /// Returns the 10-digit canonical string representation of the NIP number using the specified format.
    /// </summary>
    /// <param name="format">
    /// The format string. Comparison is case-insensitive. Accepted values are
    /// <see langword="null"/>, <c>""</c>, <c>"G"</c>, <c>"g"</c>, <c>"D10"</c>, and <c>"d10"</c>.
    /// All produce the same 10-digit output.
    /// </param>
    /// <param name="formatProvider">Ignored. NIP numbers are always formatted with invariant digits.</param>
    /// <returns>A 10-character decimal string, left-padded with zeros if necessary.</returns>
    /// <exception cref="InvalidOperationException">Thrown when called on a default instance.</exception>
    /// <exception cref="FormatException">Thrown when <paramref name="format"/> is not a supported value.</exception>
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        ThrowIfDefault();

        if (string.IsNullOrEmpty(format)
            || string.Equals(format, "G", StringComparison.OrdinalIgnoreCase)
            || string.Equals(format, "D10", StringComparison.OrdinalIgnoreCase))
        {
            return _value.ToString("D10", System.Globalization.CultureInfo.InvariantCulture);
        }

        throw new FormatException($"Unsupported format string '{format}'.");
    }

    // --- Equality / Comparison ---

    /// <summary>
    /// Indicates whether this instance is equal to another <see cref="Nip"/> instance.
    /// </summary>
    /// <param name="other">The instance to compare with.</param>
    /// <returns><see langword="true"/> if both instances represent the same NIP number; otherwise, <see langword="false"/>.</returns>
    public bool Equals(Nip other) => _isInitialized == other._isInitialized && _value == other._value;

    /// <summary>
    /// Indicates whether this instance is equal to a specified object.
    /// </summary>
    /// <param name="obj">The object to compare with this instance.</param>
    /// <returns>
    /// <see langword="true"/> when <paramref name="obj"/> is a <see cref="Nip"/> with the same value;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object? obj) => obj is Nip other && Equals(other);

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>A hash code based on the underlying NIP value.</returns>
    public override int GetHashCode() => unchecked((_value.GetHashCode() * 397) ^ _isInitialized.GetHashCode());

    /// <summary>
    /// Compares this instance to another <see cref="Nip"/> by numeric value.
    /// Default instances sort before initialized instances.
    /// </summary>
    /// <param name="other">The instance to compare with.</param>
    /// <returns>
    /// A negative integer if this instance precedes <paramref name="other"/>,
    /// zero if they are equal, or a positive integer if this instance follows <paramref name="other"/>.
    /// </returns>
    public int CompareTo(Nip other)
    {
        if (_isInitialized != other._isInitialized)
            return _isInitialized ? 1 : -1;

        return _value.CompareTo(other._value);
    }

    /// <summary>
    /// Determines whether two <see cref="Nip"/> instances represent the same number.
    /// </summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if both values are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(Nip left, Nip right) => left.Equals(right);

    /// <summary>
    /// Determines whether two <see cref="Nip"/> instances represent different numbers.
    /// </summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if the values are different; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(Nip left, Nip right) => !left.Equals(right);
}
