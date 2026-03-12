using PolishIdentifiers;
using Shouldly;

namespace PolishIdentifiers.Tests;

public class RegonParsingTests
{
    // "123456785": sum=192, 192%11=5, d8=5 ✓
    private const string ValidRegon9 = "123456785";
    // "012345675": sum=148, 148%11=5, d8=5 ✓
    private const string ValidRegon9WithLeadingZero = "012345675";
    // "491996453": sum=267, 267%11=3, d8=3 ✓
    private const string AnotherValidRegon9 = "491996453";
    // "12345678512347": base "123456785" valid; sum=260, 260%11=7, d13=7 ✓
    private const string ValidRegon14 = "12345678512347";
    // "01234567512342": base "012345675" valid; sum=222, 222%11=2, d13=2 ✓
    private const string ValidRegon14WithLeadingZeroBase = "01234567512342";
    // "49199645300010": base "491996453" valid; sum=220, 220%11=0, d13=0 ✓
    private const string AnotherValidRegon14 = "49199645300010";

    private const string InvalidRegon = "123456780";
    private const string InvalidRegonCharacters = "12345678X";
    private const string InvalidRegonWrongLength = "1234567";

    // ── Parse (strict) ────────────────────────────────────────────────────────

    [Fact]
    public void Parse_ValidRegon9_ReturnsInstance()
    {
        var regon = Regon.Parse(ValidRegon9);

        regon.IsDefault.ShouldBeFalse();
        regon.ToString().ShouldBe(ValidRegon9);
    }

    [Fact]
    public void Parse_ValidRegon9WithLeadingZero_ReturnsInstance()
    {
        var regon = Regon.Parse(ValidRegon9WithLeadingZero);

        regon.IsDefault.ShouldBeFalse();
        regon.ToString().ShouldBe(ValidRegon9WithLeadingZero);
    }

    [Fact]
    public void Parse_ValidRegon14_ReturnsInstance()
    {
        var regon = Regon.Parse(ValidRegon14);

        regon.IsDefault.ShouldBeFalse();
        regon.ToString().ShouldBe(ValidRegon14);
    }

    [Fact]
    public void Parse_ValidRegon14WithLeadingZeroBase_ReturnsInstance()
    {
        var regon = Regon.Parse(ValidRegon14WithLeadingZeroBase);

        regon.IsDefault.ShouldBeFalse();
        regon.ToString().ShouldBe(ValidRegon14WithLeadingZeroBase);
    }

