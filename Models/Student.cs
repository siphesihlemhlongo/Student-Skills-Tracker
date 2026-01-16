namespace StudentSkillsTracker.Models;

/// <summary>
/// Represents a student enrolled in the training programme.
/// </summary>
public class Student
{
    public int Id { get; set; }
    public string Name { get; private set; }
    public string Email { get; private set; }
    public DateTime EnrollmentDate { get; private set; }
    public List<SkillProgress> SkillProgressList { get; private set; }

    public Student(int id, string name, string email)
    {
        Id = id;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Email = email ?? throw new ArgumentNullException(nameof(email));
        EnrollmentDate = DateTime.Now;
        SkillProgressList = new List<SkillProgress>();
    }

    /// <summary>
    /// Updates the student's name.
    /// </summary>
    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));
        Name = name;
    }

    /// <summary>
    /// Updates the student's email.
    /// </summary>
    public void UpdateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));
        Email = email;
    }

    /// <summary>
    /// Adds or updates progress for a specific skill.
    /// </summary>
    public void UpdateSkillProgress(int skillId, int score)
    {
        var existingProgress = SkillProgressList.FirstOrDefault(sp => sp.SkillId == skillId);
        
        if (existingProgress != null)
        {
            existingProgress.UpdateScore(score);
        }
        else
        {
            SkillProgressList.Add(new SkillProgress(skillId, score));
        }
    }

    /// <summary>
    /// Gets the progress for a specific skill.
    /// </summary>
    public SkillProgress? GetSkillProgress(int skillId)
    {
        return SkillProgressList.FirstOrDefault(sp => sp.SkillId == skillId);
    }

    public override string ToString()
    {
        return $"[{Id}] {Name} ({Email}) - Enrolled: {EnrollmentDate:yyyy-MM-dd}";
    }
}
