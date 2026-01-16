using Microsoft.Data.Sqlite;
using StudentSkillsTracker.Models;
using StudentSkillsTracker.Repositories;

namespace StudentSkillsTracker.Data;

/// <summary>
/// Simple SQLite database helper for persisting data.
/// </summary>
public class DatabaseHelper
{
    private readonly string _connectionString;

    public DatabaseHelper(string databasePath = "StudentSkillsTracker.db")
    {
        _connectionString = $"Data Source={databasePath}";
        InitializeDatabase();
    }

    /// <summary>
    /// Creates all required tables if they don't exist.
    /// </summary>
    private void InitializeDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Students (
                Id INTEGER PRIMARY KEY,
                Name TEXT NOT NULL,
                Email TEXT NOT NULL,
                EnrollmentDate TEXT NOT NULL,
                IsArchived INTEGER DEFAULT 0
            );

            CREATE TABLE IF NOT EXISTS Skills (
                Id INTEGER PRIMARY KEY,
                Name TEXT NOT NULL,
                Description TEXT NOT NULL,
                Category TEXT NOT NULL,
                PassingScore INTEGER NOT NULL
            );

            CREATE TABLE IF NOT EXISTS SkillProgress (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                StudentId INTEGER NOT NULL,
                SkillId INTEGER NOT NULL,
                CurrentScore INTEGER NOT NULL,
                Status INTEGER NOT NULL,
                LastUpdated TEXT NOT NULL,
                FOREIGN KEY (StudentId) REFERENCES Students(Id),
                FOREIGN KEY (SkillId) REFERENCES Skills(Id)
            );

            CREATE TABLE IF NOT EXISTS TrainingPrograms (
                Id INTEGER PRIMARY KEY,
                Name TEXT NOT NULL,
                Description TEXT NOT NULL,
                MinimumPassingPercentage INTEGER NOT NULL
            );

