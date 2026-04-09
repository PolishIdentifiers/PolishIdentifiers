using System;
using System.Linq;
using System.Text.Json;

namespace PolishIdentifiers;

/// <summary>
/// Provides extension methods for registering JSON converters for Polish identifiers.
/// </summary>
public static class JsonSerializerOptionsExtensions
{
    /// <summary>
    /// Registers JSON converters for Polish identifiers (<see cref="Pesel"/>, <see cref="Nip"/>, <see cref="Regon"/>).
    /// </summary>
    /// <param name="options">The JSON serializer options to configure.</param>
    /// <param name="polishOptions">Optional formatting options for Polish identifier serialization.</param>
    /// <returns>The same <see cref="JsonSerializerOptions"/> instance for chaining.</returns>
    /// <remarks>
    /// This method is idempotent — calling it multiple times registers each converter at most once.
    /// First registration wins: a second call with a different <see cref="PolishIdentifiersJsonOptions.NipOutputFormat"/>
    /// is silently ignored.
    ///
    /// IMPORTANT: Detection is type-specific. This method checks only for its own converter types
    /// (<see cref="NipJsonConverter"/>, <see cref="PeselJsonConverter"/>, <see cref="RegonJsonConverter"/>).
    /// If you have registered a custom converter for <see cref="Nip"/> of a different type,
    /// this method will still add its own <see cref="NipJsonConverter"/>.
    /// STJ converter resolution is then determined by position in <see cref="JsonSerializerOptions.Converters"/>.
    /// </remarks>
    public static JsonSerializerOptions AddPolishIdentifiers(
        this JsonSerializerOptions options,
        PolishIdentifiersJsonOptions? polishOptions = null)
    {
        if (options is null)
            throw new ArgumentNullException(nameof(options));

        polishOptions ??= new PolishIdentifiersJsonOptions();

        if (!options.Converters.OfType<PeselJsonConverter>().Any())
            options.Converters.Add(new PeselJsonConverter());

        if (!options.Converters.OfType<NipJsonConverter>().Any())
            options.Converters.Add(new NipJsonConverter(polishOptions.NipOutputFormat));

        if (!options.Converters.OfType<RegonJsonConverter>().Any())
            options.Converters.Add(new RegonJsonConverter());

        return options;
    }
}
