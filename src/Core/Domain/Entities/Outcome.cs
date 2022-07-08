namespace SampleBlog.Core.Domain.Entities;

public enum Units
{
    Custom = -1,
    Pieces,
    Grams,
    Kilo,
    Portion
}

public record Outcome(float Amount, Units Units, string? CustomUnits = null);