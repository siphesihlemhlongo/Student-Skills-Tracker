namespace StudentSkillsTracker.Models;

/// <summary>
/// Represents a training programme with required skills for certification.
/// </summary>
public class TrainingProgram
{
    public int Id { get; set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public List<int> RequiredSkillIds { get; private set; }
    public int MinimumPassingPercentage { get; private set; }

    public TrainingProgram(int id, string name, string description, int minimumPassingPercentage = 100)
    {
        Id = id;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        RequiredSkillIds = new List<int>();
        
        if (minimumPassingPercentage < 0 || minimumPassingPercentage > 100)
            throw new ArgumentOutOfRangeException(nameof(minimumPassingPercentage), 
                "Minimum passing percentage must be between 0 and 100.");
        
        MinimumPassingPercentage = minimumPassingPercentage;
    }

    /// <summary>
    /// Adds a required skill to the programme.
    /// </summary>
    public void AddRequiredSkill(int skillId)
    {
        if (!RequiredSkillIds.Contains(skillId))
        {
            RequiredSkillIds.Add(skillId);
        }
    }

    /// <summary>
    /// Removes a required skill from the programme.
    /// </summary>
    public void RemoveRequiredSkill(int skillId)
    {
        RequiredSkillIds.Remove(skillId);
    }

    /// <summary>
    /// Gets the total number of required skills.
    /// </summary>
    public int TotalRequiredSkills => RequiredSkillIds.Count;

    public override string ToString()
    {
        return $"[{Id}] {Name} - {RequiredSkillIds.Count} required skills, {MinimumPassingPercentage}% minimum";
    }
}
