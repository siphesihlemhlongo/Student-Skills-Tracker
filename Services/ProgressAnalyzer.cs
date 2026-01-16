using StudentSkillsTracker.Models;
using StudentSkillsTracker.Repositories;

namespace StudentSkillsTracker.Services;

/// <summary>
/// Provides analytics and progress analysis for students and training programs.
/// </summary>
public class ProgressAnalyzer
{
    private readonly SkillRepository _skillRepository;

    public ProgressAnalyzer(SkillRepository skillRepository)
    {
        _skillRepository = skillRepository;
    }

    /// <summary>
    /// Calculates the overall progress percentage for a student across all skills.
    /// </summary>
    public double CalculateOverallProgress(Student student)
    {
        var allSkills = _skillRepository.GetAll();
        if (allSkills.Count == 0) return 0;

        int totalCompleted = 0;
        foreach (var skill in allSkills)
        {
            var progress = student.GetSkillProgress(skill.Id);
            if (progress != null && progress.IsCompleted(skill.PassingScore))
            {
                totalCompleted++;
            }
        }

        return (double)totalCompleted / allSkills.Count * 100;
    }

    /// <summary>
    /// Calculates certification readiness for a student in a specific training program.
    /// </summary>
    public CertificationReadiness CheckCertificationReadiness(Student student, TrainingProgram program)
    {
        int completedSkills = 0;
        var incompleteSkills = new List<Skill>();
        var completedSkillsList = new List<Skill>();

        foreach (var skillId in program.RequiredSkillIds)
        {
            var skill = _skillRepository.GetById(skillId);
            if (skill == null) continue;

            var progress = student.GetSkillProgress(skillId);
            if (progress != null && progress.IsCompleted(skill.PassingScore))
            {
                completedSkills++;
                completedSkillsList.Add(skill);
            }
            else
            {
                incompleteSkills.Add(skill);
            }
        }

        double readinessPercentage = program.RequiredSkillIds.Count > 0
            ? (double)completedSkills / program.RequiredSkillIds.Count * 100
            : 0;

        bool isReady = readinessPercentage >= program.MinimumPassingPercentage;

        return new CertificationReadiness
        {
            Student = student,
            Program = program,
            CompletedSkills = completedSkillsList,
            IncompleteSkills = incompleteSkills,
            ReadinessPercentage = readinessPercentage,
            IsReadyForCertification = isReady
        };
    }

    /// <summary>
    /// Generates a detailed progress summary for a student.
    /// </summary>
    public ProgressSummary GenerateProgressSummary(Student student)
    {
        var allSkills = _skillRepository.GetAll();
        var summary = new ProgressSummary { Student = student };

        foreach (var skill in allSkills)
        {
            var progress = student.GetSkillProgress(skill.Id);
            
            if (progress == null || progress.Status == ProgressStatus.NotStarted)
            {
                summary.NotStartedSkills.Add(skill);
            }
            else if (progress.IsCompleted(skill.PassingScore))
            {
                summary.CompletedSkills.Add((skill, progress.CurrentScore));
            }
            else
            {
                summary.InProgressSkills.Add((skill, progress.CurrentScore));
            }
        }

        summary.OverallProgressPercentage = allSkills.Count > 0
            ? (double)summary.CompletedSkills.Count / allSkills.Count * 100
            : 0;

        return summary;
    }

    /// <summary>
    /// Identifies skills that need attention (lowest scores).
    /// </summary>
    public IEnumerable<(Skill Skill, int Score, int Gap)> GetSkillsNeedingAttention(Student student, int count = 3)
    {
        var allSkills = _skillRepository.GetAll();
        var skillsWithScores = new List<(Skill Skill, int Score, int Gap)>();

        foreach (var skill in allSkills)
        {
            var progress = student.GetSkillProgress(skill.Id);
            int score = progress?.CurrentScore ?? 0;
            
            if (score < skill.PassingScore)
            {
                int gap = skill.PassingScore - score;
                skillsWithScores.Add((skill, score, gap));
            }
        }

        return skillsWithScores.OrderByDescending(s => s.Gap).Take(count);
    }

    /// <summary>
    /// Calculates class-wide statistics.
    /// </summary>
    public ClassStatistics CalculateClassStatistics(IEnumerable<Student> students)
    {
        var studentList = students.ToList();
        if (studentList.Count == 0)
        {
            return new ClassStatistics();
        }

        var progressByStudent = studentList
            .Select(s => (Student: s, Progress: CalculateOverallProgress(s)))
            .OrderByDescending(x => x.Progress)
            .ToList();

        return new ClassStatistics
        {
            TotalStudents = studentList.Count,
            AverageProgress = progressByStudent.Average(x => x.Progress),
            HighestProgress = progressByStudent.First().Progress,
            LowestProgress = progressByStudent.Last().Progress,
            TopPerformers = progressByStudent.Take(3).Select(x => (x.Student, x.Progress)).ToList(),
            StudentsNeedingSupport = progressByStudent
                .Where(x => x.Progress < 50)
                .Select(x => (x.Student, x.Progress))
                .ToList()
        };
    }
}

/// <summary>
/// Represents certification readiness analysis results.
/// </summary>
public class CertificationReadiness
{
    public Student Student { get; set; } = null!;
    public TrainingProgram Program { get; set; } = null!;
    public List<Skill> CompletedSkills { get; set; } = new();
    public List<Skill> IncompleteSkills { get; set; } = new();
    public double ReadinessPercentage { get; set; }
    public bool IsReadyForCertification { get; set; }
}

/// <summary>
/// Represents a detailed progress summary for a student.
/// </summary>
public class ProgressSummary
{
    public Student Student { get; set; } = null!;
    public List<(Skill Skill, int Score)> CompletedSkills { get; set; } = new();
    public List<(Skill Skill, int Score)> InProgressSkills { get; set; } = new();
    public List<Skill> NotStartedSkills { get; set; } = new();
    public double OverallProgressPercentage { get; set; }
}

/// <summary>
/// Represents class-wide statistics.
/// </summary>
public class ClassStatistics
{
    public int TotalStudents { get; set; }
    public double AverageProgress { get; set; }
    public double HighestProgress { get; set; }
    public double LowestProgress { get; set; }
    public List<(Student Student, double Progress)> TopPerformers { get; set; } = new();
    public List<(Student Student, double Progress)> StudentsNeedingSupport { get; set; } = new();
}
