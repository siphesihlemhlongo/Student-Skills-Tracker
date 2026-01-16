namespace StudentSkillsTracker.Models;

/// <summary>
/// Represents the status of a student's progress in a skill.
/// </summary>
public enum ProgressStatus
{
    NotStarted,
    InProgress,
    Completed
}

/// <summary>
/// Tracks a student's progress in a specific skill.
/// </summary>
public class SkillProgress
{
    public int SkillId { get; private set; }
    public int CurrentScore { get; private set; }
    public ProgressStatus Status { get; private set; }
    public DateTime LastUpdated { get; private set; }

    public SkillProgress(int skillId, int score = 0)
    {
        SkillId = skillId;
        CurrentScore = 0;
        Status = ProgressStatus.NotStarted;
        LastUpdated = DateTime.Now;
        
        if (score > 0)
        {
            UpdateScore(score);
        }
    }

    /// <summary>
    /// Updates the score and automatically updates the status.
    /// </summary>
    public void UpdateScore(int score)
    {
        if (score < 0 || score > 100)
            throw new ArgumentOutOfRangeException(nameof(score), "Score must be between 0 and 100.");

        CurrentScore = score;
        LastUpdated = DateTime.Now;

        // Automatically update status based on score
        if (score == 0)
        {
            Status = ProgressStatus.NotStarted;
        }
        else if (score < 70) // Below typical passing threshold
        {
            Status = ProgressStatus.InProgress;
        }
        else
        {
            Status = ProgressStatus.Completed;
        }
    }

    /// <summary>
    /// Checks if the skill is completed (passing score achieved).
    /// </summary>
    public bool IsCompleted(int passingScore)
    {
        return CurrentScore >= passingScore;
    }

    public override string ToString()
    {
        return $"Skill {SkillId}: {CurrentScore}% ({Status}) - Updated: {LastUpdated:yyyy-MM-dd HH:mm}";
    }
}
