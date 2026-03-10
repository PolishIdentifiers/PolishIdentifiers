namespace PolishIdentifiers;

/// <summary>
/// Represents a validated Polish tax identification number (NIP — Numer Identyfikacji Podatkowej).
/// </summary>
/// <remarks>
/// Instances can only be obtained through <see cref="Parse(string)"/>, <see cref="TryParse(string?, out Nip)"/>,
/// or <see cref="NipGenerator"/>. The default instance is not valid; accessing domain properties on it
/// throws <see cref="InvalidOperationException"/>. Use <see cref="IsDefault"/> to check before accessing.
/// </remarks>
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

    // --- Strict factories ---

    /// <summary>
    /// Parses the string representation of a NIP number.
    /// </summary>
    /// <param name="value">A 10-digit string representing a NIP number.</param>
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
    /// <param name="value">A 10-character span representing a NIP number.</param>
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
    /// <param name="value">A 10-digit string representing a NIP number, or <see langword="null"/>.</param>
    /// <param name="nip">
    /// When this method returns <see langword="true"/>, contains the parsed <see cref="Nip"/>;
    /// otherwise, <see langword="default"/>.
    /// </param>
    /// <returns><see langword="true"/> if <paramref name="value"/> was successfully parsed; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(string? value, out Nip nip)
    {
        if (value is null) { nip = default; return false; }
        return TryParse(value.AsSpan(), out nip);
    }

    /// <summary>
    /// Attempts to parse the span representation of a NIP number without throwing exceptions.
    /// </summary>
    /// <param name="value">A 10-character span representing a NIP number.</param>
    /// <param name="nip">
    /// When this method returns <see langword="true"/>, contains the parsed <see cref="Nip"/>;
    /// otherwise, <see langword="default"/>.
    /// </param>
    /// <returns><see langword="true"/> if <paramref name="value"/> was successfully parsed; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(ReadOnlySpan<char> value, out Nip nip)
    {
        if (!NipValidator.TryParseCore(value, out var parsedValue, out _))
        {
            nip = default;
            return false;
        }

        nip = new Nip(parsedValue);
        return true;
    }

    /// <summary>
    /// Validates a string against all NIP rules without throwing exceptions or allocating a <see cref="Nip"/> instance.
    /// </summary>
    /// <param name="value">The string to validate, or <see langword="null"/>.</param>
    /// <returns>
    /// A <see cref="ValidationResult{TError}"/> indicating whether the value is valid, and if not,
    /// the first <see cref="NipValidationError"/> encountered. Validation order:
    /// characters → length → checksum.
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
    /// characters → length → checksum.
    /// </returns>
    public static ValidationResult<NipValidationError> Validate(ReadOnlySpan<char> value)
        => NipValidator.Validate(value);

    // --- Formatted factories ---

    /// <summary>
    /// Parses a NIP string in one of the five recognized formatted patterns.
    /// </summary>
    /// <param name="value">A NIP string in canonical, hyphenated, or EU VAT format.</param>
    /// <returns>A valid <see cref="Nip"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="NipValidationException">
    /// Thrown when <paramref name="value"/> is not a valid NIP number or is not in a recognized format.
    /// </exception>
    public static Nip ParseFormatted(string value)
    {
        if (value is null) throw new ArgumentNullException(nameof(value));
        return ParseFormatted(value.AsSpan());
    }

    /// <summary>
    /// Parses a NIP span in one of the five recognized formatted patterns.
    /// </summary>
    /// <param name="value">A NIP span in canonical, hyphenated, or EU VAT format.</param>
    /// <returns>A valid <see cref="Nip"/> instance.</returns>
    /// <exception cref="NipValidationException">
    /// Thrown when <paramref name="value"/> is not a valid NIP number or is not in a recognized format.
    /// </exception>
    public static Nip ParseFormatted(ReadOnlySpan<char> value)
    {
        if (!TryParseFormattedCore(value, out var nip, out var error))
            throw new NipValidationException(error);

        return nip;
    }

    /// <summary>
    /// Attempts to parse a NIP string in one of the five recognized formatted patterns.
    /// </summary>
    /// <param name="value">A NIP string in canonical, hyphenated, or EU VAT format, or <see langword="null"/>.</param>
    /// <param name="nip">
    /// When this method returns <see langword="true"/>, contains the parsed <see cref="Nip"/>;
    /// otherwise, <see langword="default"/>.
    /// </param>
    /// <returns><see langword="true"/> if <paramref name="value"/> was successfully parsed; otherwise, <see langword="false"/>.</returns>
    public static bool TryParseFormatted(string? value, out Nip nip)
    {
        if (value is null) { nip = default; return false; }
        return TryParseFormatted(value.AsSpan(), out nip);
    }

    /// <summary>
    /// Attempts to parse a NIP span in one of the five recognized formatted patterns.
    /// </summary>
    /// <param name="value">A NIP span in canonical, hyphenated, or EU VAT format.</param>
    /// <param name="nip">
    /// When this method returns <see langword="true"/>, contains the parsed <see cref="Nip"/>;
    /// otherwise, <see langword="default"/>.
    /// </param>
    /// <returns><see langword="true"/> if <paramref name="value"/> was successfully parsed; otherwise, <see langword="false"/>.</returns>
    public static bool TryParseFormatted(ReadOnlySpan<char> value, out Nip nip)
    {
        return TryParseFormattedCore(value, out nip, out _);
    }

    /// <summary>
    /// Validates a formatted NIP string against all NIP rules.
    /// Accepts five recognized input patterns: canonical, hyphenated, PL prefix variants.
    /// </summary>
    /// <param name="value">The formatted string to validate, or <see langword="null"/>.</param>
    /// <returns>
    /// A <see cref="ValidationResult{TError}"/> indicating whether the value is valid, and if not,
    /// the first <see cref="NipValidationError"/> encountered.
    /// </returns>
    public static ValidationResult<NipValidationError> ValidateFormatted(string? value)
    {
        if (value is null)
            return ValidationResult<NipValidationError>.Failure(NipValidationError.UnrecognizedFormat);
        return ValidateFormatted(value.AsSpan());
    }

    /// <summary>
    /// Validates a formatted NIP span against all NIP rules.
    /// Accepts five recognized input patterns: canonical, hyphenated, PL prefix variants.
    /// </summary>
    /// <param name="value">The formatted span to validate.</param>
    /// <returns>
    /// A <see cref="ValidationResult{TError}"/> indicating whether the value is valid, and if not,
    /// the first <see cref="NipValidationError"/> encountered.
    /// </returns>
    public static ValidationResult<NipValidationError> ValidateFormatted(ReadOnlySpan<char> value)
    {
        Span<char> digits = stackalloc char[10];
        if (!NipInputNormalizer.TryNormalize(value, digits))
            return ValidationResult<NipValidationError>.Failure(NipValidationError.UnrecognizedFormat);

        return NipValidator.Validate(digits);
    }

    private static bool TryParseFormattedCore(ReadOnlySpan<char> value, out Nip nip, out NipValidationError error)
    {
        Span<char> digits = stackalloc char[10];
        if (!NipInputNormalizer.TryNormalize(value, digits))
        {
            nip = default;
            error = NipValidationError.UnrecognizedFormat;
            return false;
        }

        if (!NipValidator.TryParseCore(digits, out var parsedValue, out error))
        {
            nip = default;
            return false;
        }

        nip = new Nip(parsedValue);
        return true;
    }

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
    /// Gets the issuing tax office prefix (first three digits of the NIP).
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessed on a default instance.</exception>
    public int IssuingTaxOfficePrefix
    {
        get
        {
            ThrowIfDefault();
            return (int)(_value / 10_000_000UL);
        }
    }

    // --- Standard overrides ---

    /// <summary>
    /// Returns the 10-digit canonical string representation of the NIP number.
    /// </summary>
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
    public string ToString(NipFormat format)
    {
        ThrowIfDefault();
        return NipFormatter.Format(_value, format);
    }

    /// <summary>
    /// Returns the 10-digit canonical string representation of the NIP number using the specified format.
    /// </summary>
    /// <param name="format">
    /// The format string. Accepted values are <see langword="null"/>, <c>""</c>, <c>"G"</c>, and <c>"D10"</c>.
    /// All produce the same 10-digit output.
    /// </param>
    /// <param name="formatProvider">Ignored. NIP numbers are always formatted with invariant digits.</param>
    /// <returns>A 10-character decimal string, left-padded with zeros if necessary.</returns>
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
