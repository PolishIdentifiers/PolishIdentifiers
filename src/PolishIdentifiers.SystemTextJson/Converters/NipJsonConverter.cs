using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PolishIdentifiers;

/// <summary>
/// Converts <see cref="Nip"/> to and from JSON string representation.
/// </summary>
/// <remarks>
/// Polish identifiers are always represented in JSON as strings, never as numbers or objects.
/// Accepts any NIP string recognized by <see cref="Nip.TryParse(string?, out Nip, out NipValidationError?)"/>,
/// including the canonical digits-only representation and the supported formatted representations.
/// Always serializes to the configured <see cref="NipFormat"/> (default: <see cref="NipFormat.DigitsOnly"/>).
///
/// Exception messages do not contain raw identifier values to prevent accidental logging of PII.
/// Error details are available via <see cref="Exception.InnerException"/>.
/// </remarks>
public sealed class NipJsonConverter : JsonConverter<Nip>
{
    private readonly NipFormat _format;

    /// <summary>
    /// Initializes a new instance of the <see cref="NipJsonConverter"/> class.
    /// </summary>
    /// <param name="format">The output format used when writing <see cref="Nip"/> values.</param>
    public NipJsonConverter(NipFormat format = NipFormat.DigitsOnly)
        => _format = format;

    /// <summary>
    /// Reads a <see cref="Nip"/> value from a JSON string.
    /// </summary>
    /// <exception cref="JsonException">
    /// Thrown when the JSON value is null, empty, whitespace, or not a valid NIP.
    /// When the value fails domain validation, <see cref="Exception.InnerException"/> is a
    /// <see cref="NipValidationException"/> containing the specific <see cref="NipValidationError"/>.
    /// Exception messages do not contain raw identifier values.
    /// </exception>
    public override Nip Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = PolishIdentifierJsonConverterHelper.ReadStringValue(ref reader, "NIP");

        if (!Nip.TryParse(value, out var nip, out var error))
            throw new JsonException($"Invalid NIP value: {error}", new NipValidationException(error!.Value));

        return nip;
    }

    /// <summary>
    /// Writes a <see cref="Nip"/> value as a JSON string.
    /// </summary>
    /// <exception cref="JsonException">
    /// Thrown during serialization if <paramref name="value"/> is a default instance.
    /// Use <see cref="Nullable{T}"/> if the value is optional.
    /// </exception>
    public override void Write(Utf8JsonWriter writer, Nip value, JsonSerializerOptions options)
    {
        if (value.IsDefault)
            throw new JsonException("Cannot serialize a default Nip instance. Use Nip? if the value is optional.");

        writer.WriteStringValue(value.ToString(_format));
    }
}
