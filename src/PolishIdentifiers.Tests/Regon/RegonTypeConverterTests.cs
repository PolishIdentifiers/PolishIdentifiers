using System.ComponentModel;
using PolishIdentifiers;
using Shouldly;

namespace PolishIdentifiers.Tests;

public class RegonTypeConverterTests
{
    private const string ValidRegon9 = "123456785";
    private const string ValidRegon9AllZeros = "000000000";
    private const string ValidRegon9WithChecksumR10 = "000000030";
    private const string ValidRegon9WithLeadingZero = "012345675";
    private const string ValidRegon14 = "12345678512347";
    private const string ValidRegon14AllZeros = "00000000000000";
    private const string ValidRegon14WithChecksumR10 = "12345678500010";
    private const string ValidRegon14WithLeadingZero = "01234567512342";
    private const string InvalidCharactersRegon = "12345678X";
    private const string TooShortRegon = "1234567";
    private const string InvalidLengthRegon10 = "1234567890";
    private const string InvalidLengthRegon13 = "1234567890123";
    private const string TooLongRegon = "123456785123470";
    private const string InvalidChecksumRegon9 = "123456780";
    private const string EmptyRegon = "";

    public static TheoryData<string> InvalidInputData => new()
    {
        InvalidCharactersRegon,
        TooShortRegon,
        InvalidLengthRegon10,
        InvalidLengthRegon13,
        TooLongRegon,
        InvalidChecksumRegon9,
    };

    // ── CanConvertFrom ────────────────────────────────────────────────────────

    [Fact]
    public void CanConvertFrom_String_ReturnsTrue()
    {
        var converter = new RegonTypeConverter();

        converter.CanConvertFrom(typeof(string)).ShouldBeTrue();
    }

    [Fact]
    public void CanConvertFrom_Int_ReturnsFalse()
    {
        var converter = new RegonTypeConverter();

        converter.CanConvertFrom(typeof(int)).ShouldBeFalse();
    }

    // ── CanConvertTo ──────────────────────────────────────────────────────────

    [Fact]
    public void CanConvertTo_String_ReturnsTrue()
    {
        var converter = new RegonTypeConverter();

        converter.CanConvertTo(typeof(string)).ShouldBeTrue();
    }

    [Fact]
    public void CanConvertTo_Int_ReturnsFalse()
    {
        var converter = new RegonTypeConverter();

        converter.CanConvertTo(typeof(int)).ShouldBeFalse();
    }

    // ── ConvertFrom ───────────────────────────────────────────────────────────

    [Fact]
    public void ConvertFrom_Null_ThrowsNotSupportedException()
    {
        var converter = new RegonTypeConverter();

        Should.Throw<NotSupportedException>(() => converter.ConvertFrom(null!));
    }

    [Fact]
    public void ConvertFrom_ValidRegon9_ReturnsRegon()
    {
        var converter = new RegonTypeConverter();

        var result = converter.ConvertFrom(ValidRegon9);

        result.ShouldBeOfType<Regon>().ToString().ShouldBe(ValidRegon9);
    }

    [Fact]
    public void ConvertFrom_ValidRegon14_ReturnsRegon()
    {
        var converter = new RegonTypeConverter();

        var result = converter.ConvertFrom(ValidRegon14);

        result.ShouldBeOfType<Regon>().ToString().ShouldBe(ValidRegon14);
    }

    [Fact]
    public void ConvertFrom_ValidRegon9AllZeros_ReturnsInitializedRegon()
    {
        var converter = new RegonTypeConverter();

        var result = converter.ConvertFrom(ValidRegon9AllZeros);
        var regon = result.ShouldBeOfType<Regon>();

        regon.IsDefault.ShouldBeFalse();
        regon.ToString().ShouldBe(ValidRegon9AllZeros);
    }

    [Fact]
    public void ConvertFrom_ValidRegon14AllZeros_ReturnsInitializedRegon()
    {
        var converter = new RegonTypeConverter();

        var result = converter.ConvertFrom(ValidRegon14AllZeros);
        var regon = result.ShouldBeOfType<Regon>();

        regon.IsDefault.ShouldBeFalse();
        regon.ToString().ShouldBe(ValidRegon14AllZeros);
    }

