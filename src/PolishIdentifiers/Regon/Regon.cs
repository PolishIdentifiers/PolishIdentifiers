namespace PolishIdentifiers;

/// <summary>
/// Represents a validated Polish statistical identification number (REGON — Rejestr Gospodarki Narodowej).
/// Supports both 9-digit (REGON-9, primary entity) and 14-digit (REGON-14, local unit) variants.
/// </summary>
/// <remarks>
/// Instances can only be obtained through <see cref="Parse(string)"/>, <see cref="TryParse(string?, out Regon)"/>,
/// or <see cref="RegonGenerator"/>. The default instance is not valid; accessing domain properties on it
/// throws <see cref="InvalidOperationException"/>. Use <see cref="IsDefault"/> to check before accessing.
/// </remarks>
#if NET10_0_OR_GREATER
public readonly struct Regon
    : IEquatable<Regon>, IComparable<Regon>, IFormattable,
      IParsable<Regon>, ISpanParsable<Regon>
#else
public readonly struct Regon : IEquatable<Regon>, IComparable<Regon>, IFormattable
#endif
{
    private readonly ulong _value;
    private readonly bool _isLocal;
    private readonly bool _isInitialized;

    internal Regon(ulong value, bool isLocal)
    {
        _value = value;
        _isLocal = isLocal;
        _isInitialized = true;
    }

    // --- Strict factories ---

    /// <summary>
    /// Parses the string representation of a REGON number.
    /// </summary>
    /// <param name="value">A 9- or 14-digit string representing a REGON number.</param>
    /// <returns>A valid <see cref="Regon"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="RegonValidationException">
    /// Thrown when <paramref name="value"/> is not a valid REGON number.
    /// The <see cref="RegonValidationException.Error"/> property indicates the first validation rule violated.
    /// </exception>
    public static Regon Parse(string value)
    {
        if (value is null) throw new ArgumentNullException(nameof(value));
        return Parse(value.AsSpan());
    }

    /// <summary>
    /// Parses the span representation of a REGON number.
    /// </summary>
    /// <param name="value">A 9- or 14-character span representing a REGON number.</param>
    /// <returns>A valid <see cref="Regon"/> instance.</returns>
    /// <exception cref="RegonValidationException">
    /// Thrown when <paramref name="value"/> is not a valid REGON number.
    /// The <see cref="RegonValidationException.Error"/> property indicates the first validation rule violated.
    /// </exception>
    public static Regon Parse(ReadOnlySpan<char> value)
    {
        if (!RegonValidator.TryParseCore(value, out var parsedValue, out var isLocal, out var error))
            throw new RegonValidationException(error);

        return new Regon(parsedValue, isLocal);
    }

    /// <summary>
    /// Attempts to parse the string representation of a REGON number without throwing exceptions.
    /// </summary>
    /// <param name="value">A 9- or 14-digit string representing a REGON number, or <see langword="null"/>.</param>
    /// <param name="regon">
    /// When this method returns <see langword="true"/>, contains the parsed <see cref="Regon"/>;
    /// otherwise, <see langword="default"/>.
    /// </param>
    /// <returns><see langword="true"/> if <paramref name="value"/> was successfully parsed; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(string? value, out Regon regon)
    {
        if (value is null) { regon = default; return false; }
        return TryParse(value.AsSpan(), out regon);
    }

    /// <summary>
    /// Attempts to parse the span representation of a REGON number without throwing exceptions.
    /// </summary>
    /// <param name="value">A 9- or 14-character span representing a REGON number.</param>
    /// <param name="regon">
    /// When this method returns <see langword="true"/>, contains the parsed <see cref="Regon"/>;
    /// otherwise, <see langword="default"/>.
    /// </param>
    /// <returns><see langword="true"/> if <paramref name="value"/> was successfully parsed; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(ReadOnlySpan<char> value, out Regon regon)
    {
        if (!RegonValidator.TryParseCore(value, out var parsedValue, out var isLocal, out _))
        {
            regon = default;
            return false;
        }

        regon = new Regon(parsedValue, isLocal);
        return true;
    }

    /// <summary>
    /// Validates a string against all REGON rules without throwing exceptions or allocating a <see cref="Regon"/> instance.
    /// </summary>
    /// <param name="value">The string to validate, or <see langword="null"/>.</param>
    /// <returns>
    /// A <see cref="ValidationResult{TError}"/> indicating whether the value is valid, and if not,
    /// the first <see cref="RegonValidationError"/> encountered. Validation order:
    /// characters → length → checksum.
    /// </returns>
    public static ValidationResult<RegonValidationError> Validate(string? value)
        => RegonValidator.Validate(value);

    /// <summary>
    /// Validates a character span against all REGON rules without throwing exceptions or allocating a <see cref="Regon"/> instance.
    /// </summary>
    /// <param name="value">The span to validate.</param>
    /// <returns>
    /// A <see cref="ValidationResult{TError}"/> indicating whether the value is valid, and if not,
    /// the first <see cref="RegonValidationError"/> encountered. Validation order:
    /// characters → length → checksum.
    /// </returns>
    public static ValidationResult<RegonValidationError> Validate(ReadOnlySpan<char> value)
        => RegonValidator.Validate(value);

#if NET10_0_OR_GREATER
    // --- IParsable<Regon> / ISpanParsable<Regon> ---

    static Regon IParsable<Regon>.Parse(string s, IFormatProvider? _)
        => Parse(s);

    static bool IParsable<Regon>.TryParse(string? s, IFormatProvider? _, out Regon result)
        => TryParse(s, out result);

    static Regon ISpanParsable<Regon>.Parse(ReadOnlySpan<char> s, IFormatProvider? _)
        => Parse(s);

    static bool ISpanParsable<Regon>.TryParse(ReadOnlySpan<char> s, IFormatProvider? _, out Regon result)
        => TryParse(s, out result);
#endif

    // --- Domain properties ---

    /// <summary>
    /// Gets a value indicating whether this instance was obtained via <c>default</c>
    /// rather than through one of the <c>Parse</c>, <c>TryParse</c>, or <see cref="RegonGenerator"/> methods.
    /// Accessing any domain property on a default instance throws <see cref="InvalidOperationException"/>.
    /// </summary>
    public bool IsDefault => !_isInitialized;

    private void ThrowIfDefault()
    {
        if (!_isInitialized)
            throw new InvalidOperationException(
                "Cannot access properties on a default Regon instance. Use Parse or TryParse.");
    }

    /// <summary>
    /// Gets the structural kind of this REGON: <see cref="RegonKind.Main"/> (9-digit) or
    /// <see cref="RegonKind.Local"/> (14-digit).
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessed on a default instance.</exception>
    public RegonKind Kind
    {
        get
        {
            ThrowIfDefault();
            return _isLocal ? RegonKind.Local : RegonKind.Main;
        }
    }

    /// <summary>
    /// Gets <see langword="true"/> when this is a 9-digit primary-entity REGON.
    /// Sugar for <c>Kind == RegonKind.Main</c>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessed on a default instance.</exception>
    public bool IsMain
    {
        get
        {
            ThrowIfDefault();
            return !_isLocal;
        }
    }

    /// <summary>
    /// Gets <see langword="true"/> when this is a 14-digit local-unit REGON.
    /// Sugar for <c>Kind == RegonKind.Local</c>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessed on a default instance.</exception>
    public bool IsLocal
    {
        get
        {
            ThrowIfDefault();
            return _isLocal;
        }
    }

    /// <summary>
    /// Returns the base REGON-9 for this identifier.
    /// For local (14-digit) REGON numbers, returns the parent entity REGON-9.
    /// For main (9-digit) REGON numbers, returns the current instance.
    /// </summary>
    /// <remarks>
    /// No re-validation is performed: REGON-14 undergoes two-step validation during construction,
    /// so the embedded REGON-9 base is guaranteed to be structurally valid.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when accessed on a default instance.</exception>
    public Regon BaseRegon
    {
        get
        {
            ThrowIfDefault();
            if (!_isLocal)
                return this;

            ulong baseValue = _value / 100_000UL; // strip the last 5 digits of the 14-digit value
            return new Regon(baseValue, false);
        }
    }

    // --- Standard overrides ---

    /// <summary>
    /// Returns the canonical string representation: 9 digits for REGON-9, 14 digits for REGON-14.
    /// Left-padded with zeros when necessary.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when called on a default instance.</exception>
    public override string ToString()
    {
        ThrowIfDefault();
        return _value.ToString(
            _isLocal ? "D14" : "D9",
            System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Returns the canonical string representation of the REGON number using the specified format.
    /// </summary>
    /// <param name="format">
    /// The format string. Accepted values are <see langword="null"/>, <c>""</c>, <c>"G"</c>, <c>"D9"</c> (REGON-9),
    /// and <c>"D14"</c> (REGON-14). Passing <see langword="null"/> or empty uses the natural format for the variant.
    /// </param>
    /// <param name="formatProvider">Ignored. REGON numbers are always formatted with invariant digits.</param>
    /// <returns>A decimal string left-padded with zeros to the appropriate length.</returns>
    /// <exception cref="InvalidOperationException">Thrown when called on a default instance.</exception>
    /// <exception cref="FormatException">Thrown when <paramref name="format"/> is not a supported value.</exception>
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        ThrowIfDefault();

        var natural = _isLocal ? "D14" : "D9";

        if (string.IsNullOrEmpty(format)
            || string.Equals(format, "G", StringComparison.OrdinalIgnoreCase)
            || string.Equals(format, natural, StringComparison.OrdinalIgnoreCase))
        {
            return _value.ToString(natural, System.Globalization.CultureInfo.InvariantCulture);
        }

        throw new FormatException($"Unsupported format string '{format}'.");
    }

    // --- Equality / Comparison ---

    /// <summary>
    /// Indicates whether this instance is equal to another <see cref="Regon"/> instance.
    /// Two instances are equal when they are both initialized, have the same <see cref="Kind"/>,
    /// and represent the same number.
    /// </summary>
    /// <param name="other">The instance to compare with.</param>
    /// <returns><see langword="true"/> if both instances represent the same REGON number; otherwise, <see langword="false"/>.</returns>
    public bool Equals(Regon other)
        => _isInitialized == other._isInitialized
           && _isLocal == other._isLocal
           && _value == other._value;

    /// <summary>
    /// Indicates whether this instance is equal to a specified object.
    /// </summary>
    public override bool Equals(object? obj) => obj is Regon other && Equals(other);

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    public override int GetHashCode()
        => unchecked((_value.GetHashCode() * 397) ^ (_isLocal.GetHashCode() * 31) ^ _isInitialized.GetHashCode());

    /// <summary>
    /// Compares this instance to another <see cref="Regon"/> by numeric value.
    /// Default instances sort before initialized instances.
    /// </summary>
    /// <param name="other">The instance to compare with.</param>
    /// <returns>
    /// A negative integer if this instance precedes <paramref name="other"/>,
    /// zero if they are equal, or a positive integer if this instance follows <paramref name="other"/>.
    /// </returns>
    public int CompareTo(Regon other)
    {
        if (_isInitialized != other._isInitialized)
            return _isInitialized ? 1 : -1;

        var valueComparison = _value.CompareTo(other._value);
        if (valueComparison != 0)
            return valueComparison;

        return _isLocal.CompareTo(other._isLocal);
    }

    /// <summary>
    /// Determines whether two <see cref="Regon"/> instances represent the same number.
    /// </summary>
    public static bool operator ==(Regon left, Regon right) => left.Equals(right);

    /// <summary>
    /// Determines whether two <see cref="Regon"/> instances represent different numbers.
    /// </summary>
    public static bool operator !=(Regon left, Regon right) => !left.Equals(right);
}
