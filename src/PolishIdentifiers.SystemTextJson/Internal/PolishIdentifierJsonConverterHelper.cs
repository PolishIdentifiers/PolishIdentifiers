using System.Text.Json;

namespace PolishIdentifiers;

internal static class PolishIdentifierJsonConverterHelper
{
    internal static string ReadStringValue(ref Utf8JsonReader reader, string identifierName)
    {
        if (reader.TokenType == JsonTokenType.Null)
            throw new JsonException($"{identifierName} value cannot be null.");

        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException($"{identifierName} value must be a JSON string, got {reader.TokenType}.");

        var value = reader.GetString();

        if (value is null)
            throw new JsonException($"{identifierName} value cannot be null.");

        if (string.IsNullOrWhiteSpace(value))
            throw new JsonException($"{identifierName} value cannot be empty or whitespace.");

        return value;
    }
}
