using StudentSkillsTracker.Models;

namespace StudentSkillsTracker.Repositories;

/// <summary>
/// Repository for managing students with database persistence support.
/// </summary>
public class StudentRepository
{
    private readonly List<Student> _students;
    private int _nextId;

    public StudentRepository()
    {
        _students = new List<Student>();
        _nextId = 1;
    }

    /// <summary>
    /// Sets the next ID (used when loading from database).
    /// </summary>
    public void SetNextId(int nextId)
    {
        _nextId = nextId;
    }

    /// <summary>
    /// Adds a new student to the repository.
    /// </summary>
    public Student Add(string name, string email)
    {
        var student = new Student(_nextId++, name, email);
        _students.Add(student);
        return student;
    }

    /// <summary>
    /// Adds a student with a specific ID (used when loading from database).
    /// </summary>
    public Student AddWithId(int id, string name, string email)
    {
        var student = new Student(id, name, email);
        _students.Add(student);
        if (id >= _nextId) _nextId = id + 1;
        return student;
    }

    /// <summary>
    /// Gets a student by their ID.
    /// </summary>
    public Student? GetById(int id)
    {
        return _students.FirstOrDefault(s => s.Id == id);
    }

    /// <summary>
    /// Gets all students in the repository.
    /// </summary>
    public IReadOnlyList<Student> GetAll()
    {
        return _students.AsReadOnly();
    }

    /// <summary>
    /// Deletes a student by their ID.
    /// </summary>
    public bool Delete(int id)
    {
        var student = GetById(id);
        if (student != null)
        {
            _students.Remove(student);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Gets the total count of students.
    /// </summary>
    public int Count => _students.Count;

    /// <summary>
    /// Searches for students by name (case-insensitive).
    /// </summary>
    public IEnumerable<Student> SearchByName(string name)
    {
        return _students.Where(s => s.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
    }
}
