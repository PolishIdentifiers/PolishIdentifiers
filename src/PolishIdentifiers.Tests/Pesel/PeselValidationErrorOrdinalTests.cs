using PolishIdentifiers;
using Shouldly;

namespace PolishIdentifiers.Tests;

public class PeselValidationErrorOrdinalTests
{
    [Theory]
    [InlineData(PeselValidationError.InvalidCharacters, 0)]
    [InlineData(PeselValidationError.InvalidLength, 1)]
    [InlineData(PeselValidationError.InvalidDate, 2)]
    [InlineData(PeselValidationError.InvalidChecksum, 3)]
    public void PeselValidationError_OrdinalValue_MatchesExpected(PeselValidationError error, int expectedValue)
    {
        ((int)error).ShouldBe(expectedValue);
    }
}
