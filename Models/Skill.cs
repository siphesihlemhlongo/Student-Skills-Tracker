namespace StudentSkillsTracker.Models;

/// <summary>
/// Represents a skill that students can learn in the training programme.
/// </summary>
public class Skill
{
    public int Id { get; set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Category { get; private set; }
    public int PassingScore { get; private set; }

    public Skill(int id, string name, string description, string category, int passingScore = 70)
    {
        Id = id;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Category = category ?? throw new ArgumentNullException(nameof(category));
        
        if (passingScore < 0 || passingScore > 100)
            throw new ArgumentOutOfRangeException(nameof(passingScore), "Passing score must be between 0 and 100.");
        
        PassingScore = passingScore;
    }

    /// <summary>
    /// Checks if a given score meets the passing threshold.
    /// </summary>
    public bool IsPassing(int score)
    {
        return score >= PassingScore;
    }

    public override string ToString()
    {
        return $"[{Id}] {Name} ({Category}) - Pass: {PassingScore}%";
    }
}