            CREATE TABLE IF NOT EXISTS ProgramSkills (
                ProgramId INTEGER NOT NULL,
                SkillId INTEGER NOT NULL,
                PRIMARY KEY (ProgramId, SkillId),
                FOREIGN KEY (ProgramId) REFERENCES TrainingPrograms(Id),
                FOREIGN KEY (SkillId) REFERENCES Skills(Id)
            );
        ";
        command.ExecuteNonQuery();
    }

    // ===================== STUDENTS =====================

    public void SaveStudent(Student student)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT OR REPLACE INTO Students (Id, Name, Email, EnrollmentDate, IsArchived)
            VALUES ($id, $name, $email, $date, 0)";
        command.Parameters.AddWithValue("$id", student.Id);
        command.Parameters.AddWithValue("$name", student.Name);
        command.Parameters.AddWithValue("$email", student.Email);
        command.Parameters.AddWithValue("$date", student.EnrollmentDate.ToString("o"));
        command.ExecuteNonQuery();

        // Save skill progress
        SaveStudentProgress(student);
    }

    private void SaveStudentProgress(Student student)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Delete existing progress for this student
        var deleteCmd = connection.CreateCommand();
        deleteCmd.CommandText = "DELETE FROM SkillProgress WHERE StudentId = $studentId";
        deleteCmd.Parameters.AddWithValue("$studentId", student.Id);
        deleteCmd.ExecuteNonQuery();

        // Insert current progress
        foreach (var progress in student.SkillProgressList)
        {
            var insertCmd = connection.CreateCommand();
            insertCmd.CommandText = @"
                INSERT INTO SkillProgress (StudentId, SkillId, CurrentScore, Status, LastUpdated)
                VALUES ($studentId, $skillId, $score, $status, $updated)";
            insertCmd.Parameters.AddWithValue("$studentId", student.Id);
            insertCmd.Parameters.AddWithValue("$skillId", progress.SkillId);
            insertCmd.Parameters.AddWithValue("$score", progress.CurrentScore);
            insertCmd.Parameters.AddWithValue("$status", (int)progress.Status);
            insertCmd.Parameters.AddWithValue("$updated", progress.LastUpdated.ToString("o"));
            insertCmd.ExecuteNonQuery();
        }
    }

    public void ArchiveStudent(int studentId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "UPDATE Students SET IsArchived = 1 WHERE Id = $id";
        command.Parameters.AddWithValue("$id", studentId);
        command.ExecuteNonQuery();
    }

    public void LoadStudents(StudentRepository repository)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name, Email, EnrollmentDate FROM Students WHERE IsArchived = 0 ORDER BY Id";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            int id = reader.GetInt32(0);
            string name = reader.GetString(1);
            string email = reader.GetString(2);

            var student = repository.AddWithId(id, name, email);
            LoadStudentProgress(student);
        }
    }

    private void LoadStudentProgress(Student student)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT SkillId, CurrentScore FROM SkillProgress WHERE StudentId = $studentId";
        command.Parameters.AddWithValue("$studentId", student.Id);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            int skillId = reader.GetInt32(0);
            int score = reader.GetInt32(1);
            student.UpdateSkillProgress(skillId, score);
        }
    }

    // ===================== SKILLS =====================

    public void SaveSkill(Skill skill)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT OR REPLACE INTO Skills (Id, Name, Description, Category, PassingScore)
            VALUES ($id, $name, $desc, $cat, $score)";
        command.Parameters.AddWithValue("$id", skill.Id);
        command.Parameters.AddWithValue("$name", skill.Name);
        command.Parameters.AddWithValue("$desc", skill.Description);
        command.Parameters.AddWithValue("$cat", skill.Category);
        command.Parameters.AddWithValue("$score", skill.PassingScore);
        command.ExecuteNonQuery();
    }

    public void LoadSkills(SkillRepository repository)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name, Description, Category, PassingScore FROM Skills ORDER BY Id";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            int id = reader.GetInt32(0);
            string name = reader.GetString(1);
            string desc = reader.GetString(2);
            string category = reader.GetString(3);
            int passingScore = reader.GetInt32(4);

            repository.AddWithId(id, name, desc, category, passingScore);
        }
    }

    // ===================== TRAINING PROGRAMS =====================

    public void SaveProgram(TrainingProgram program)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT OR REPLACE INTO TrainingPrograms (Id, Name, Description, MinimumPassingPercentage)
            VALUES ($id, $name, $desc, $min)";
        command.Parameters.AddWithValue("$id", program.Id);
        command.Parameters.AddWithValue("$name", program.Name);
        command.Parameters.AddWithValue("$desc", program.Description);
        command.Parameters.AddWithValue("$min", program.MinimumPassingPercentage);
        command.ExecuteNonQuery();

        // Save required skills
        SaveProgramSkills(program);
    }

    private void SaveProgramSkills(TrainingProgram program)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Delete existing
        var deleteCmd = connection.CreateCommand();
        deleteCmd.CommandText = "DELETE FROM ProgramSkills WHERE ProgramId = $programId";
        deleteCmd.Parameters.AddWithValue("$programId", program.Id);
        deleteCmd.ExecuteNonQuery();

        // Insert current
        foreach (var skillId in program.RequiredSkillIds)
        {
            var insertCmd = connection.CreateCommand();
            insertCmd.CommandText = "INSERT INTO ProgramSkills (ProgramId, SkillId) VALUES ($programId, $skillId)";
            insertCmd.Parameters.AddWithValue("$programId", program.Id);
            insertCmd.Parameters.AddWithValue("$skillId", skillId);
            insertCmd.ExecuteNonQuery();
        }
    }

    public void LoadPrograms(TrainingProgramRepository repository)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name, Description, MinimumPassingPercentage FROM TrainingPrograms ORDER BY Id";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            int id = reader.GetInt32(0);
            string name = reader.GetString(1);
            string desc = reader.GetString(2);
            int minPass = reader.GetInt32(3);

            var program = repository.AddWithId(id, name, desc, minPass);
            LoadProgramSkills(program);
        }
    }

    private void LoadProgramSkills(TrainingProgram program)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT SkillId FROM ProgramSkills WHERE ProgramId = $programId";
        command.Parameters.AddWithValue("$programId", program.Id);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            program.AddRequiredSkill(reader.GetInt32(0));
        }
    }

    // ===================== UTILITY =====================

    public int GetNextStudentId()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT COALESCE(MAX(Id), 0) + 1 FROM Students";
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public int GetNextSkillId()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT COALESCE(MAX(Id), 0) + 1 FROM Skills";
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public int GetNextProgramId()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT COALESCE(MAX(Id), 0) + 1 FROM TrainingPrograms";
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public bool HasData()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Students";
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }
}
