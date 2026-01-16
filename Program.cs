using StudentSkillsTracker.Data;
using StudentSkillsTracker.Repositories;
using StudentSkillsTracker.Services;
using StudentSkillsTracker.UI;

namespace StudentSkillsTracker;

/// <summary>
/// Main entry point for the Student Skills Tracker application.
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        // Initialize database helper
        var database = new DatabaseHelper();

        // Initialize repositories
        var studentRepository = new StudentRepository();
        var skillRepository = new SkillRepository();
        var programRepository = new TrainingProgramRepository();

        // Check if database has existing data
        if (database.HasData())
        {
            // Load existing data from database
            Console.WriteLine("Loading saved data...");
            database.LoadSkills(skillRepository);
            database.LoadStudents(studentRepository);
            database.LoadPrograms(programRepository);
            Console.WriteLine($"Loaded {studentRepository.Count} students, {skillRepository.Count} skills, {programRepository.Count} programs.\n");
        }
        else
        {
            // First run - load sample data and save to database
            Console.WriteLine("First run - creating sample data...");
            LoadSampleData(studentRepository, skillRepository, programRepository, database);
            Console.WriteLine("Sample data created and saved to database.\n");
        }

        // Initialize services
        var progressAnalyzer = new ProgressAnalyzer(skillRepository);

        // Create and run the menu manager
        var menuManager = new MenuManager(
            studentRepository,
            skillRepository,
            programRepository,
            progressAnalyzer,
            database  // Pass database for saving changes
        );

        menuManager.Run();
    }

    /// <summary>
    /// Loads sample data to demonstrate the application's features.
    /// </summary>
    static void LoadSampleData(
        StudentRepository studentRepo,
        SkillRepository skillRepo,
        TrainingProgramRepository programRepo,
        DatabaseHelper database)
    {
        // Add sample skills organized by category
        // Programming Skills
        var csharp = skillRepo.Add("C# Fundamentals", "Core C# programming concepts including syntax, OOP, and LINQ", "Programming", 70);
        var dotnet = skillRepo.Add(".NET Framework", "Understanding of .NET runtime, libraries, and tools", "Programming", 70);
        var sql = skillRepo.Add("SQL & Databases", "Database design, SQL queries, and data management", "Programming", 65);
        var git = skillRepo.Add("Git Version Control", "Source control with Git, branching, and collaboration", "Programming", 60);

        // Web Development Skills
        var html = skillRepo.Add("HTML/CSS", "Web markup and styling fundamentals", "Web Development", 70);
        var javascript = skillRepo.Add("JavaScript", "Client-side scripting and DOM manipulation", "Web Development", 70);
        var aspnet = skillRepo.Add("ASP.NET Core", "Building web applications with ASP.NET Core MVC and Web API", "Web Development", 75);

        // Soft Skills
        var communication = skillRepo.Add("Communication", "Clear written and verbal communication skills", "Soft Skills", 70);
        var teamwork = skillRepo.Add("Teamwork", "Collaborative work and team dynamics", "Soft Skills", 65);
        var problemSolving = skillRepo.Add("Problem Solving", "Analytical thinking and solution development", "Soft Skills", 70);

        // Save all skills to database
        foreach (var skill in skillRepo.GetAll())
        {
            database.SaveSkill(skill);
        }

        // Add sample students with varied progress
        var student1 = studentRepo.Add("Sarah Johnson", "sarah.johnson@email.com");
        student1.UpdateSkillProgress(csharp.Id, 85);
        student1.UpdateSkillProgress(dotnet.Id, 78);
        student1.UpdateSkillProgress(sql.Id, 72);
        student1.UpdateSkillProgress(git.Id, 90);
        student1.UpdateSkillProgress(html.Id, 88);
        student1.UpdateSkillProgress(javascript.Id, 65);
        student1.UpdateSkillProgress(communication.Id, 82);

        var student2 = studentRepo.Add("Michael Chen", "michael.chen@email.com");
        student2.UpdateSkillProgress(csharp.Id, 92);
        student2.UpdateSkillProgress(dotnet.Id, 88);
        student2.UpdateSkillProgress(sql.Id, 95);
        student2.UpdateSkillProgress(git.Id, 85);
        student2.UpdateSkillProgress(aspnet.Id, 80);
        student2.UpdateSkillProgress(problemSolving.Id, 90);

        var student3 = studentRepo.Add("Emily Davis", "emily.davis@email.com");
        student3.UpdateSkillProgress(csharp.Id, 55);
        student3.UpdateSkillProgress(dotnet.Id, 45);
        student3.UpdateSkillProgress(html.Id, 70);
        student3.UpdateSkillProgress(javascript.Id, 60);
        student3.UpdateSkillProgress(teamwork.Id, 85);

        var student4 = studentRepo.Add("James Wilson", "james.wilson@email.com");
        student4.UpdateSkillProgress(csharp.Id, 78);
        student4.UpdateSkillProgress(sql.Id, 82);
        student4.UpdateSkillProgress(git.Id, 75);
        student4.UpdateSkillProgress(communication.Id, 88);
        student4.UpdateSkillProgress(teamwork.Id, 90);
        student4.UpdateSkillProgress(problemSolving.Id, 85);

        var student5 = studentRepo.Add("Lisa Thompson", "lisa.thompson@email.com");
        student5.UpdateSkillProgress(html.Id, 95);
        student5.UpdateSkillProgress(javascript.Id, 88);
        student5.UpdateSkillProgress(aspnet.Id, 72);

        // Save all students to database
        foreach (var student in studentRepo.GetAll())
        {
            database.SaveStudent(student);
        }

        // Add sample training programs
        var fullStackProgram = programRepo.Add(
            "Full Stack Developer",
            "Complete web development certification covering front-end, back-end, and databases",
            100
        );
        fullStackProgram.AddRequiredSkill(csharp.Id);
        fullStackProgram.AddRequiredSkill(dotnet.Id);
        fullStackProgram.AddRequiredSkill(sql.Id);
        fullStackProgram.AddRequiredSkill(html.Id);
        fullStackProgram.AddRequiredSkill(javascript.Id);
        fullStackProgram.AddRequiredSkill(aspnet.Id);
        fullStackProgram.AddRequiredSkill(git.Id);

        var backendProgram = programRepo.Add(
            "Backend Developer",
            "Server-side development with C# and databases",
            100
        );
        backendProgram.AddRequiredSkill(csharp.Id);
        backendProgram.AddRequiredSkill(dotnet.Id);
        backendProgram.AddRequiredSkill(sql.Id);
        backendProgram.AddRequiredSkill(aspnet.Id);

        var foundationsProgram = programRepo.Add(
            "Programming Foundations",
            "Entry-level programming concepts and tools",
            80
        );
        foundationsProgram.AddRequiredSkill(csharp.Id);
        foundationsProgram.AddRequiredSkill(git.Id);
        foundationsProgram.AddRequiredSkill(problemSolving.Id);

        // Save all programs to database
        foreach (var program in programRepo.GetAll())
        {
            database.SaveProgram(program);
        }
    }
}
