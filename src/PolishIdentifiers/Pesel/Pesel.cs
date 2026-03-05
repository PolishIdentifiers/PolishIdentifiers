namespace PolishIdentifiers;

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

    public static Pesel Parse(string value)
    {
        if (value is null) throw new ArgumentNullException(nameof(value));
        return Parse(value.AsSpan());
    }

    public static Pesel Parse(ReadOnlySpan<char> value)
    {
        var result = PeselValidator.Validate(value);
        if (!result.IsValid)
            throw new PeselValidationException(result.Error!.Value);
        return new Pesel(PeselValidator.SpanToUlong(value));
    }

    public static bool TryParse(string? value, out Pesel pesel)
    {
        if (value is null) { pesel = default; return false; }
        return TryParse(value.AsSpan(), out pesel);
    }

    public static bool TryParse(ReadOnlySpan<char> value, out Pesel pesel)
    {
        var result = PeselValidator.Validate(value);
        if (!result.IsValid) { pesel = default; return false; }
        pesel = new Pesel(PeselValidator.SpanToUlong(value));
        return true;
    }

    public static ValidationResult<PeselValidationError> Validate(string? value)
        => PeselValidator.Validate(value);

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
    /// Returns <see langword="true"/> when this instance was created by <c>default</c>
    /// rather than via <see cref="Parse(string)"/> or <see cref="TryParse(string?, out Pesel)"/>.
    /// Accessing any other property on a default instance throws <see cref="InvalidOperationException"/>.
    /// </summary>
    public bool IsDefault => _value == 0;

    private void ThrowIfDefault()
    {
        if (_value == 0)
            throw new InvalidOperationException(
                "Cannot access properties on a default Pesel instance. Use Parse or TryParse.");
    }

    public DateTime BirthDateTime
    {
        get
        {
            ThrowIfDefault();
#if NET10_0_OR_GREATER
            Span<char> chars = stackalloc char[11];
            _value.TryFormat(chars, out _, "D11");
            return PeselParser.DecodeDate(chars);
#else
            return PeselParser.DecodeDate(ToString().AsSpan());
#endif
        }
    }

    public Gender Gender
    {
        get
        {
            ThrowIfDefault();
            return PeselParser.DecodeGender(_value);
        }
    }

#if NET10_0_OR_GREATER
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

    public override string ToString() => _value.ToString("D11");

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        if (string.IsNullOrEmpty(format)
            || string.Equals(format, "G", StringComparison.OrdinalIgnoreCase)
            || string.Equals(format, "D11", StringComparison.OrdinalIgnoreCase))
        {
            return _value.ToString("D11", formatProvider);
        }

        throw new FormatException($"Unsupported format string '{format}'.");
    }

    public bool Equals(Pesel other) => _value == other._value;

    public override bool Equals(object? obj) => obj is Pesel other && Equals(other);

    public override int GetHashCode() => _value.GetHashCode();

    public int CompareTo(Pesel other) => _value.CompareTo(other._value);

    public static bool operator ==(Pesel left, Pesel right) => left.Equals(right);
    public static bool operator !=(Pesel left, Pesel right) => !left.Equals(right);
}
