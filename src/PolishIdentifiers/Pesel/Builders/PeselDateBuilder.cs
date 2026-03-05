namespace PolishIdentifiers;

public sealed class PeselDateBuilder
{
    private readonly DateTime _date;

    internal PeselDateBuilder(DateTime date) => _date = date;

    /// <summary>Generates a valid PESEL for a male person born on the configured date.</summary>
    public Pesel Male()   => PeselGenerator.BuildPesel(_date, Gender.Male);

    /// <summary>Generates a valid PESEL for a female person born on the configured date.</summary>
    public Pesel Female() => PeselGenerator.BuildPesel(_date, Gender.Female);

    /// <summary>
    /// Generates a valid PESEL for a person of the specified <paramref name="gender"/>
    /// born on the configured date. Useful when gender is determined programmatically.
    /// </summary>
    public Pesel WithGender(Gender gender) => PeselGenerator.BuildPesel(_date, gender);
}
