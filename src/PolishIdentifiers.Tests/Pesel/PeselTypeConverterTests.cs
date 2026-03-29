using System.ComponentModel;
using PolishIdentifiers;
using Shouldly;

namespace PolishIdentifiers.Tests;

public class PeselTypeConverterTests
{
    private const string ValidPesel = "44051401458";
    private const string ValidPeselWithLeadingZero = "02070803628";
    private const string InvalidCharactersPesel = "4405140145A";
    private const string TooShortPesel = "4405140145";
    private const string TooLongPesel = "440514014580";
    private const string InvalidChecksumPesel = "44051401457";
    private const string EmptyPesel = "";

    public static TheoryData<string> InvalidInputData => new()
    {
        InvalidCharactersPesel,
        TooShortPesel,
        TooLongPesel,
        InvalidChecksumPesel,
    };

    // ── CanConvertFrom ────────────────────────────────────────────────────────

    [Fact]
    public void CanConvertFrom_String_ReturnsTrue()
    {
        var converter = new PeselTypeConverter();

        converter.CanConvertFrom(typeof(string)).ShouldBeTrue();
    }

    [Fact]
    public void CanConvertFrom_Int_ReturnsFalse()
    {
        var converter = new PeselTypeConverter();

        converter.CanConvertFrom(typeof(int)).ShouldBeFalse();
    }

    // ── CanConvertTo ──────────────────────────────────────────────────────────

    [Fact]
    public void CanConvertTo_String_ReturnsTrue()
    {
        var converter = new PeselTypeConverter();

        converter.CanConvertTo(typeof(string)).ShouldBeTrue();
    }

    [Fact]
    public void CanConvertTo_Int_ReturnsFalse()
    {
        var converter = new PeselTypeConverter();

        converter.CanConvertTo(typeof(int)).ShouldBeFalse();
    }

    // ── ConvertFrom ───────────────────────────────────────────────────────────

    [Fact]
    public void ConvertFrom_Null_ThrowsNotSupportedException()
    {
        var converter = new PeselTypeConverter();

        Should.Throw<NotSupportedException>(() => converter.ConvertFrom(null!));
    }

    [Fact]
    public void ConvertFrom_ValidCanonical_ReturnsPesel()
    {
        var converter = new PeselTypeConverter();

        var result = converter.ConvertFrom(ValidPesel);

        result.ShouldBeOfType<Pesel>().ToString().ShouldBe(ValidPesel);
    }

    [Fact]
    public void ConvertFrom_ValidPeselWithLeadingZero_PreservesLeadingZero()
    {
        var converter = new PeselTypeConverter();

        var result = converter.ConvertFrom(ValidPeselWithLeadingZero);

        result.ShouldBeOfType<Pesel>().ToString().ShouldBe(ValidPeselWithLeadingZero);
    }

    [Theory]
    [MemberData(nameof(InvalidInputData))]
    public void ConvertFrom_InvalidInput_ThrowsFormatExceptionWithDomainInnerException(string input)
    {
        var converter = new PeselTypeConverter();

        var ex = Should.Throw<FormatException>(() => converter.ConvertFrom(input));

        ex.InnerException.ShouldBeOfType<PeselValidationException>();
    }

    [Fact]
    public void ConvertFrom_EmptyString_ThrowsFormatException()
    {
        var converter = new PeselTypeConverter();

        var ex = Should.Throw<FormatException>(() => converter.ConvertFrom(EmptyPesel));

        ex.InnerException.ShouldBeOfType<PeselValidationException>();
    }

    [Fact]
    public void ConvertFrom_BoxedLong_ThrowsNotSupportedException()
    {
        var converter = new PeselTypeConverter();

        Should.Throw<NotSupportedException>(() => converter.ConvertFrom(44051401458L));
    }

    // ── ConvertTo ─────────────────────────────────────────────────────────────

    [Fact]
    public void ConvertTo_ValidPesel_ReturnsCanonicalString()
    {
        var converter = new PeselTypeConverter();
        var pesel = Pesel.Parse(ValidPesel);

        var result = converter.ConvertTo(pesel, typeof(string));

        result.ShouldBe(ValidPesel);
    }

    [Fact]
    public void ConvertTo_DefaultPesel_ThrowsInvalidOperationException()
    {
        var converter = new PeselTypeConverter();

        Should.Throw<InvalidOperationException>(() => converter.ConvertTo(default(Pesel), typeof(string)));
    }

    [Fact]
    public void ConvertTo_NullValue_ReturnsEmptyString()
    {
        var converter = new PeselTypeConverter();

        var result = converter.ConvertTo(null, typeof(string));

        result.ShouldBe(string.Empty);
    }

    [Fact]
    public void ConvertTo_WrongDestinationType_ThrowsNotSupportedException()
    {
        var converter = new PeselTypeConverter();
        var pesel = Pesel.Parse(ValidPesel);

        Should.Throw<NotSupportedException>(() => converter.ConvertTo(pesel, typeof(int)));
    }

    // ── Round-trips ───────────────────────────────────────────────────────────

    [Fact]
    public void RoundTrip_Canonical()
    {
        var converter = new PeselTypeConverter();

        var parsed = converter.ConvertFrom(ValidPesel);
        var canonical = (string)converter.ConvertTo(parsed, typeof(string))!;

        canonical.ShouldBe(ValidPesel);
    }

    [Fact]
    public void RoundTrip_LeadingZero()
    {
        var converter = new PeselTypeConverter();

        var parsed = converter.ConvertFrom(ValidPeselWithLeadingZero);
        var canonical = (string)converter.ConvertTo(parsed, typeof(string))!;

        canonical.ShouldBe(ValidPeselWithLeadingZero);
    }

    // ── TypeDescriptor integration ────────────────────────────────────────────

    [Fact]
    public void TypeDescriptor_GetConverter_ReturnsPeselTypeConverter()
    {
        TypeDescriptor.GetConverter(typeof(Pesel)).ShouldBeOfType<PeselTypeConverter>();
    }
}
