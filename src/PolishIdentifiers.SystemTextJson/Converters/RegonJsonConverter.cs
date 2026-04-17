using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PolishIdentifiers;

/// <summary>
/// Converts <see cref="Regon"/> to and from JSON string representation.
/// </summary>
/// <remarks>
/// Polish identifiers are always represented in JSON as strings, never as numbers or objects.
/// Exception messages do not contain raw identifier values to prevent accidental logging of PII.
/// Error details are available via <see cref="Exception.InnerException"/>.
/// </remarks>
public sealed class RegonJsonConverter : JsonConverter<Regon>
{
    /// <summary>
    /// Reads a <see cref="Regon"/> value from a JSON string.
    /// </summary>
    /// <exception cref="JsonException">
    /// Thrown when the JSON value is null, empty, whitespace, or not a valid REGON.
    /// When the value fails domain validation, <see cref="Exception.InnerException"/> is a
    /// <see cref="RegonValidationException"/> containing the specific <see cref="RegonValidationError"/>.
    /// Exception messages do not contain raw identifier values.
    /// </exception>
    public override Regon Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = PolishIdentifierJsonConverterHelper.ReadStringValue(ref reader, "REGON");

        if (!Regon.TryParse(value, out var regon, out var error))
            throw new JsonException($"Invalid REGON value: {error}", new RegonValidationException(error!.Value));

        return regon;
    }

    /// <summary>
    /// Writes a <see cref="Regon"/> value as a JSON string.
    /// </summary>
    /// <exception cref="JsonException">
    /// Thrown during serialization if <paramref name="value"/> is a default instance.
    /// Use <see cref="Nullable{T}"/> if the value is optional.
    /// </exception>
    public override void Write(Utf8JsonWriter writer, Regon value, JsonSerializerOptions options)
    {
        if (value.IsDefault)
            throw new JsonException("Cannot serialize a default Regon instance. Use Regon? if the value is optional.");

        writer.WriteStringValue(value.ToString());
    }
}
