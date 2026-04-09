namespace PolishIdentifiers;

/// <summary>
/// Configures JSON serialization options for Polish identifiers.
/// </summary>
public sealed class PolishIdentifiersJsonOptions
{
    /// <summary>
    /// Gets or sets the output format used when serializing <see cref="Nip"/> values.
    /// </summary>
    public NipFormat NipOutputFormat { get; set; } = NipFormat.DigitsOnly;
}
