using System.ComponentModel;
using PolishIdentifiers;
using Shouldly;

namespace PolishIdentifiers.Tests;

public class NipTypeConverterTests
{
    private const string ValidNip = "1234563218";
    private const string AllZeroNip = "0000000000";
    private const string ValidNipWithLeadingZero = "0123456789";
    private const string ValidHyphenated = "123-456-32-18";
    private const string ValidPlPrefix = "PL1234563218";
    private const string ValidPlSpacePrefix = "PL 1234563218";
    private const string ValidPlSpaceHyphenated = "PL 123-456-32-18";
    private const string InvalidCharactersNip = "123456321A";
    private const string TooShortNip = "123456321";
    private const string TooLongNip = "12345632180";
    private const string InvalidChecksumNip = "1234563217";
    private const string EmptyNip = "";
    private const string UnsupportedFormat1 = "PL-1234563218";
    private const string UnsupportedFormat2 = "123-456-3218";

    public static TheoryData<string> InvalidInputData => new()
    {
        InvalidCharactersNip,
        TooShortNip,
        TooLongNip,
        InvalidChecksumNip,
    };

    public static TheoryData<string, string> SupportedRepresentationData => new()
    {
        { ValidHyphenated, ValidNip },
        { ValidPlPrefix, ValidNip },
        { ValidPlSpacePrefix, ValidNip },
        { ValidPlSpaceHyphenated, ValidNip },
    };

    public static TheoryData<string> UnsupportedFormatData => new()
    {
        UnsupportedFormat1,
        UnsupportedFormat2,
    };

    // ── CanConvertFrom ────────────────────────────────────────────────────────

    [Fact]
    public void CanConvertFrom_String_ReturnsTrue()
    {
        var converter = new NipTypeConverter();

        converter.CanConvertFrom(typeof(string)).ShouldBeTrue();
    }

    [Fact]
    public void CanConvertFrom_Int_ReturnsFalse()
    {
        var converter = new NipTypeConverter();

        converter.CanConvertFrom(typeof(int)).ShouldBeFalse();
    }

    // ── CanConvertTo ──────────────────────────────────────────────────────────

    [Fact]
    public void CanConvertTo_String_ReturnsTrue()
    {
        var converter = new NipTypeConverter();

        converter.CanConvertTo(typeof(string)).ShouldBeTrue();
    }

    [Fact]
    public void CanConvertTo_Int_ReturnsFalse()
    {
        var converter = new NipTypeConverter();

        converter.CanConvertTo(typeof(int)).ShouldBeFalse();
    }

    // ── ConvertFrom ───────────────────────────────────────────────────────────

    [Fact]
    public void ConvertFrom_Null_ThrowsNotSupportedException()
    {
        var converter = new NipTypeConverter();

        Should.Throw<NotSupportedException>(() => converter.ConvertFrom(null!));
    }

    [Fact]
    public void ConvertFrom_ValidCanonical_ReturnsNip()
    {
        var converter = new NipTypeConverter();

        var result = converter.ConvertFrom(ValidNip);

        result.ShouldBeOfType<Nip>().ToString().ShouldBe(ValidNip);
    }

    [Fact]
    public void ConvertFrom_ValidNipWithLeadingZero_PreservesLeadingZero()
    {
        var converter = new NipTypeConverter();

        var result = converter.ConvertFrom(ValidNipWithLeadingZero);

        result.ShouldBeOfType<Nip>().ToString().ShouldBe(ValidNipWithLeadingZero);
    }

    [Fact]
    public void ConvertFrom_ValidAllZeroNip_ReturnsInitializedNip()
    {
        var converter = new NipTypeConverter();

        var result = converter.ConvertFrom(AllZeroNip);
        var nip = result.ShouldBeOfType<Nip>();

        nip.IsDefault.ShouldBeFalse();
        nip.ToString().ShouldBe(AllZeroNip);
    }

    [Theory]
    [MemberData(nameof(SupportedRepresentationData))]
    public void ConvertFrom_AllSupportedRepresentations_RoundTripToCanonical(string input, string expectedCanonical)
    {
        var converter = new NipTypeConverter();

        var parsed = converter.ConvertFrom(input);
        var canonical = (string)converter.ConvertTo(parsed, typeof(string))!;

        canonical.ShouldBe(expectedCanonical);
    }

    [Theory]
    [MemberData(nameof(InvalidInputData))]
    public void ConvertFrom_InvalidInput_ThrowsFormatExceptionWithDomainInnerException(string input)
    {
        var converter = new NipTypeConverter();

        var ex = Should.Throw<FormatException>(() => converter.ConvertFrom(input));

        ex.InnerException.ShouldBeOfType<NipValidationException>();
    }

