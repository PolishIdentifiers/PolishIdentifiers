using System.Collections.Concurrent;
using System.Globalization;
using System.Threading.Tasks;
using PolishIdentifiers;
using Shouldly;

namespace PolishIdentifiers.Tests;

public class NipParsingTests
{
    private const string ValidNip = "1234563218";
    private const string AnotherValidNip = "7680002466";
    private const string ValidNipWithLeadingZero = "0123456789";
    private const string AllZeroNip = "0000000000";
    private const string InvalidChecksumNip = "1234563217";

    private const string TooShortNip = "123456321";
    private const string TooLongNip = "12345632180";
    private const string VeryLongNumericNip = "123456789012345678901234567890";
    private const string InvalidCharactersNip = "123456321A";
    private const string EmptyNip = "";
    private const string LeadingWhitespaceNip = " 1234563218";
    private const string TrailingWhitespaceNip = "1234563218 ";

    public static TheoryData<string, NipValidationError> InvalidInputData => new()
    {
        { TooShortNip, NipValidationError.InvalidLength },
        { TooLongNip, NipValidationError.InvalidLength },
        { InvalidCharactersNip, NipValidationError.InvalidCharacters },
    };

    public static TheoryData<string> InvalidInputStringsData => new()
    {
        TooShortNip,
        TooLongNip,
        InvalidCharactersNip,
    };

    public static TheoryData<string, NipValidationError> WhitespaceInputData => new()
    {
        { LeadingWhitespaceNip, NipValidationError.InvalidCharacters },
        { TrailingWhitespaceNip, NipValidationError.InvalidCharacters },
    };

    // --- Parse ---

    [Fact]
    public void Parse_ValidNip_ReturnsNipStruct()
    {
        var nip = Nip.Parse(ValidNip);

        nip.ToString().ShouldBe(ValidNip);
    }

    [Fact]
    public void Parse_NipWithLeadingZero_ReturnsCanonicalDigits()
    {
        var input = ValidNipWithLeadingZero;

        var nip = Nip.Parse(input);

        nip.ToString().ShouldBe(input);
    }

    [Fact]
    public void Parse_AllZeroNip_ReturnsInitializedNip()
    {
        var nip = Nip.Parse(AllZeroNip);

        nip.IsDefault.ShouldBeFalse();
    }

    [Fact]
    public void Parse_AllZeroNip_ToString_ReturnsCanonicalDigits()
    {
        var nip = Nip.Parse(AllZeroNip);

        nip.ToString().ShouldBe(AllZeroNip);
    }

    [Fact]
    public void Parse_InvalidNip_ThrowsNipValidationException()
    {
        var ex = Should.Throw<NipValidationException>(() => Nip.Parse(InvalidChecksumNip));

        ex.Error.ShouldBe(NipValidationError.InvalidChecksum);
    }

    [Theory]
    [MemberData(nameof(InvalidInputData))]
    public void Parse_InvalidInput_ThrowsNipValidationExceptionWithExpectedError(string input, NipValidationError expectedError)
    {
        var ex = Should.Throw<NipValidationException>(() => Nip.Parse(input));

        ex.Error.ShouldBe(expectedError);
    }

    [Fact]
    public void Parse_VeryLongNumericInput_ThrowsNipValidationExceptionWithInvalidLength()
    {
        var ex = Should.Throw<NipValidationException>(() => Nip.Parse(VeryLongNumericNip));

        ex.Error.ShouldBe(NipValidationError.InvalidLength);
    }

    [Fact]
    public void Parse_NullString_ThrowsArgumentNullException()
    {
        var ex = Should.Throw<ArgumentNullException>(() => Nip.Parse((string)null!));

        ex.ParamName.ShouldBe("value");
    }

    [Fact]
    public void Parse_EmptyString_ThrowsNipValidationExceptionWithInvalidLength()
    {
        var ex = Should.Throw<NipValidationException>(() => Nip.Parse(EmptyNip));

        ex.Error.ShouldBe(NipValidationError.InvalidLength);
    }

