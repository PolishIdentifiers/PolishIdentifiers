namespace PolishIdentifiers;

/// <summary>
/// A builder for generating valid PESEL numbers for a specific birth date.
/// Obtain an instance via <see cref="PeselGenerator.ForBirthDate(DateTime)"/>.
/// </summary>
public sealed class PeselDateBuilder
{
    private readonly DateTime _date;

    internal PeselDateBuilder(DateTime date) => _date = date;

    /// <summary>Generates a valid PESEL for a male person born on the configured date.</summary>
    /// <returns>A randomly generated valid <see cref="Pesel"/> with the configured birth date and male gender.</returns>
    public Pesel Male()   => PeselGenerator.BuildPesel(_date, Gender.Male);

    /// <summary>Generates a valid PESEL for a female person born on the configured date.</summary>
    /// <returns>A randomly generated valid <see cref="Pesel"/> with the configured birth date and female gender.</returns>
    public Pesel Female() => PeselGenerator.BuildPesel(_date, Gender.Female);

    /// <summary>
    /// Generates a valid PESEL for a person of the specified gender born on the configured date.
    /// Useful when gender is determined programmatically.
    /// </summary>
    /// <param name="gender">The gender to encode in the generated PESEL.</param>
    /// <returns>A randomly generated valid <see cref="Pesel"/> with the configured birth date and specified gender.</returns>
    public Pesel WithGender(Gender gender) => PeselGenerator.BuildPesel(_date, gender);
}
