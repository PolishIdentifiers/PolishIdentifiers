using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PolishIdentifiers;

/// <summary>
/// Converts <see cref="Pesel"/> to and from JSON string representation.
/// </summary>
/// <remarks>
/// Polish identifiers are always represented in JSON as strings, never as numbers or objects.
/// Exception messages do not contain raw identifier values to prevent accidental logging of PII.
/// Error details are available via <see cref="Exception.InnerException"/>.
/// </remarks>
public sealed class PeselJsonConverter : JsonConverter<Pesel>
{
    /// <summary>
    /// Reads a <see cref="Pesel"/> value from a JSON string.
    /// </summary>
    /// <exception cref="JsonException">
    /// Thrown when the JSON value is null, empty, whitespace, or not a valid PESEL.
    /// When the value fails domain validation, <see cref="Exception.InnerException"/> is a
    /// <see cref="PeselValidationException"/> containing the specific <see cref="PeselValidationError"/>.
    /// Exception messages do not contain raw identifier values.
    /// </exception>
    public override Pesel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = PolishIdentifierJsonConverterHelper.ReadStringValue(ref reader, "PESEL");

        if (!Pesel.TryParse(value, out var pesel, out var error))
            throw new JsonException($"Invalid PESEL value: {error}", new PeselValidationException(error!.Value));

        return pesel;
    }

    /// <summary>
    /// Writes a <see cref="Pesel"/> value as a JSON string.
    /// </summary>
    /// <exception cref="JsonException">
    /// Thrown during serialization if <paramref name="value"/> is a default instance.
    /// Use <see cref="Nullable{T}"/> if the value is optional.
    /// </exception>
    public override void Write(Utf8JsonWriter writer, Pesel value, JsonSerializerOptions options)
    {
        if (value.IsDefault)
            throw new JsonException("Cannot serialize a default Pesel instance. Use Pesel? if the value is optional.");

        writer.WriteStringValue(value.ToString());
    }
}