    [Fact]
    public void Parse_NullString_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() => Regon.Parse((string)null!));
    }

    [Fact]
    public void Parse_InvalidRegon_ThrowsRegonValidationException()
    {
        var ex = Should.Throw<RegonValidationException>(() => Regon.Parse(InvalidRegon));

        ex.Error.ShouldBe(RegonValidationError.InvalidChecksum);
    }

    [Fact]
    public void Parse_SpanOverload_ValidRegon9_ReturnsInstance()
    {
        var regon = Regon.Parse(ValidRegon9.AsSpan());

        regon.ToString().ShouldBe(ValidRegon9);
    }

    [Fact]
    public void Parse_SpanOverload_ValidRegon14_ReturnsInstance()
    {
        var regon = Regon.Parse(ValidRegon14.AsSpan());

        regon.ToString().ShouldBe(ValidRegon14);
    }

    [Fact]
    public void Parse_SpanOverload_InvalidCharacters_ThrowsRegonValidationException()
    {
        var ex = Should.Throw<RegonValidationException>(() => Regon.Parse(InvalidRegonCharacters.AsSpan()));

        ex.Error.ShouldBe(RegonValidationError.InvalidCharacters);
    }

    [Fact]
    public void Parse_SpanOverload_InvalidLength_ThrowsRegonValidationException()
    {
        var ex = Should.Throw<RegonValidationException>(() => Regon.Parse(InvalidRegonWrongLength.AsSpan()));

        ex.Error.ShouldBe(RegonValidationError.InvalidLength);
    }

    // ── TryParse ──────────────────────────────────────────────────────────────

    [Fact]
    public void TryParse_ValidRegon9_ReturnsTrueAndInstance()
    {
        var success = Regon.TryParse(ValidRegon9, out var regon);

        success.ShouldBeTrue();
        regon.IsDefault.ShouldBeFalse();
        regon.ToString().ShouldBe(ValidRegon9);
    }

    [Fact]
    public void TryParse_ValidRegon14_ReturnsTrueAndInstance()
    {
        var success = Regon.TryParse(ValidRegon14, out var regon);

        success.ShouldBeTrue();
        regon.ToString().ShouldBe(ValidRegon14);
    }

    [Fact]
    public void TryParse_NullString_ReturnsFalseAndDefault()
    {
        var success = Regon.TryParse(null, out var regon);

        success.ShouldBeFalse();
        regon.IsDefault.ShouldBeTrue();
    }

    [Fact]
    public void TryParse_InvalidRegon_ReturnsFalseAndDefault()
    {
        var success = Regon.TryParse(InvalidRegon, out var regon);

        success.ShouldBeFalse();
        regon.IsDefault.ShouldBeTrue();
    }

    [Fact]
    public void TryParse_SpanOverload_ValidRegon9_ReturnsTrueAndInstance()
    {
        var success = Regon.TryParse(ValidRegon9.AsSpan(), out var regon);

        success.ShouldBeTrue();
        regon.ToString().ShouldBe(ValidRegon9);
    }

    [Fact]
    public void TryParse_SpanOverload_InvalidRegon_ReturnsFalseAndDefault()
    {
        var success = Regon.TryParse(InvalidRegon.AsSpan(), out var regon);

        success.ShouldBeFalse();
        regon.IsDefault.ShouldBeTrue();
    }

    // ── Validate consistency ──────────────────────────────────────────────────

    [Fact]
    public void Validate_And_TryParse_AreConsistent_ForValidRegon9()
    {
        var validateResult = Regon.Validate(ValidRegon9);
        var tryParseResult = Regon.TryParse(ValidRegon9, out _);

        validateResult.IsValid.ShouldBe(tryParseResult);
    }

    [Fact]
    public void Validate_And_TryParse_AreConsistent_ForInvalidRegon()
    {
        var validateResult = Regon.Validate(InvalidRegon);
        var tryParseResult = Regon.TryParse(InvalidRegon, out _);

        validateResult.IsValid.ShouldBe(tryParseResult);
    }

    // ── Domain properties: Kind / IsMain / IsLocal ────────────────────────────

    [Fact]
    public void Kind_Regon9_ReturnsMain()
    {
        var regon = Regon.Parse(ValidRegon9);

        regon.Kind.ShouldBe(RegonKind.Main);
    }

    [Fact]
    public void IsMain_Regon9_ReturnsTrue()
    {
        var regon = Regon.Parse(ValidRegon9);

        regon.IsMain.ShouldBeTrue();
        regon.IsLocal.ShouldBeFalse();
    }

    [Fact]
    public void Kind_Regon14_ReturnsLocal()
    {
        var regon = Regon.Parse(ValidRegon14);

        regon.Kind.ShouldBe(RegonKind.Local);
    }

    [Fact]
    public void IsLocal_Regon14_ReturnsTrue()
    {
        var regon = Regon.Parse(ValidRegon14);

        regon.IsLocal.ShouldBeTrue();
        regon.IsMain.ShouldBeFalse();
    }

    // ── Domain properties: BaseRegon ──────────────────────────────────────────

    [Fact]
    public void BaseRegon_Regon9_ReturnsSelf()
    {
        var regon = Regon.Parse(ValidRegon9);

        regon.BaseRegon.ShouldBe(regon);
    }

    [Fact]
    public void BaseRegon_Regon14_ReturnsParentRegon9()
    {
        var regon14 = Regon.Parse(ValidRegon14);
        var expectedBase = Regon.Parse(ValidRegon9); // first 9 digits of ValidRegon14

        regon14.BaseRegon.ShouldBe(expectedBase);
    }

    [Fact]
    public void BaseRegon_Regon14_BaseIsMain()
    {
        var regon14 = Regon.Parse(ValidRegon14);

        regon14.BaseRegon.IsMain.ShouldBeTrue();
    }

    [Fact]
    public void BaseRegon_Regon14_BaseToStringHas9Digits()
    {
        var regon14 = Regon.Parse(ValidRegon14);

        regon14.BaseRegon.ToString().Length.ShouldBe(9);
    }

    [Fact]
    public void BaseRegon_Regon14_BaseMatchesFirstNineDigits()
    {
        var regon14 = Regon.Parse(ValidRegon14);
        var expectedBase9 = ValidRegon14.Substring(0, 9);

        regon14.BaseRegon.ToString().ShouldBe(expectedBase9);
    }

    [Fact]
    public void BaseRegon_Regon14WithLeadingZeroBase_PreservesLeadingZeros()
    {
        var regon14 = Regon.Parse(ValidRegon14WithLeadingZeroBase);

        regon14.BaseRegon.ToString().ShouldBe(ValidRegon9WithLeadingZero);
    }

    // ── IsDefault ────────────────────────────────────────────────────────────

    [Fact]
    public void IsDefault_DefaultInstance_ReturnsTrue()
    {
        Regon regon = default;

        regon.IsDefault.ShouldBeTrue();
    }

    [Fact]
    public void IsDefault_ParsedInstance_ReturnsFalse()
    {
        var regon = Regon.Parse(ValidRegon9);

        regon.IsDefault.ShouldBeFalse();
    }

    [Fact]
    public void Kind_DefaultInstance_ThrowsInvalidOperationException()
    {
        Regon regon = default;

        Should.Throw<InvalidOperationException>(() => _ = regon.Kind);
    }

    [Fact]
    public void IsMain_DefaultInstance_ThrowsInvalidOperationException()
    {
        Regon regon = default;

        Should.Throw<InvalidOperationException>(() => _ = regon.IsMain);
    }

    [Fact]
    public void IsLocal_DefaultInstance_ThrowsInvalidOperationException()
    {
        Regon regon = default;

        Should.Throw<InvalidOperationException>(() => _ = regon.IsLocal);
    }

    [Fact]
    public void BaseRegon_DefaultInstance_ThrowsInvalidOperationException()
    {
        Regon regon = default;

        Should.Throw<InvalidOperationException>(() => _ = regon.BaseRegon);
    }

    [Fact]
    public void ToString_DefaultInstance_ThrowsInvalidOperationException()
    {
        Regon regon = default;

        Should.Throw<InvalidOperationException>(() => regon.ToString());
    }

    // ── ToString ─────────────────────────────────────────────────────────────

    [Fact]
    public void ToString_Regon9_Returns9Digits()
    {
        var regon = Regon.Parse(ValidRegon9);

        regon.ToString().Length.ShouldBe(9);
        regon.ToString().ShouldBe(ValidRegon9);
    }

    [Fact]
    public void ToString_Regon14_Returns14Digits()
    {
        var regon = Regon.Parse(ValidRegon14);

        regon.ToString().Length.ShouldBe(14);
        regon.ToString().ShouldBe(ValidRegon14);
    }

    [Fact]
    public void ToString_FormatNull_ReturnsCanonical()
    {
        var regon = Regon.Parse(ValidRegon9);

        regon.ToString(null, null).ShouldBe(ValidRegon9);
    }

    [Fact]
    public void ToString_FormatG_ReturnsCanonical()
    {
        var regon = Regon.Parse(ValidRegon9);

        regon.ToString("G", null).ShouldBe(ValidRegon9);
    }

    [Fact]
    public void ToString_FormatD9_Regon9_ReturnsCanonical()
    {
        var regon = Regon.Parse(ValidRegon9);

        regon.ToString("D9", null).ShouldBe(ValidRegon9);
    }

    [Fact]
    public void ToString_FormatD14_Regon14_ReturnsCanonical()
    {
        var regon = Regon.Parse(ValidRegon14);

        regon.ToString("D14", null).ShouldBe(ValidRegon14);
    }

    [Fact]
    public void ToString_FormatD14_Regon9_ThrowsFormatException()
    {
        var regon = Regon.Parse(ValidRegon9);

        Should.Throw<FormatException>(() => regon.ToString("D14", null));
    }

    [Fact]
    public void ToString_FormatD9_Regon14_ThrowsFormatException()
    {
        var regon = Regon.Parse(ValidRegon14);

        Should.Throw<FormatException>(() => regon.ToString("D9", null));
    }

    [Fact]
    public void ToString_UnsupportedFormat_ThrowsFormatException()
    {
        var regon = Regon.Parse(ValidRegon9);

        Should.Throw<FormatException>(() => regon.ToString("X", null));
    }

    // ── Equality ─────────────────────────────────────────────────────────────

    [Fact]
    public void Equals_SameRegon9_ReturnsTrue()
    {
        var a = Regon.Parse(ValidRegon9);
        var b = Regon.Parse(ValidRegon9);

        a.Equals(b).ShouldBeTrue();
        (a == b).ShouldBeTrue();
        (a != b).ShouldBeFalse();
    }

    [Fact]
    public void Equals_DifferentRegon9_ReturnsFalse()
    {
        var a = Regon.Parse(ValidRegon9);
        var b = Regon.Parse(AnotherValidRegon9);

        a.Equals(b).ShouldBeFalse();
        (a != b).ShouldBeTrue();
    }

    [Fact]
    public void Equals_SameRegon14_ReturnsTrue()
    {
        var a = Regon.Parse(ValidRegon14);
        var b = Regon.Parse(ValidRegon14);

        a.Equals(b).ShouldBeTrue();
    }

    [Fact]
    public void Equals_DefaultInstances_AreEqual()
    {
        Regon a = default;
        Regon b = default;

        a.Equals(b).ShouldBeTrue();
    }

    [Fact]
    public void Equals_DefaultAndInitialized_ReturnsFalse()
    {
        Regon defaultRegon = default;
        var parsed = Regon.Parse(ValidRegon9);

        defaultRegon.Equals(parsed).ShouldBeFalse();
    }

    [Fact]
    public void GetHashCode_SameRegon_ReturnsSameHash()
    {
        var a = Regon.Parse(ValidRegon9);
        var b = Regon.Parse(ValidRegon9);

        a.GetHashCode().ShouldBe(b.GetHashCode());
    }

    // ── IComparable ──────────────────────────────────────────────────────────

    [Fact]
    public void CompareTo_SameRegon_ReturnsZero()
    {
        var a = Regon.Parse(ValidRegon9);
        var b = Regon.Parse(ValidRegon9);

        a.CompareTo(b).ShouldBe(0);
    }

    [Fact]
    public void CompareTo_DefaultBeforeInitialized()
    {
        Regon defaultRegon = default;
        var parsed = Regon.Parse(ValidRegon9);

        defaultRegon.CompareTo(parsed).ShouldBeLessThan(0);
        parsed.CompareTo(defaultRegon).ShouldBeGreaterThan(0);
    }

    [Fact]
    public void CompareTo_CanSortInCollection()
    {
        var regons = new[]
        {
            Regon.Parse(AnotherValidRegon9),
            Regon.Parse(ValidRegon9),
        };

        var sorted = regons.OrderBy(r => r).ToList();

        sorted[0].ToString().ShouldBe(regons.Min().ToString());
    }

    // ── Collection usage ─────────────────────────────────────────────────────

    [Fact]
    public void CanBeUsedAsHashSetKey()
    {
        var set = new HashSet<Regon>
        {
            Regon.Parse(ValidRegon9),
            Regon.Parse(ValidRegon9),   // duplicate
            Regon.Parse(AnotherValidRegon9),
        };

        set.Count.ShouldBe(2);
    }

    [Fact]
    public void CanBeUsedAsDictionaryKey()
    {
        var dict = new Dictionary<Regon, string>
        {
            [Regon.Parse(ValidRegon9)] = "company-a",
            [Regon.Parse(AnotherValidRegon9)] = "company-b",
        };

        dict[Regon.Parse(ValidRegon9)].ShouldBe("company-a");
    }

