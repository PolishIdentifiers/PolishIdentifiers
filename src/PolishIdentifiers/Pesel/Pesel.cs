namespace PolishIdentifiers;

/// <summary>
/// Represents a validated Polish national identification number (PESEL).
/// </summary>
/// <remarks>
/// Instances can only be obtained through <see cref="Parse(string)"/>, <see cref="TryParse(string?, out Pesel)"/>,
/// or <see cref="PeselGenerator"/>. The default instance is not valid; accessing domain properties on it
/// throws <see cref="InvalidOperationException"/>. Use <see cref="IsDefault"/> to check before accessing.
/// </remarks>
#if NET10_0_OR_GREATER
public readonly struct Pesel
    : IEquatable<Pesel>, IComparable<Pesel>, IFormattable,
      IParsable<Pesel>, ISpanParsable<Pesel>
#else
public readonly struct Pesel : IEquatable<Pesel>, IComparable<Pesel>, IFormattable
#endif
{
    private readonly ulong _value;

    internal Pesel(ulong value) => _value = value;

    // --- Factories ---

    /// <summary>
    /// Parses the string representation of a PESEL number.
    /// </summary>
    /// <param name="value">An 11-digit string representing a PESEL number.</param>
    /// <returns>A valid <see cref="Pesel"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="PeselValidationException">
    /// Thrown when <paramref name="value"/> is not a valid PESEL number.
    /// The <see cref="PeselValidationException.Error"/> property indicates the first validation rule violated.
    /// </exception>
    public static Pesel Parse(string value)
    {
        if (value is null) throw new ArgumentNullException(nameof(value));
        return Parse(value.AsSpan());
    }

    /// <summary>
    /// Parses the span representation of a PESEL number.
    /// </summary>
    /// <param name="value">An 11-character span representing a PESEL number.</param>
    /// <returns>A valid <see cref="Pesel"/> instance.</returns>
    /// <exception cref="PeselValidationException">
    /// Thrown when <paramref name="value"/> is not a valid PESEL number.
    /// The <see cref="PeselValidationException.Error"/> property indicates the first validation rule violated.
    /// </exception>
    public static Pesel Parse(ReadOnlySpan<char> value)
    {
        var result = PeselValidator.Validate(value);
        if (!result.IsValid)
            throw new PeselValidationException(result.Error!.Value);
        return new Pesel(PeselValidator.SpanToUlong(value));
    }

    /// <summary>
    /// Attempts to parse the string representation of a PESEL number without throwing exceptions.
    /// </summary>
    /// <param name="value">An 11-digit string representing a PESEL number, or <see langword="null"/>.</param>
    /// <param name="pesel">
    /// When this method returns <see langword="true"/>, contains the parsed <see cref="Pesel"/>;
    /// otherwise, <see langword="default"/>.
    /// </param>
    /// <returns><see langword="true"/> if <paramref name="value"/> was successfully parsed; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(string? value, out Pesel pesel)
    {
        if (value is null) { pesel = default; return false; }
        return TryParse(value.AsSpan(), out pesel);
    }

    /// <summary>
    /// Attempts to parse the span representation of a PESEL number without throwing exceptions.
    /// </summary>
    /// <param name="value">An 11-character span representing a PESEL number.</param>
    /// <param name="pesel">
    /// When this method returns <see langword="true"/>, contains the parsed <see cref="Pesel"/>;
    /// otherwise, <see langword="default"/>.
    /// </param>
    /// <returns><see langword="true"/> if <paramref name="value"/> was successfully parsed; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(ReadOnlySpan<char> value, out Pesel pesel)
    {
        var result = PeselValidator.Validate(value);
        if (!result.IsValid) { pesel = default; return false; }
        pesel = new Pesel(PeselValidator.SpanToUlong(value));
        return true;
    }

    /// <summary>
    /// Validates a string against all PESEL rules without throwing exceptions or allocating a <see cref="Pesel"/> instance.
    /// </summary>
    /// <param name="value">The string to validate, or <see langword="null"/>.</param>
    /// <returns>
    /// A <see cref="ValidationResult{TError}"/> indicating whether the value is valid, and if not,
    /// the first <see cref="PeselValidationError"/> encountered. Validation order:
    /// characters → length → date → checksum.
    /// </returns>
    public static ValidationResult<PeselValidationError> Validate(string? value)
        => PeselValidator.Validate(value);

    /// <summary>
    /// Validates a character span against all PESEL rules without throwing exceptions or allocating a <see cref="Pesel"/> instance.
    /// </summary>
    /// <param name="value">The span to validate.</param>
    /// <returns>
    /// A <see cref="ValidationResult{TError}"/> indicating whether the value is valid, and if not,
    /// the first <see cref="PeselValidationError"/> encountered. Validation order:
    /// characters → length → date → checksum.
    /// </returns>
    public static ValidationResult<PeselValidationError> Validate(ReadOnlySpan<char> value)
        => PeselValidator.Validate(value);

#if NET10_0_OR_GREATER
    // --- IParsable<Pesel> / ISpanParsable<Pesel> ---

    static Pesel IParsable<Pesel>.Parse(string s, IFormatProvider? _)
        => Parse(s);

    static bool IParsable<Pesel>.TryParse(string? s, IFormatProvider? _, out Pesel result)
        => TryParse(s, out result);

    static Pesel ISpanParsable<Pesel>.Parse(ReadOnlySpan<char> s, IFormatProvider? _)
        => Parse(s);

    static bool ISpanParsable<Pesel>.TryParse(ReadOnlySpan<char> s, IFormatProvider? _, out Pesel result)
        => TryParse(s, out result);
#endif

    // --- Domain properties ---

    /// <summary>
    /// Gets a value indicating whether this instance was obtained via <c>default</c>
    /// rather than through one of the <c>Parse</c>, <c>TryParse</c>, or <see cref="PeselGenerator"/> methods.
    /// Accessing any domain property on a default instance throws <see cref="InvalidOperationException"/>.
    /// </summary>
    public bool IsDefault => _value == 0;

    private void ThrowIfDefault()
    {
        if (_value == 0)
            throw new InvalidOperationException(
                "Cannot access properties on a default Pesel instance. Use Parse or TryParse.");
    }

    /// <summary>
    /// Gets the date of birth encoded in the PESEL number.
    /// </summary>
    /// <remarks>
    /// PESEL encodes birth dates in the range 1800–2299 using a century offset applied to the month digits.
    /// Only the date component is meaningful; the time component is always midnight.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when accessed on a default instance.</exception>
    public DateTime BirthDateTime
    {
        get
        {
            ThrowIfDefault();
#if NET10_0_OR_GREATER
            Span<char> chars = stackalloc char[11];
            _value.TryFormat(chars, out _, "D11", System.Globalization.CultureInfo.InvariantCulture);
            return PeselParser.DecodeDate(chars);
#else
            return PeselParser.DecodeDate(ToString().AsSpan());
#endif
        }
    }

    /// <summary>
    /// Gets the gender encoded in the PESEL number.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessed on a default instance.</exception>
    public Gender Gender
    {
        get
        {
            ThrowIfDefault();
            return PeselParser.DecodeGender(_value);
        }
    }

#if NET10_0_OR_GREATER
    /// <summary>
    /// Gets the date of birth encoded in the PESEL number as a <see cref="DateOnly"/> value.
    /// </summary>
    /// <remarks>
    /// Equivalent to <see cref="BirthDateTime"/> without a time component.
    /// Available on .NET 10 and later only.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when accessed on a default instance.</exception>
    public DateOnly BirthDateOnly
    {
        get
        {
            var dt = BirthDateTime;
            return new DateOnly(dt.Year, dt.Month, dt.Day);
        }
    }
#endif

    // --- Standard overrides ---

    /// <summary>
    /// Returns the 11-digit string representation of the PESEL number.
    /// </summary>
    /// <returns>An 11-character decimal string, left-padded with zeros if necessary.</returns>
    public override string ToString() => _value.ToString("D11", System.Globalization.CultureInfo.InvariantCulture);

    /// <summary>
    /// Returns the 11-digit string representation of the PESEL number using the specified format.
    /// </summary>
    /// <param name="format">
    /// The format string. Accepted values are <see langword="null"/>, <c>""</c>, <c>"G"</c>, and <c>"D11"</c>.
    /// All produce the same 11-digit output.
    /// </param>
    /// <param name="formatProvider">Ignored. PESEL numbers are always formatted with invariant digits.</param>
    /// <returns>An 11-character decimal string, left-padded with zeros if necessary.</returns>
    /// <exception cref="FormatException">Thrown when <paramref name="format"/> is not a supported value.</exception>
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        if (string.IsNullOrEmpty(format)
            || string.Equals(format, "G", StringComparison.OrdinalIgnoreCase)
            || string.Equals(format, "D11", StringComparison.OrdinalIgnoreCase))
        {
            return _value.ToString("D11", System.Globalization.CultureInfo.InvariantCulture);
        }

        throw new FormatException($"Unsupported format string '{format}'.");
    }

    /// <summary>
    /// Indicates whether this instance is equal to another <see cref="Pesel"/> instance.
    /// </summary>
    /// <param name="other">The instance to compare with.</param>
    /// <returns><see langword="true"/> if both instances represent the same PESEL number; otherwise, <see langword="false"/>.</returns>
    public bool Equals(Pesel other) => _value == other._value;

    /// <summary>
    /// Indicates whether this instance is equal to a specified object.
    /// </summary>
    /// <param name="obj">The object to compare with this instance.</param>
    /// <returns>
    /// <see langword="true"/> when <paramref name="obj"/> is a <see cref="Pesel"/> with the same value;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object? obj) => obj is Pesel other && Equals(other);

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>A hash code based on the underlying PESEL value.</returns>
    public override int GetHashCode() => _value.GetHashCode();

    /// <summary>
    /// Compares this instance to another <see cref="Pesel"/> by numeric value.
    /// </summary>
    /// <param name="other">The instance to compare with.</param>
    /// <returns>
    /// A negative integer if this instance precedes <paramref name="other"/>,
    /// zero if they are equal, or a positive integer if this instance follows <paramref name="other"/>.
    /// </returns>
    public int CompareTo(Pesel other) => _value.CompareTo(other._value);

    /// <summary>
    /// Determines whether two <see cref="Pesel"/> instances represent the same number.
    /// </summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if both values are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(Pesel left, Pesel right) => left.Equals(right);

    /// <summary>
    /// Determines whether two <see cref="Pesel"/> instances represent different numbers.
    /// </summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if the values are different; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(Pesel left, Pesel right) => !left.Equals(right);
}