    [Theory]
    [MemberData(nameof(WhitespaceInputData))]
    public void Parse_WhitespaceInput_ThrowsNipValidationExceptionWithExpectedError(string input, NipValidationError expectedError)
    {
        var ex = Should.Throw<NipValidationException>(() => Nip.Parse(input));

        ex.Error.ShouldBe(expectedError);
    }

    // --- TryParse ---

    [Fact]
    public void TryParse_ValidNip_ReturnsTrue()
    {
        Nip.TryParse(ValidNip, out _).ShouldBeTrue();
    }

    [Fact]
    public void TryParse_ValidNip_SetsOutParam()
    {
        Nip.TryParse(ValidNip, out var nip);

        nip.ToString().ShouldBe(ValidNip);
    }

    [Fact]
    public void TryParse_AllZeroNip_SetsOutParamToInitializedValue()
    {
        Nip.TryParse(AllZeroNip, out var nip).ShouldBeTrue();

        nip.IsDefault.ShouldBeFalse();
    }

    [Fact]
    public void TryParse_InvalidChecksumNip_ReturnsFalse()
    {
        Nip.TryParse(InvalidChecksumNip, out _).ShouldBeFalse();
    }

    [Fact]
    public void TryParse_InvalidChecksumNip_SetsOutParamToDefault()
    {
        Nip.TryParse(InvalidChecksumNip, out var nip);

        nip.ShouldBe(default);
    }

    [Theory]
    [MemberData(nameof(InvalidInputStringsData))]
    public void TryParse_InvalidInput_ReturnsFalse(string input)
    {
        Nip.TryParse(input, out _).ShouldBeFalse();
    }

    [Theory]
    [MemberData(nameof(InvalidInputStringsData))]
    public void TryParse_InvalidInput_SetsOutParamToDefault(string input)
    {
        Nip.TryParse(input, out var nip);

        nip.ShouldBe(default);
    }

    [Fact]
    public void TryParse_VeryLongNumericInput_ReturnsFalse()
    {
        Nip.TryParse(VeryLongNumericNip, out _).ShouldBeFalse();
    }

    [Fact]
    public void TryParse_Null_ReturnsFalse()
    {
        Nip.TryParse(null, out _).ShouldBeFalse();
    }

    [Fact]
    public void TryParse_Null_SetsOutParamToDefault()
    {
        Nip.TryParse(null, out var nip);

        nip.ShouldBe(default);
    }

    // --- Span overloads ---

    [Fact]
    public void Parse_SpanOverload_ValidNip_ReturnsNip()
    {
        var nip = Nip.Parse(ValidNip.AsSpan());

        nip.ToString().ShouldBe(ValidNip);
    }

    [Fact]
    public void Parse_SpanOverload_InvalidNip_ThrowsNipValidationException()
    {
        var ex = Should.Throw<NipValidationException>(() => Nip.Parse(InvalidChecksumNip.AsSpan()));

        ex.Error.ShouldBe(NipValidationError.InvalidChecksum);
    }

    [Fact]
    public void TryParse_SpanOverload_ValidNip_ReturnsTrue()
    {
        Nip.TryParse(ValidNip.AsSpan(), out _).ShouldBeTrue();
    }

    [Fact]
    public void TryParse_SpanOverload_ValidNip_SetsOutParam()
    {
        Nip.TryParse(ValidNip.AsSpan(), out var nip);

        nip.ToString().ShouldBe(ValidNip);
    }

    [Fact]
    public void TryParse_SpanOverload_InvalidNip_ReturnsFalse()
    {
        Nip.TryParse(InvalidChecksumNip.AsSpan(), out _).ShouldBeFalse();
    }

    [Fact]
    public void TryParse_SpanOverload_EmptySpan_ReturnsFalse()
    {
        Nip.TryParse(ReadOnlySpan<char>.Empty, out _).ShouldBeFalse();
    }

    [Theory]
    [MemberData(nameof(InvalidInputStringsData))]
    public void TryParse_SpanOverload_InvalidInput_ReturnsFalse(string input)
    {
        Nip.TryParse(input.AsSpan(), out _).ShouldBeFalse();
    }

