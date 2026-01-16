using StudentSkillsTracker.Models;

namespace StudentSkillsTracker.Repositories;

/// <summary>
/// Repository for managing skills with database persistence support.
/// </summary>
public class SkillRepository
{
    private readonly List<Skill> _skills;
    private int _nextId;

    public SkillRepository()
    {
        _skills = new List<Skill>();
        _nextId = 1;
    }

    /// <summary>
    /// Adds a new skill to the repository.
    /// </summary>
    public Skill Add(string name, string description, string category, int passingScore = 70)
    {
        var skill = new Skill(_nextId++, name, description, category, passingScore);
        _skills.Add(skill);
        return skill;
    }

    /// <summary>
    /// Adds a skill with a specific ID (used when loading from database).
    /// </summary>
    public Skill AddWithId(int id, string name, string description, string category, int passingScore = 70)
    {
        var skill = new Skill(id, name, description, category, passingScore);
        _skills.Add(skill);
        if (id >= _nextId) _nextId = id + 1;
        return skill;
    }

    /// <summary>
    /// Gets a skill by its ID.
    /// </summary>
    public Skill? GetById(int id)
    {
        return _skills.FirstOrDefault(s => s.Id == id);
    }

    /// <summary>
    /// Gets all skills in the repository.
    /// </summary>
    public IReadOnlyList<Skill> GetAll()
    {
        return _skills.AsReadOnly();
    }

    /// <summary>
    /// Gets skills filtered by category.
    /// </summary>
    public IEnumerable<Skill> GetByCategory(string category)
    {
        return _skills.Where(s => s.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets all unique categories.
    /// </summary>
    public IEnumerable<string> GetCategories()
    {
        return _skills.Select(s => s.Category).Distinct().OrderBy(c => c);
    }

    /// <summary>
    /// Gets the total count of skills.
    /// </summary>
    public int Count => _skills.Count;
}
