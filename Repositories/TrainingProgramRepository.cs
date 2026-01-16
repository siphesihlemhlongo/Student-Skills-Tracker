using StudentSkillsTracker.Models;

namespace StudentSkillsTracker.Repositories;

/// <summary>
/// Repository for managing training programs with database persistence support.
/// </summary>
public class TrainingProgramRepository
{
    private readonly List<TrainingProgram> _programs;
    private int _nextId;

    public TrainingProgramRepository()
    {
        _programs = new List<TrainingProgram>();
        _nextId = 1;
    }

    /// <summary>
    /// Adds a new training program to the repository.
    /// </summary>
    public TrainingProgram Add(string name, string description, int minimumPassingPercentage = 100)
    {
        var program = new TrainingProgram(_nextId++, name, description, minimumPassingPercentage);
        _programs.Add(program);
        return program;
    }

    /// <summary>
    /// Adds a program with a specific ID (used when loading from database).
    /// </summary>
    public TrainingProgram AddWithId(int id, string name, string description, int minimumPassingPercentage = 100)
    {
        var program = new TrainingProgram(id, name, description, minimumPassingPercentage);
        _programs.Add(program);
        if (id >= _nextId) _nextId = id + 1;
        return program;
    }

    /// <summary>
    /// Gets a training program by its ID.
    /// </summary>
    public TrainingProgram? GetById(int id)
    {
        return _programs.FirstOrDefault(p => p.Id == id);
    }

    /// <summary>
    /// Gets all training programs.
    /// </summary>
    public IReadOnlyList<TrainingProgram> GetAll()
    {
        return _programs.AsReadOnly();
    }

    /// <summary>
    /// Gets the total count of programs.
    /// </summary>
    public int Count => _programs.Count;
}