    [Theory]
    [MemberData(nameof(InvalidInputStringsData))]
    public void TryParse_SpanOverload_InvalidInput_SetsOutParamToDefault(string input)
    {
        Nip.TryParse(input.AsSpan(), out var nip);

        nip.ShouldBe(default);
    }

    [Theory]
    [MemberData(nameof(InvalidInputData))]
    public void Parse_SpanOverload_InvalidInput_ThrowsNipValidationExceptionWithExpectedError(string input, NipValidationError expectedError)
    {
        var ex = Should.Throw<NipValidationException>(() => Nip.Parse(input.AsSpan()));

        ex.Error.ShouldBe(expectedError);
    }

    [Fact]
    public void Parse_SpanOverload_EmptySpan_ThrowsWithInvalidLength()
    {
        var ex = Should.Throw<NipValidationException>(() => Nip.Parse(ReadOnlySpan<char>.Empty));

        ex.Error.ShouldBe(NipValidationError.InvalidLength);
    }

    // --- IssuingTaxOfficePrefix ---

    [Theory]
    [InlineData("1234563218", 123)]
    [InlineData("7680002466", 768)]
    [InlineData("0123456789", 12)]
    [InlineData("5270000001", 527)]
    public void IssuingTaxOfficePrefix_ReturnsFirstThreeDigitsAsInt(string input, int expectedPrefix)
    {
        var nip = Nip.Parse(input);

        nip.IssuingTaxOfficePrefix.ShouldBe(expectedPrefix);
    }

    [Fact]
    public void IssuingTaxOfficePrefix_AllZeroNip_ReturnsZero()
    {
        var nip = Nip.Parse(AllZeroNip);

        nip.IssuingTaxOfficePrefix.ShouldBe(0);
    }

    // --- Default struct ---

    [Fact]
    public void DefaultNip_IsDefault_ReturnsTrue()
    {
        var nip = default(Nip);

        nip.IsDefault.ShouldBeTrue();
    }

    [Fact]
    public void ParsedNip_IsDefault_ReturnsFalse()
    {
        var nip = Nip.Parse(ValidNip);

        nip.IsDefault.ShouldBeFalse();
    }

    [Fact]
    public void DefaultNip_IssuingTaxOfficePrefix_ThrowsInvalidOperationException()
    {
        var nip = default(Nip);

        Should.Throw<InvalidOperationException>(() => { var _ = nip.IssuingTaxOfficePrefix; });
    }

