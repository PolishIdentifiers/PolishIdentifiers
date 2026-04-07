using PolishIdentifiers;
using Shouldly;

namespace PolishIdentifiers.Tests;

public class NipValidationErrorOrdinalTests
{
    [Theory]
    [InlineData(NipValidationError.InvalidCharacters, 0)]
    [InlineData(NipValidationError.InvalidLength, 1)]
    [InlineData(NipValidationError.UnrecognizedFormat, 2)]
    [InlineData(NipValidationError.InvalidChecksum, 3)]
    public void NipValidationError_OrdinalValue_MatchesExpected(NipValidationError error, int expectedValue)
    {
        ((int)error).ShouldBe(expectedValue);
    }
}