#if NET10_0_OR_GREATER
    // ── IParsable<Regon> / ISpanParsable<Regon> (net10 only) ─────────────────

    [Fact]
    public void IParsable_Parse_ValidRegon9_ReturnsInstance()
    {
        static T CallParse<T>(string s) where T : IParsable<T> => T.Parse(s, null);

        var regon = CallParse<Regon>(ValidRegon9);

        regon.ToString().ShouldBe(ValidRegon9);
    }

    [Fact]
    public void IParsable_Parse_Null_ThrowsArgumentNullException()
    {
        static T CallParse<T>(string s) where T : IParsable<T> => T.Parse(s, null);

        Should.Throw<ArgumentNullException>(() => CallParse<Regon>(null!));
    }

    [Fact]
    public void IParsable_Parse_InvalidRegon_ThrowsRegonValidationException()
    {
        static T CallParse<T>(string s) where T : IParsable<T> => T.Parse(s, null);

        var ex = Should.Throw<RegonValidationException>(() => CallParse<Regon>(InvalidRegon));

        ex.Error.ShouldBe(RegonValidationError.InvalidChecksum);
    }

    [Fact]
    public void IParsable_TryParse_ValidRegon9_ReturnsTrueAndInstance()
    {
        static bool CallTryParse<T>(string? s, out T result) where T : struct, IParsable<T>
            => T.TryParse(s, null, out result);

        var success = CallTryParse<Regon>(ValidRegon9, out var result);

        success.ShouldBeTrue();
        result.ToString().ShouldBe(ValidRegon9);
    }

    [Fact]
    public void IParsable_TryParse_Null_ReturnsFalseAndDefault()
    {
        static bool CallTryParse<T>(string? s, out T result) where T : struct, IParsable<T>
            => T.TryParse(s, null, out result);

        var success = CallTryParse<Regon>(null, out var result);

        success.ShouldBeFalse();
        result.IsDefault.ShouldBeTrue();
    }

    [Fact]
    public void IParsable_TryParse_InvalidRegon_ReturnsFalseAndDefault()
    {
        static bool CallTryParse<T>(string? s, out T result) where T : struct, IParsable<T>
            => T.TryParse(s, null, out result);

        var success = CallTryParse<Regon>(InvalidRegon, out var result);

        success.ShouldBeFalse();
        result.IsDefault.ShouldBeTrue();
    }

    [Fact]
    public void ISpanParsable_Parse_ValidRegon14_ReturnsInstance()
    {
        static T CallParse<T>(ReadOnlySpan<char> s) where T : ISpanParsable<T> => T.Parse(s, null);

        var regon = CallParse<Regon>(ValidRegon14.AsSpan());

        regon.ToString().ShouldBe(ValidRegon14);
    }

    [Fact]
    public void ISpanParsable_Parse_InvalidRegon_ThrowsRegonValidationException()
    {
        static T CallParse<T>(ReadOnlySpan<char> s) where T : ISpanParsable<T> => T.Parse(s, null);

        var ex = Should.Throw<RegonValidationException>(() => CallParse<Regon>(InvalidRegon.AsSpan()));

        ex.Error.ShouldBe(RegonValidationError.InvalidChecksum);
    }

    [Fact]
    public void ISpanParsable_TryParse_ValidRegon9_ReturnsTrueAndInstance()
    {
        static bool CallTryParse<T>(ReadOnlySpan<char> s, out T result) where T : struct, ISpanParsable<T>
            => T.TryParse(s, null, out result);

        var success = CallTryParse<Regon>(ValidRegon9.AsSpan(), out var result);

        success.ShouldBeTrue();
        result.ToString().ShouldBe(ValidRegon9);
    }

    [Fact]
    public void ISpanParsable_TryParse_InvalidRegon_ReturnsFalseAndDefault()
    {
        static bool CallTryParse<T>(ReadOnlySpan<char> s, out T result) where T : struct, ISpanParsable<T>
            => T.TryParse(s, null, out result);

        var success = CallTryParse<Regon>(InvalidRegonCharacters.AsSpan(), out var result);

        success.ShouldBeFalse();
        result.IsDefault.ShouldBeTrue();
    }
#endif
}