    [Fact]
    public void ConvertFrom_ValidRegon9WithLeadingZero_PreservesLeadingZero()
    {
        var converter = new RegonTypeConverter();

        var result = converter.ConvertFrom(ValidRegon9WithLeadingZero);

        result.ShouldBeOfType<Regon>().ToString().ShouldBe(ValidRegon9WithLeadingZero);
    }

    [Fact]
    public void ConvertFrom_ValidRegon14WithLeadingZero_PreservesLeadingZero()
    {
        var converter = new RegonTypeConverter();

        var result = converter.ConvertFrom(ValidRegon14WithLeadingZero);

        result.ShouldBeOfType<Regon>().ToString().ShouldBe(ValidRegon14WithLeadingZero);
    }

    [Theory]
    [MemberData(nameof(InvalidInputData))]
    public void ConvertFrom_InvalidInput_ThrowsFormatExceptionWithDomainInnerException(string input)
    {
        var converter = new RegonTypeConverter();

        var ex = Should.Throw<FormatException>(() => converter.ConvertFrom(input));

        ex.InnerException.ShouldBeOfType<RegonValidationException>();
    }

    [Fact]
    public void ConvertFrom_EmptyString_ThrowsFormatException()
    {
        var converter = new RegonTypeConverter();

        var ex = Should.Throw<FormatException>(() => converter.ConvertFrom(EmptyRegon));

        ex.InnerException.ShouldBeOfType<RegonValidationException>();
    }

    [Fact]
    public void ConvertFrom_BoxedLong_ThrowsNotSupportedException()
    {
        var converter = new RegonTypeConverter();

        Should.Throw<NotSupportedException>(() => converter.ConvertFrom(123456785L));
    }

    // ── ConvertTo ─────────────────────────────────────────────────────────────

    [Fact]
    public void ConvertTo_ValidRegon9_Returns9DigitString()
    {
        var converter = new RegonTypeConverter();
        var regon = Regon.Parse(ValidRegon9);

        var result = (string)converter.ConvertTo(regon, typeof(string))!;

        result.ShouldBe(ValidRegon9);
        result.Length.ShouldBe(9);
    }

    [Fact]
    public void ConvertTo_ValidRegon14_Returns14DigitString()
    {
        var converter = new RegonTypeConverter();
        var regon = Regon.Parse(ValidRegon14);

        var result = (string)converter.ConvertTo(regon, typeof(string))!;

        result.ShouldBe(ValidRegon14);
        result.Length.ShouldBe(14);
    }

    [Fact]
    public void ConvertTo_ValidRegon9AllZeros_Returns9DigitString()
    {
        var converter = new RegonTypeConverter();
        var regon = Regon.Parse(ValidRegon9AllZeros);

        var result = (string)converter.ConvertTo(regon, typeof(string))!;

        result.ShouldBe(ValidRegon9AllZeros);
    }

    [Fact]
    public void ConvertTo_ValidRegon14AllZeros_Returns14DigitString()
    {
        var converter = new RegonTypeConverter();
        var regon = Regon.Parse(ValidRegon14AllZeros);

        var result = (string)converter.ConvertTo(regon, typeof(string))!;

        result.ShouldBe(ValidRegon14AllZeros);
    }

    [Fact]
    public void ConvertTo_DefaultRegon_ThrowsInvalidOperationException()
    {
        var converter = new RegonTypeConverter();

        Should.Throw<InvalidOperationException>(() => converter.ConvertTo(default(Regon), typeof(string)));
    }

    [Fact]
    public void ConvertTo_NullValue_ReturnsEmptyString()
    {
        var converter = new RegonTypeConverter();

        var result = converter.ConvertTo(null, typeof(string));

        result.ShouldBe(string.Empty);
    }