    [Fact]
    public void DefaultNip_ToString_ThrowsInvalidOperationException()
    {
        var nip = default(Nip);

        Should.Throw<InvalidOperationException>(() => nip.ToString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("G")]
    [InlineData("g")]
    [InlineData("D10")]
    [InlineData("d10")]
    public void DefaultNip_ToString_WithSupportedFormat_ThrowsInvalidOperationException(string? format)
    {
        var nip = default(Nip);

        Should.Throw<InvalidOperationException>(() => nip.ToString(format, null));
    }

    [Fact]
    public void DefaultNip_ToString_WithNipFormat_ThrowsInvalidOperationException()
    {
        var nip = default(Nip);

        Should.Throw<InvalidOperationException>(() => nip.ToString(NipFormat.Hyphenated));
    }

    [Fact]
    public void TryParse_InvalidNip_OutParam_IsDefault_ReturnsTrue()
    {
        Nip.TryParse(InvalidChecksumNip, out var nip);

        nip.IsDefault.ShouldBeTrue();
    }

    // --- Equality ---

    [Fact]
    public void Equals_SameNipValue_ReturnsTrue()
    {
        var a = Nip.Parse(ValidNip);
        var b = Nip.Parse(ValidNip);

        a.ShouldBe(b);
    }

    [Fact]
    public void EqualityOperator_SameNipValue_ReturnsTrue()
    {
        var a = Nip.Parse(ValidNip);
        var b = Nip.Parse(ValidNip);

        (a == b).ShouldBeTrue();
    }

    [Fact]
    public void InequalityOperator_SameNipValue_ReturnsFalse()
    {
        var a = Nip.Parse(ValidNip);
        var b = Nip.Parse(ValidNip);

        (a != b).ShouldBeFalse();
    }

    [Fact]
    public void Equals_DifferentNipValues_ReturnsFalse()
    {
        var a = Nip.Parse(ValidNip);
        var b = Nip.Parse(AnotherValidNip);

        a.ShouldNotBe(b);
    }

    [Fact]
    public void Equals_BoxedSameNip_ReturnsTrue()
    {
        var nip = Nip.Parse(ValidNip);

        nip.Equals((object)nip).ShouldBeTrue();
    }

    [Fact]
    public void Equals_BoxedNonNipObject_ReturnsFalse()
    {
        var nip = Nip.Parse(ValidNip);

        nip.Equals((object)"not a nip").ShouldBeFalse();
    }

    [Fact]
    public void Equals_NullObject_ReturnsFalse()
    {
        var nip = Nip.Parse(ValidNip);

        nip.Equals((object?)null).ShouldBeFalse();
    }

    [Fact]
    public void GetHashCode_EqualNips_HaveSameHashCode()
    {
        var a = Nip.Parse(ValidNip);
        var b = Nip.Parse(ValidNip);

        a.GetHashCode().ShouldBe(b.GetHashCode());
    }

    [Fact]
    public void EqualityOperator_TwoDefaults_ReturnsTrue()
    {
        var a = default(Nip);
        var b = default(Nip);

        (a == b).ShouldBeTrue();
    }

    [Fact]
    public void InequalityOperator_DefaultAndValidNip_ReturnsTrue()
    {
        var valid = Nip.Parse(ValidNip);
        var def = default(Nip);

        (valid != def).ShouldBeTrue();
    }

    [Fact]
    public void EqualityOperator_DefaultAndParsedAllZeroNip_ReturnsFalse()
    {
        var parsed = Nip.Parse(AllZeroNip);
        var def = default(Nip);

        (parsed == def).ShouldBeFalse();
    }

    [Fact]
    public void HashSet_EqualNips_CountsAsOneEntry()
    {
        var a = Nip.Parse(ValidNip);
        var b = Nip.Parse(ValidNip);

        var set = new HashSet<Nip> { a, b };

        set.Count.ShouldBe(1);
    }

    [Fact]
    public void HashSet_DifferentNips_CountsAsTwoEntries()
    {
        var a = Nip.Parse(ValidNip);
        var b = Nip.Parse(AnotherValidNip);

        var set = new HashSet<Nip> { a, b };

        set.Count.ShouldBe(2);
    }

    [Fact]
    public void Dictionary_NipAsKey_RetrievesValueCorrectly()
    {
        var nip = Nip.Parse(ValidNip);
        var dict = new Dictionary<Nip, string> { [nip] = "ACME Corp" };

        dict[Nip.Parse(ValidNip)].ShouldBe("ACME Corp");
    }

    // --- IComparable ---

    [Fact]
    public void CompareTo_SmallerNip_ReturnsNegative()
    {
        var smaller = Nip.Parse(ValidNipWithLeadingZero);
        var larger = Nip.Parse(AnotherValidNip);

        (smaller.CompareTo(larger) < 0).ShouldBeTrue();
    }

    [Fact]
    public void CompareTo_EqualNip_ReturnsZero()
    {
        var a = Nip.Parse(ValidNip);
        var b = Nip.Parse(ValidNip);

        a.CompareTo(b).ShouldBe(0);
    }

    [Fact]
    public void CompareTo_IsAntisymmetric()
    {
        var a = Nip.Parse(ValidNip);
        var b = Nip.Parse(AnotherValidNip);

        a.CompareTo(b).ShouldBe(-b.CompareTo(a));
    }

    [Fact]
    public void CompareTo_LargerNip_ReturnsPositive()
    {
        var smaller = Nip.Parse(ValidNipWithLeadingZero);
        var larger = Nip.Parse(AnotherValidNip);

        (larger.CompareTo(smaller) > 0).ShouldBeTrue();
    }

    [Fact]
    public void CompareTo_DefaultNip_ReturnsPositiveForValidNip()
    {
        var nip = Nip.Parse(ValidNip);
        var defaultNip = default(Nip);

        (nip.CompareTo(defaultNip) > 0).ShouldBeTrue();
    }

    [Fact]
    public void CompareTo_ValidNip_ReturnsNegativeForDefaultNip()
    {
        var nip = Nip.Parse(ValidNip);
        var defaultNip = default(Nip);

        (defaultNip.CompareTo(nip) < 0).ShouldBeTrue();
    }

    [Fact]
    public void CompareTo_DefaultNip_ReturnsPositiveForParsedAllZeroNip()
    {
        var nip = Nip.Parse(AllZeroNip);
        var defaultNip = default(Nip);

        nip.CompareTo(defaultNip).ShouldBeGreaterThan(0);
    }

    [Fact]
    public void CompareTo_TwoDefaults_ReturnsZero()
    {
        var a = default(Nip);
        var b = default(Nip);

        a.CompareTo(b).ShouldBe(0);
    }

    [Fact]
    public void CompareTo_IsTransitive()
    {
        var a = Nip.Parse(ValidNipWithLeadingZero); // 0123456789 — smallest
        var b = Nip.Parse(ValidNip);                // 1234563218 — middle
        var c = Nip.Parse(AnotherValidNip);         // 7680002466 — largest

        (a.CompareTo(b) < 0).ShouldBeTrue();
        (b.CompareTo(c) < 0).ShouldBeTrue();
        (a.CompareTo(c) < 0).ShouldBeTrue();
    }

    [Fact]
    public void Sort_ListOfNips_ProducesAscendingOrder()
    {
        var a = Nip.Parse(ValidNipWithLeadingZero); // 0123456789
        var b = Nip.Parse(ValidNip);                // 1234563218
        var c = Nip.Parse(AnotherValidNip);         // 7680002466

        var list = new List<Nip> { c, b, a };
        list.Sort();

        list.ShouldBe([a, b, c]);
    }

    // --- ToString ---

    [Fact]
    public void ToString_PreservesLeadingZeros()
    {
        var nip = Nip.Parse(ValidNipWithLeadingZero);

        nip.ToString().ShouldBe(ValidNipWithLeadingZero);
    }

    [Fact]
    public void Parse_ToString_Parse_RoundtripPreservesValue()
    {
        var parsed = Nip.Parse(ValidNipWithLeadingZero);
        var serialized = parsed.ToString();
        var reparsed = Nip.Parse(serialized);

        reparsed.ShouldBe(parsed);
    }

    // --- ToString(NipFormat) ---

    [Fact]
    public void ToString_DigitsOnly_ReturnsCanonical()
    {
        var nip = Nip.Parse(ValidNip);

        nip.ToString(NipFormat.DigitsOnly).ShouldBe("1234563218");
    }

    [Fact]
    public void ToString_Hyphenated_ReturnsHyphenatedFormat()
    {
        var nip = Nip.Parse(ValidNip);

        nip.ToString(NipFormat.Hyphenated).ShouldBe("123-456-32-18");
    }

    [Fact]
    public void ToString_VatEu_ReturnsVatEuFormat()
    {
        var nip = Nip.Parse(ValidNip);

        nip.ToString(NipFormat.VatEu).ShouldBe("PL1234563218");
    }

    [Fact]
    public void ToString_Hyphenated_WithLeadingZero_ReturnsCorrectFormat()
    {
        var nip = Nip.Parse(ValidNipWithLeadingZero);

        nip.ToString(NipFormat.Hyphenated).ShouldBe("012-345-67-89");
    }

    [Fact]
    public void ToString_VatEu_WithLeadingZero_ReturnsCorrectFormat()
    {
        var nip = Nip.Parse(ValidNipWithLeadingZero);

        nip.ToString(NipFormat.VatEu).ShouldBe("PL0123456789");
    }

    // --- IFormattable ---

    [Theory]
    [InlineData(null)]
    [InlineData("G")]
    [InlineData("g")]
    [InlineData("D10")]
    [InlineData("d10")]
    public void ToString_SupportedFormat_ReturnsCanonicalNip(string? format)
    {
        var nip = Nip.Parse(ValidNipWithLeadingZero);

        nip.ToString(format, null).ShouldBe(ValidNipWithLeadingZero);
    }

    [Fact]
    public void ToString_SupportedFormat_WithCulture_ReturnsCanonicalNip()
    {
        var nip = Nip.Parse(ValidNipWithLeadingZero);

        nip.ToString("D10", CultureInfo.GetCultureInfo("pl-PL")).ShouldBe(ValidNipWithLeadingZero);
    }

    [Theory]
    [InlineData("pl-PL")]
    [InlineData("en-US")]
    [InlineData("fr-FR")]
    public void ToString_SupportedFormat_WithMultipleCultures_ReturnsCanonicalNip(string cultureName)
    {
        var nip = Nip.Parse(ValidNipWithLeadingZero);

        nip.ToString("D10", CultureInfo.GetCultureInfo(cultureName)).ShouldBe(ValidNipWithLeadingZero);
    }

    [Theory]
    [InlineData("X")]
    [InlineData("F")]
    [InlineData("N")]
    [InlineData("D")]
    [InlineData("D11")]
    [InlineData("G10")]
    public void ToString_UnsupportedFormat_ThrowsFormatException(string format)
    {
        var nip = Nip.Parse(ValidNip);

        var ex = Should.Throw<FormatException>(() => nip.ToString(format, null));

        ex.Message.ShouldContain(format);
    }

    // --- NipValidationException ---

    [Fact]
    public void NipValidationException_Message_ContainsErrorName()
    {
        var ex = Should.Throw<NipValidationException>(() => Nip.Parse(InvalidChecksumNip));

        ex.Message.ShouldContain(nameof(NipValidationError.InvalidChecksum));
    }

    [Fact]
    public void NipValidationException_InnerException_IsNull()
    {
        var ex = Should.Throw<NipValidationException>(() => Nip.Parse(InvalidChecksumNip));

        ex.InnerException.ShouldBeNull();
    }

    // --- Validate API consistency ---

    [Fact]
    public void Validate_Null_ReturnsInvalidLength_UnlikeParseWhichThrowsArgumentNullException()
    {
        var result = Nip.Validate(null);

        result.IsValid.ShouldBeFalse();
        result.Error.ShouldBe(NipValidationError.InvalidLength);
    }

    [Fact]
    public void Validate_ValidNip_IsConsistentWithTryParse()
    {
        var canParse = Nip.TryParse(ValidNip, out _);
        var result = Nip.Validate(ValidNip);

        result.IsValid.ShouldBe(canParse);
    }

    [Fact]
    public void Validate_InvalidNip_IsConsistentWithTryParse()
    {
        var canParse = Nip.TryParse(InvalidChecksumNip, out _);
        var result = Nip.Validate(InvalidChecksumNip);

        result.IsValid.ShouldBe(canParse);
    }

    // --- Struct semantics ---

    [Fact]
    public void StructAssignment_ProducesEqualCopy()
    {
        var original = Nip.Parse(ValidNip);
        var copy = original;

        copy.ShouldBe(original);
    }

    // --- Thread safety ---

    [Fact]
    public void Parse_ParallelLoad_ProducesNoFailures()
    {
        var inputs = new[]
        {
            ValidNip,
            AnotherValidNip,
            ValidNipWithLeadingZero,
        };

        var failures = new ConcurrentBag<Exception>();

        Parallel.For(0, 500, i =>
        {
            try
            {
                var input = inputs[i % inputs.Length];
                var parsed = Nip.Parse(input);
                _ = parsed.ToString();
                _ = parsed.IssuingTaxOfficePrefix;
            }
            catch (Exception ex)
            {
                failures.Add(ex);
            }
        });

        failures.ShouldBeEmpty();
    }

    // --- net10 only ---

#if NET10_0_OR_GREATER
    [Fact]
    public void IParsable_Parse_ValidNip_ReturnsNip()
    {
        static T CallParse<T>(string s) where T : IParsable<T> => T.Parse(s, null);

        var nip = CallParse<Nip>(ValidNip);

        nip.ToString().ShouldBe(ValidNip);
    }

    [Fact]
    public void IParsable_TryParse_ValidNip_ReturnsTrue()
    {
        static bool CallTryParse<T>(string? s, out T result) where T : struct, IParsable<T>
            => T.TryParse(s, null, out result);

        CallTryParse<Nip>(ValidNip, out _).ShouldBeTrue();
    }

    [Fact]
    public void IParsable_Parse_InvalidNip_ThrowsNipValidationException()
    {
        static T CallParse<T>(string s) where T : IParsable<T> => T.Parse(s, null);

        Should.Throw<NipValidationException>(() => CallParse<Nip>(InvalidChecksumNip));
    }

    [Fact]
    public void IParsable_Parse_Null_ThrowsArgumentNullException()
    {
        static T CallParse<T>(string? s) where T : IParsable<T> => T.Parse(s!, null);

        Should.Throw<ArgumentNullException>(() => CallParse<Nip>(null));
    }

    [Fact]
    public void IParsable_Parse_WithProvider_ReturnsCanonicalNip()
    {
        static T CallParse<T>(string s, IFormatProvider? provider) where T : IParsable<T>
            => T.Parse(s, provider);

        var nip = CallParse<Nip>(ValidNipWithLeadingZero, CultureInfo.GetCultureInfo("pl-PL"));

        nip.ToString().ShouldBe(ValidNipWithLeadingZero);
    }

    [Fact]
    public void IParsable_TryParse_InvalidNip_ReturnsFalse()
    {
        static bool CallTryParse<T>(string? s, out T result) where T : struct, IParsable<T>
            => T.TryParse(s, null, out result);

        CallTryParse<Nip>(InvalidChecksumNip, out _).ShouldBeFalse();
    }

    [Fact]
    public void IParsable_TryParse_Null_ReturnsFalse()
    {
        static bool CallTryParse<T>(string? s, out T result) where T : struct, IParsable<T>
            => T.TryParse(s, null, out result);

        CallTryParse<Nip>(null, out _).ShouldBeFalse();
    }

    [Fact]
    public void ISpanParsable_Parse_ValidNip_ReturnsNip()
    {
        static T CallParse<T>(ReadOnlySpan<char> s) where T : ISpanParsable<T> => T.Parse(s, null);

        var nip = CallParse<Nip>(ValidNip.AsSpan());

        nip.ToString().ShouldBe(ValidNip);
    }

    [Fact]
    public void ISpanParsable_TryParse_ValidNip_ReturnsTrue()
    {
        static bool CallTryParse<T>(ReadOnlySpan<char> s, out T result) where T : struct, ISpanParsable<T>
            => T.TryParse(s, null, out result);

        CallTryParse<Nip>(ValidNip.AsSpan(), out _).ShouldBeTrue();
    }

    [Fact]
    public void ISpanParsable_Parse_InvalidNip_ThrowsNipValidationException()
    {
        static T CallParse<T>(ReadOnlySpan<char> s) where T : ISpanParsable<T> => T.Parse(s, null);

        Should.Throw<NipValidationException>(() => CallParse<Nip>(InvalidChecksumNip.AsSpan()));
    }

    [Fact]
    public void ISpanParsable_TryParse_InvalidNip_ReturnsFalse()
    {
        static bool CallTryParse<T>(ReadOnlySpan<char> s, out T result) where T : struct, ISpanParsable<T>
            => T.TryParse(s, null, out result);

        CallTryParse<Nip>(InvalidChecksumNip.AsSpan(), out _).ShouldBeFalse();
    }

    [Fact]
    public void ISpanParsable_Parse_EmptySpan_ThrowsNipValidationExceptionWithInvalidLength()
    {
        static T CallParse<T>(ReadOnlySpan<char> s) where T : ISpanParsable<T> => T.Parse(s, null);

        var ex = Should.Throw<NipValidationException>(() => CallParse<Nip>(ReadOnlySpan<char>.Empty));

        ex.Error.ShouldBe(NipValidationError.InvalidLength);
    }
#endif
}
