namespace PolishIdentifiers;

/// <summary>
/// Represents the biological sex encoded in a PESEL number.
/// </summary>
public enum Gender
{
    /// <summary>Female sex, encoded as an even digit (0, 2, 4, 6, or 8) in the 10th PESEL position.</summary>
    Female,

    /// <summary>Male sex, encoded as an odd digit (1, 3, 5, 7, or 9) in the 10th PESEL position.</summary>
    Male,
}