    [Fact]
    public void ConvertFrom_EmptyString_ThrowsFormatException()
    {
        var converter = new NipTypeConverter();

        var ex = Should.Throw<FormatException>(() => converter.ConvertFrom(EmptyNip));

        ex.InnerException.ShouldBeOfType<NipValidationException>();
    }

    [Theory]
    [MemberData(nameof(UnsupportedFormatData))]
    public void ConvertFrom_UnsupportedFormattedLayout_ThrowsFormatException(string input)
    {
        var converter = new NipTypeConverter();

        var ex = Should.Throw<FormatException>(() => converter.ConvertFrom(input));

        ex.InnerException.ShouldBeOfType<NipValidationException>();
    }

    [Fact]
    public void ConvertFrom_BoxedLong_ThrowsNotSupportedException()
    {
        var converter = new NipTypeConverter();

        Should.Throw<NotSupportedException>(() => converter.ConvertFrom(1234563218L));
    }

    // ── ConvertTo ─────────────────────────────────────────────────────────────

    [Fact]
    public void ConvertTo_ValidNip_ReturnsCanonicalString()
    {
        var converter = new NipTypeConverter();
        var nip = Nip.Parse(ValidNip);

        var result = converter.ConvertTo(nip, typeof(string));

        result.ShouldBe(ValidNip);
    }

    [Fact]
    public void ConvertTo_ValidAllZeroNip_ReturnsCanonicalString()
    {
        var converter = new NipTypeConverter();
        var nip = Nip.Parse(AllZeroNip);

        var result = converter.ConvertTo(nip, typeof(string));

        result.ShouldBe(AllZeroNip);
    }

    [Fact]
    public void ConvertTo_DefaultNip_ThrowsInvalidOperationException()
    {
        var converter = new NipTypeConverter();

        Should.Throw<InvalidOperationException>(() => converter.ConvertTo(default(Nip), typeof(string)));
    }

    [Fact]
    public void ConvertTo_NullValue_ReturnsEmptyString()
    {
        var converter = new NipTypeConverter();

        var result = converter.ConvertTo(null, typeof(string));

        result.ShouldBe(string.Empty);
    }

    [Fact]
    public void ConvertTo_WrongDestinationType_ThrowsNotSupportedException()
    {
        var converter = new NipTypeConverter();
        var nip = Nip.Parse(ValidNip);

        Should.Throw<NotSupportedException>(() => converter.ConvertTo(nip, typeof(int)));
    }

    // ── Round-trips ───────────────────────────────────────────────────────────

    [Fact]
    public void RoundTrip_Canonical()
    {
        var converter = new NipTypeConverter();

        var parsed = converter.ConvertFrom(ValidNip);
        var canonical = (string)converter.ConvertTo(parsed, typeof(string))!;

        canonical.ShouldBe(ValidNip);
    }

    [Fact]
    public void RoundTrip_LeadingZero()
    {
        var converter = new NipTypeConverter();

        var parsed = converter.ConvertFrom(ValidNipWithLeadingZero);
        var canonical = (string)converter.ConvertTo(parsed, typeof(string))!;

        canonical.ShouldBe(ValidNipWithLeadingZero);
    }

    // ── TypeDescriptor integration ────────────────────────────────────────────

    [Fact]
    public void TypeDescriptor_GetConverter_ReturnsNipTypeConverter()
    {
        TypeDescriptor.GetConverter(typeof(Nip)).ShouldBeOfType<NipTypeConverter>();
    }

    [Fact]
    public void TypeDescriptor_GetConverter_ForNullableNip_ReturnsNullableConverter()
    {
        TypeDescriptor.GetConverter(typeof(Nip?)).ShouldBeOfType<NullableConverter>();
    }

    [Fact]
    public void NullableConverter_ConvertFrom_Null_ReturnsNull()
    {
        var converter = TypeDescriptor.GetConverter(typeof(Nip?));

        var result = converter.ConvertFrom(null!);

        result.ShouldBeNull();
    }

    [Fact]
    public void NullableConverter_ConvertFrom_ValidCanonical_ReturnsNip()
    {
        var converter = TypeDescriptor.GetConverter(typeof(Nip?));

        var result = converter.ConvertFrom(ValidNip);

        result.ShouldBeOfType<Nip>().ToString().ShouldBe(ValidNip);
    }

    [Fact]
    public void NullableConverter_ConvertFrom_InvalidInput_ThrowsFormatExceptionWithDomainInnerException()
    {
        var converter = TypeDescriptor.GetConverter(typeof(Nip?));

        var ex = Should.Throw<FormatException>(() => converter.ConvertFrom(InvalidChecksumNip));

        ex.InnerException.ShouldBeOfType<NipValidationException>();
    }
}
