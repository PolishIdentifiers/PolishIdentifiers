using PolishIdentifiers;
using Shouldly;

namespace PolishIdentifiers.Tests;

public class RegonValidationErrorOrdinalTests
{
    [Theory]
    [InlineData(RegonValidationError.InvalidCharacters, 0)]
    [InlineData(RegonValidationError.InvalidLength, 1)]
    [InlineData(RegonValidationError.InvalidChecksum, 2)]
    public void RegonValidationError_OrdinalValue_MatchesExpected(RegonValidationError error, int expectedValue)
    {
        ((int)error).ShouldBe(expectedValue);
    }
}
