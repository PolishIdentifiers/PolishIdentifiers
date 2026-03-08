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
    private const string InvalidChecksumNip = "1234563217";

    private const string TooShortNip = "123456321";
    private const string TooLongNip = "12345632180";
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

        Assert.Equal(ValidNip, nip.ToString());
    }

    [Fact]
    public void Parse_InvalidNip_ThrowsNipValidationException()
    {
        var ex = Assert.Throws<NipValidationException>(() => Nip.Parse(InvalidChecksumNip));

        Assert.Equal(NipValidationError.InvalidChecksum, ex.Error);
    }

    [Theory]
    [MemberData(nameof(InvalidInputData))]
    public void Parse_InvalidInput_ThrowsNipValidationExceptionWithExpectedError(string input, NipValidationError expectedError)
    {
        var ex = Assert.Throws<NipValidationException>(() => Nip.Parse(input));

        Assert.Equal(expectedError, ex.Error);
    }

    [Fact]
    public void Parse_NullString_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => Nip.Parse((string)null!));

        Assert.Equal("value", ex.ParamName);
    }

    [Fact]
    public void Parse_EmptyString_ThrowsNipValidationExceptionWithInvalidLength()
    {
        var ex = Assert.Throws<NipValidationException>(() => Nip.Parse(EmptyNip));

        Assert.Equal(NipValidationError.InvalidLength, ex.Error);
    }

    [Theory]
    [MemberData(nameof(WhitespaceInputData))]
    public void Parse_WhitespaceInput_ThrowsNipValidationExceptionWithExpectedError(string input, NipValidationError expectedError)
    {
        var ex = Assert.Throws<NipValidationException>(() => Nip.Parse(input));

        Assert.Equal(expectedError, ex.Error);
    }

    // --- TryParse ---

    [Fact]
    public void TryParse_ValidNip_ReturnsTrue()
    {
        Assert.True(Nip.TryParse(ValidNip, out _));
    }

    [Fact]
    public void TryParse_ValidNip_SetsOutParam()
    {
        Nip.TryParse(ValidNip, out var nip);

        Assert.Equal(ValidNip, nip.ToString());
    }

    [Fact]
    public void TryParse_InvalidChecksumNip_ReturnsFalse()
    {
        Assert.False(Nip.TryParse(InvalidChecksumNip, out _));
    }

    [Fact]
    public void TryParse_InvalidChecksumNip_SetsOutParamToDefault()
    {
        Nip.TryParse(InvalidChecksumNip, out var nip);

        Assert.Equal(default, nip);
    }

    [Theory]
    [MemberData(nameof(InvalidInputStringsData))]
    public void TryParse_InvalidInput_ReturnsFalse(string input)
    {
        Assert.False(Nip.TryParse(input, out _));
    }

    [Fact]
    public void TryParse_Null_ReturnsFalse()
    {
        Assert.False(Nip.TryParse(null, out _));
    }

    [Fact]
    public void TryParse_Null_SetsOutParamToDefault()
    {
        Nip.TryParse(null, out var nip);

        Assert.Equal(default, nip);
    }

    // --- Span overloads ---

    [Fact]
    public void Parse_SpanOverload_ValidNip_ReturnsNip()
    {
        var nip = Nip.Parse(ValidNip.AsSpan());

        Assert.Equal(ValidNip, nip.ToString());
    }

    [Fact]
    public void Parse_SpanOverload_InvalidNip_ThrowsNipValidationException()
    {
        var ex = Assert.Throws<NipValidationException>(() => Nip.Parse(InvalidChecksumNip.AsSpan()));

        Assert.Equal(NipValidationError.InvalidChecksum, ex.Error);
    }

    [Fact]
    public void TryParse_SpanOverload_ValidNip_ReturnsTrue()
    {
        Assert.True(Nip.TryParse(ValidNip.AsSpan(), out _));
    }

    [Fact]
    public void TryParse_SpanOverload_ValidNip_SetsOutParam()
    {
        Nip.TryParse(ValidNip.AsSpan(), out var nip);

        Assert.Equal(ValidNip, nip.ToString());
    }

    [Fact]
    public void TryParse_SpanOverload_InvalidNip_ReturnsFalse()
    {
        Assert.False(Nip.TryParse(InvalidChecksumNip.AsSpan(), out _));
    }

    [Fact]
    public void TryParse_SpanOverload_EmptySpan_ReturnsFalse()
    {
        Assert.False(Nip.TryParse(ReadOnlySpan<char>.Empty, out _));
    }

    [Fact]
    public void Parse_SpanOverload_EmptySpan_ThrowsWithInvalidLength()
    {
        var ex = Assert.Throws<NipValidationException>(() => Nip.Parse(ReadOnlySpan<char>.Empty));

        Assert.Equal(NipValidationError.InvalidLength, ex.Error);
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

    // --- Default struct ---

    [Fact]
    public void DefaultNip_IsDefault_ReturnsTrue()
    {
        var nip = default(Nip);

        Assert.True(nip.IsDefault);
    }

    [Fact]
    public void ParsedNip_IsDefault_ReturnsFalse()
    {
        var nip = Nip.Parse(ValidNip);

        Assert.False(nip.IsDefault);
    }

    [Fact]
    public void DefaultNip_IssuingTaxOfficePrefix_ThrowsInvalidOperationException()
    {
        var nip = default(Nip);

        Should.Throw<InvalidOperationException>(() => { var _ = nip.IssuingTaxOfficePrefix; });
    }

    [Fact]
    public void DefaultNip_ToString_ReturnsZeroPaddedCanonicalString()
    {
        var nip = default(Nip);

        Assert.Equal("0000000000", nip.ToString());
    }

    [Fact]
    public void TryParse_InvalidNip_OutParam_IsDefault_ReturnsTrue()
    {
        Nip.TryParse(InvalidChecksumNip, out var nip);

        Assert.True(nip.IsDefault);
    }

    // --- Equality ---

    [Fact]
    public void Equals_SameNipValue_ReturnsTrue()
    {
        var a = Nip.Parse(ValidNip);
        var b = Nip.Parse(ValidNip);

        Assert.Equal(a, b);
    }

    [Fact]
    public void EqualityOperator_SameNipValue_ReturnsTrue()
    {
        var a = Nip.Parse(ValidNip);
        var b = Nip.Parse(ValidNip);

        Assert.True(a == b);
    }

    [Fact]
    public void InequalityOperator_SameNipValue_ReturnsFalse()
    {
        var a = Nip.Parse(ValidNip);
        var b = Nip.Parse(ValidNip);

        Assert.False(a != b);
    }

    [Fact]
    public void Equals_DifferentNipValues_ReturnsFalse()
    {
        var a = Nip.Parse(ValidNip);
        var b = Nip.Parse(AnotherValidNip);

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Equals_BoxedSameNip_ReturnsTrue()
    {
        var nip = Nip.Parse(ValidNip);

        Assert.True(nip.Equals((object)nip));
    }

    [Fact]
    public void Equals_BoxedNonNipObject_ReturnsFalse()
    {
        var nip = Nip.Parse(ValidNip);

        Assert.False(nip.Equals((object)"not a nip"));
    }

    [Fact]
    public void Equals_NullObject_ReturnsFalse()
    {
        var nip = Nip.Parse(ValidNip);

        Assert.False(nip.Equals((object?)null));
    }

    [Fact]
    public void GetHashCode_EqualNips_HaveSameHashCode()
    {
        var a = Nip.Parse(ValidNip);
        var b = Nip.Parse(ValidNip);

        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void HashSet_EqualNips_CountsAsOneEntry()
    {
        var a = Nip.Parse(ValidNip);
        var b = Nip.Parse(ValidNip);

        var set = new HashSet<Nip> { a, b };

        Assert.Single(set);
    }

    [Fact]
    public void HashSet_DifferentNips_CountsAsTwoEntries()
    {
        var a = Nip.Parse(ValidNip);
        var b = Nip.Parse(AnotherValidNip);

        var set = new HashSet<Nip> { a, b };

        Assert.Equal(2, set.Count);
    }

    [Fact]
    public void Dictionary_NipAsKey_RetrievesValueCorrectly()
    {
        var nip = Nip.Parse(ValidNip);
        var dict = new Dictionary<Nip, string> { [nip] = "ACME Corp" };

        Assert.Equal("ACME Corp", dict[Nip.Parse(ValidNip)]);
    }

    // --- IComparable ---

    [Fact]
    public void CompareTo_SmallerNip_ReturnsNegative()
    {
        var smaller = Nip.Parse(ValidNipWithLeadingZero);
        var larger = Nip.Parse(AnotherValidNip);

        Assert.True(smaller.CompareTo(larger) < 0);
    }

    [Fact]
    public void CompareTo_EqualNip_ReturnsZero()
    {
        var a = Nip.Parse(ValidNip);
        var b = Nip.Parse(ValidNip);

        Assert.Equal(0, a.CompareTo(b));
    }

    [Fact]
    public void CompareTo_LargerNip_ReturnsPositive()
    {
        var smaller = Nip.Parse(ValidNipWithLeadingZero);
        var larger = Nip.Parse(AnotherValidNip);

        Assert.True(larger.CompareTo(smaller) > 0);
    }

    [Fact]
    public void Sort_ListOfNips_ProducesAscendingOrder()
    {
        var a = Nip.Parse(ValidNipWithLeadingZero); // 0123456789
        var b = Nip.Parse(ValidNip);                // 1234563218
        var c = Nip.Parse(AnotherValidNip);         // 7680002466

        var list = new List<Nip> { c, b, a };
        list.Sort();

        Assert.Equal(new List<Nip> { a, b, c }, list);
    }

    // --- ToString ---

    [Fact]
    public void ToString_PreservesLeadingZeros()
    {
        var nip = Nip.Parse(ValidNipWithLeadingZero);

        Assert.Equal(ValidNipWithLeadingZero, nip.ToString());
    }

    [Fact]
    public void Parse_ToString_Parse_RoundtripPreservesValue()
    {
        var parsed = Nip.Parse(ValidNipWithLeadingZero);
        var serialized = parsed.ToString();
        var reparsed = Nip.Parse(serialized);

        Assert.Equal(parsed, reparsed);
    }

    // --- ToString(NipFormat) ---

    [Fact]
    public void ToString_DigitsOnly_ReturnsCanonical()
    {
        var nip = Nip.Parse(ValidNip);

        Assert.Equal("1234563218", nip.ToString(NipFormat.DigitsOnly));
    }

    [Fact]
    public void ToString_Hyphenated_ReturnsHyphenatedFormat()
    {
        var nip = Nip.Parse(ValidNip);

        Assert.Equal("123-456-32-18", nip.ToString(NipFormat.Hyphenated));
    }

    [Fact]
    public void ToString_VatEu_ReturnsVatEuFormat()
    {
        var nip = Nip.Parse(ValidNip);

        Assert.Equal("PL1234563218", nip.ToString(NipFormat.VatEu));
    }

    [Fact]
    public void ToString_Hyphenated_WithLeadingZero_ReturnsCorrectFormat()
    {
        var nip = Nip.Parse(ValidNipWithLeadingZero);

        Assert.Equal("012-345-67-89", nip.ToString(NipFormat.Hyphenated));
    }

    [Fact]
    public void ToString_VatEu_WithLeadingZero_ReturnsCorrectFormat()
    {
        var nip = Nip.Parse(ValidNipWithLeadingZero);

        Assert.Equal("PL0123456789", nip.ToString(NipFormat.VatEu));
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

        Assert.Equal(ValidNipWithLeadingZero, nip.ToString(format, null));
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

        var ex = Assert.Throws<FormatException>(() => nip.ToString(format, null));

        Assert.Contains(format, ex.Message, StringComparison.Ordinal);
    }

    // --- NipValidationException ---

    [Fact]
    public void NipValidationException_Message_ContainsErrorName()
    {
        var ex = Assert.Throws<NipValidationException>(() => Nip.Parse(InvalidChecksumNip));

        Assert.Contains(nameof(NipValidationError.InvalidChecksum), ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void NipValidationException_InnerException_IsNull()
    {
        var ex = Assert.Throws<NipValidationException>(() => Nip.Parse(InvalidChecksumNip));

        Assert.Null(ex.InnerException);
    }

    // --- Validate API consistency ---

    [Fact]
    public void Validate_Null_ReturnsInvalidLength_UnlikeParseWhichThrowsArgumentNullException()
    {
        var result = Nip.Validate(null);

        Assert.False(result.IsValid);
        Assert.Equal(NipValidationError.InvalidLength, result.Error);
    }

    [Fact]
    public void Validate_ValidNip_IsConsistentWithTryParse()
    {
        var canParse = Nip.TryParse(ValidNip, out _);
        var result = Nip.Validate(ValidNip);

        Assert.Equal(canParse, result.IsValid);
    }

    [Fact]
    public void Validate_InvalidNip_IsConsistentWithTryParse()
    {
        var canParse = Nip.TryParse(InvalidChecksumNip, out _);
        var result = Nip.Validate(InvalidChecksumNip);

        Assert.Equal(canParse, result.IsValid);
    }

    // --- Struct semantics ---

    [Fact]
    public void StructAssignment_ProducesEqualCopy()
    {
        var original = Nip.Parse(ValidNip);
        var copy = original;

        Assert.Equal(original, copy);
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

        Assert.Empty(failures);
    }

    // --- net10 only ---

#if NET10_0_OR_GREATER
    [Fact]
    public void IParsable_Parse_ValidNip_ReturnsNip()
    {
        static T CallParse<T>(string s) where T : IParsable<T> => T.Parse(s, null);

        var nip = CallParse<Nip>(ValidNip);

        Assert.Equal(ValidNip, nip.ToString());
    }

    [Fact]
    public void IParsable_TryParse_ValidNip_ReturnsTrue()
    {
        static bool CallTryParse<T>(string? s, out T result) where T : struct, IParsable<T>
            => T.TryParse(s, null, out result);

        Assert.True(CallTryParse<Nip>(ValidNip, out _));
    }

    [Fact]
    public void IParsable_Parse_InvalidNip_ThrowsNipValidationException()
    {
        static T CallParse<T>(string s) where T : IParsable<T> => T.Parse(s, null);

        Assert.Throws<NipValidationException>(() => CallParse<Nip>(InvalidChecksumNip));
    }

    [Fact]
    public void IParsable_Parse_Null_ThrowsArgumentNullException()
    {
        static T CallParse<T>(string? s) where T : IParsable<T> => T.Parse(s!, null);

        Assert.Throws<ArgumentNullException>(() => CallParse<Nip>(null));
    }

    [Fact]
    public void IParsable_TryParse_InvalidNip_ReturnsFalse()
    {
        static bool CallTryParse<T>(string? s, out T result) where T : struct, IParsable<T>
            => T.TryParse(s, null, out result);

        Assert.False(CallTryParse<Nip>(InvalidChecksumNip, out _));
    }

    [Fact]
    public void IParsable_TryParse_Null_ReturnsFalse()
    {
        static bool CallTryParse<T>(string? s, out T result) where T : struct, IParsable<T>
            => T.TryParse(s, null, out result);

        Assert.False(CallTryParse<Nip>(null, out _));
    }

    [Fact]
    public void ISpanParsable_Parse_ValidNip_ReturnsNip()
    {
        static T CallParse<T>(ReadOnlySpan<char> s) where T : ISpanParsable<T> => T.Parse(s, null);

        var nip = CallParse<Nip>(ValidNip.AsSpan());

        Assert.Equal(ValidNip, nip.ToString());
    }

    [Fact]
    public void ISpanParsable_TryParse_ValidNip_ReturnsTrue()
    {
        static bool CallTryParse<T>(ReadOnlySpan<char> s, out T result) where T : struct, ISpanParsable<T>
            => T.TryParse(s, null, out result);

        Assert.True(CallTryParse<Nip>(ValidNip.AsSpan(), out _));
    }

    [Fact]
    public void ISpanParsable_Parse_InvalidNip_ThrowsNipValidationException()
    {
        static T CallParse<T>(ReadOnlySpan<char> s) where T : ISpanParsable<T> => T.Parse(s, null);

        Assert.Throws<NipValidationException>(() => CallParse<Nip>(InvalidChecksumNip.AsSpan()));
    }

    [Fact]
    public void ISpanParsable_TryParse_InvalidNip_ReturnsFalse()
    {
        static bool CallTryParse<T>(ReadOnlySpan<char> s, out T result) where T : struct, ISpanParsable<T>
            => T.TryParse(s, null, out result);

        Assert.False(CallTryParse<Nip>(InvalidChecksumNip.AsSpan(), out _));
    }

    [Fact]
    public void ISpanParsable_Parse_EmptySpan_ThrowsNipValidationExceptionWithInvalidLength()
    {
        static T CallParse<T>(ReadOnlySpan<char> s) where T : ISpanParsable<T> => T.Parse(s, null);

        var ex = Assert.Throws<NipValidationException>(() => CallParse<Nip>(ReadOnlySpan<char>.Empty));

        Assert.Equal(NipValidationError.InvalidLength, ex.Error);
    }
#endif
}