    [Fact]
    public void ConvertTo_WrongDestinationType_ThrowsNotSupportedException()
    {
        var converter = new RegonTypeConverter();
        var regon = Regon.Parse(ValidRegon9);

        Should.Throw<NotSupportedException>(() => converter.ConvertTo(regon, typeof(int)));
    }

    // ── Round-trips ───────────────────────────────────────────────────────────

    [Fact]
    public void RoundTrip_Regon9_Canonical()
    {
        var converter = new RegonTypeConverter();

        var parsed = converter.ConvertFrom(ValidRegon9);
        var canonical = (string)converter.ConvertTo(parsed, typeof(string))!;

        canonical.ShouldBe(ValidRegon9);
    }

    [Fact]
    public void RoundTrip_Regon14_Canonical()
    {
        var converter = new RegonTypeConverter();

        var parsed = converter.ConvertFrom(ValidRegon14);
        var canonical = (string)converter.ConvertTo(parsed, typeof(string))!;

        canonical.ShouldBe(ValidRegon14);
    }

    [Fact]
    public void RoundTrip_Regon9_ChecksumR10()
    {
        var converter = new RegonTypeConverter();

        var parsed = converter.ConvertFrom(ValidRegon9WithChecksumR10);
        var canonical = (string)converter.ConvertTo(parsed, typeof(string))!;

        canonical.ShouldBe(ValidRegon9WithChecksumR10);
    }

    [Fact]
    public void RoundTrip_Regon14_ChecksumR10()
    {
        var converter = new RegonTypeConverter();

        var parsed = converter.ConvertFrom(ValidRegon14WithChecksumR10);
        var canonical = (string)converter.ConvertTo(parsed, typeof(string))!;

        canonical.ShouldBe(ValidRegon14WithChecksumR10);
    }

    [Fact]
    public void RoundTrip_Regon9_LeadingZero()
    {
        var converter = new RegonTypeConverter();

        var parsed = converter.ConvertFrom(ValidRegon9WithLeadingZero);
        var canonical = (string)converter.ConvertTo(parsed, typeof(string))!;

        canonical.ShouldBe(ValidRegon9WithLeadingZero);
    }

    [Fact]
    public void RoundTrip_Regon14_LeadingZero()
    {
        var converter = new RegonTypeConverter();

        var parsed = converter.ConvertFrom(ValidRegon14WithLeadingZero);
        var canonical = (string)converter.ConvertTo(parsed, typeof(string))!;

        canonical.ShouldBe(ValidRegon14WithLeadingZero);
    }

    // ── TypeDescriptor integration ────────────────────────────────────────────

    [Fact]
    public void TypeDescriptor_GetConverter_ReturnsRegonTypeConverter()
    {
        TypeDescriptor.GetConverter(typeof(Regon)).ShouldBeOfType<RegonTypeConverter>();
    }

    [Fact]
    public void TypeDescriptor_GetConverter_ForNullableRegon_ReturnsNullableConverter()
    {
        TypeDescriptor.GetConverter(typeof(Regon?)).ShouldBeOfType<NullableConverter>();
    }

    [Fact]
    public void NullableConverter_ConvertFrom_Null_ReturnsNull()
    {
        var converter = TypeDescriptor.GetConverter(typeof(Regon?));

        var result = converter.ConvertFrom(null!);

        result.ShouldBeNull();
    }

    [Fact]
    public void NullableConverter_ConvertFrom_ValidRegon9_ReturnsRegon()
    {
        var converter = TypeDescriptor.GetConverter(typeof(Regon?));

        var result = converter.ConvertFrom(ValidRegon9);

        result.ShouldBeOfType<Regon>().ToString().ShouldBe(ValidRegon9);
    }

    [Fact]
    public void NullableConverter_ConvertFrom_InvalidInput_ThrowsFormatExceptionWithDomainInnerException()
    {
        var converter = TypeDescriptor.GetConverter(typeof(Regon?));

        var ex = Should.Throw<FormatException>(() => converter.ConvertFrom(InvalidChecksumRegon9));

        ex.InnerException.ShouldBeOfType<RegonValidationException>();
    }
}
