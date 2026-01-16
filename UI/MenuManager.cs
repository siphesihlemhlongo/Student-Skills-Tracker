using StudentSkillsTracker.Data;
using StudentSkillsTracker.Models;
using StudentSkillsTracker.Repositories;
using StudentSkillsTracker.Services;

namespace StudentSkillsTracker.UI;

/// <summary>
/// Manages the console menu interface for the application.
/// </summary>
public class MenuManager
{
    private readonly StudentRepository _studentRepository;
    private readonly SkillRepository _skillRepository;
    private readonly TrainingProgramRepository _programRepository;
    private readonly ProgressAnalyzer _progressAnalyzer;
    private readonly DatabaseHelper? _database;
    private bool _isRunning = true;

    public MenuManager(
        StudentRepository studentRepository,
        SkillRepository skillRepository,
        TrainingProgramRepository programRepository,
        ProgressAnalyzer progressAnalyzer,
        DatabaseHelper? database = null)
    {
        _studentRepository = studentRepository;
        _skillRepository = skillRepository;
        _programRepository = programRepository;
        _progressAnalyzer = progressAnalyzer;
        _database = database;
    }

    /// <summary>
    /// Starts the main menu loop.
    /// </summary>
    public void Run()
    {
        Console.Clear();
        PrintHeader();

        while (_isRunning)
        {
            ShowMainMenu();
            var choice = GetUserChoice(1, 6);
            
            switch (choice)
            {
                case 1:
                    StudentManagementMenu();
                    break;
                case 2:
                    SkillManagementMenu();
                    break;
                case 3:
                    UpdateProgressMenu();
                    break;
                case 4:
                    ViewReportsMenu();
                    break;
                case 5:
                    TrainingProgramMenu();
                    break;
                case 6:
                    _isRunning = false;
                    Console.WriteLine("\n Thank you for using Student Skills Tracker. Goodbye!");
                    break;
            }
        }
    }

    private void PrintHeader()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║           STUDENT SKILLS TRACKER                  ");
        Console.WriteLine("║       Training Programme Progress Management             ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
        Console.ResetColor();
        Console.WriteLine();
    }

    private void ShowMainMenu()
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("═══════════════ MAIN MENU ═══════════════");
        Console.ResetColor();
        Console.WriteLine("  1.Student Management");
        Console.WriteLine("  2.Skill Management");
        Console.WriteLine("  3.Update Student Progress");
        Console.WriteLine("  4.View Reports & Analytics");
        Console.WriteLine("  5.Training Programs");
        Console.WriteLine("  6.Exit");
        Console.WriteLine("═════════════════════════════════════════");
    }

    #region Student Management

    private void StudentManagementMenu()
    {
        bool inSubmenu = true;
        while (inSubmenu)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("──────── Student Management ────────");
            Console.ResetColor();
            Console.WriteLine("  1. View All Students");
            Console.WriteLine("  2. Add New Student");
            Console.WriteLine("  3. View Student Details");
            Console.WriteLine("  4. Update Student");
            Console.WriteLine("  5. Delete Student");
            Console.WriteLine("  6. Search Students");
            Console.WriteLine("  0. ← Back to Main Menu");
            Console.WriteLine("─────────────────────────────────────");

            var choice = GetUserChoice(0, 6);

            switch (choice)
            {
                case 1:
                    ViewAllStudents();
                    break;
                case 2:
                    AddNewStudent();
                    break;
                case 3:
                    ViewStudentDetails();
                    break;
                case 4:
                    UpdateStudent();
                    break;
                case 5:
                    DeleteStudent();
                    break;
                case 6:
                    SearchStudents();
                    break;
                case 0:
                    inSubmenu = false;
                    break;
            }
        }
    }

    private void ViewAllStudents()
    {
        var students = _studentRepository.GetAll();
        Console.WriteLine();
        
        if (students.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(" No students registered yet.");
            Console.ResetColor();
            return;
        }

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"All Students ({students.Count} total):");
        Console.ResetColor();
        Console.WriteLine("────────────────────────────────────────────");
        
        foreach (var student in students)
        {
            var progress = _progressAnalyzer.CalculateOverallProgress(student);
            Console.WriteLine($"  {student} | Progress: {progress:F1}%");
        }
    }

    private void AddNewStudent()
    {
        Console.WriteLine();
        Console.WriteLine("Add New Student");
        Console.WriteLine("────────────────────");
        
        Console.Write("Enter student name: ");
        var name = Console.ReadLine()?.Trim();
        
        if (string.IsNullOrWhiteSpace(name))
        {
            PrintError("Name cannot be empty.");
            return;
        }

        Console.Write("Enter student email: ");
        var email = Console.ReadLine()?.Trim();
        
        if (string.IsNullOrWhiteSpace(email))
        {
            PrintError("Email cannot be empty.");
            return;
        }

        var student = _studentRepository.Add(name, email);
        _database?.SaveStudent(student);
        PrintSuccess($"Student '{student.Name}' added successfully with ID: {student.Id}");
    }

    private void ViewStudentDetails()
    {
        var student = SelectStudent("View details for which student?");
        if (student == null) return;

        var summary = _progressAnalyzer.GenerateProgressSummary(student);
        
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"═══════════ Student Profile ═══════════");
        Console.ResetColor();
        Console.WriteLine($"  ID:         {student.Id}");
        Console.WriteLine($"  Name:       {student.Name}");
        Console.WriteLine($"  Email:      {student.Email}");
        Console.WriteLine($"  Enrolled:   {student.EnrollmentDate:yyyy-MM-dd}");
        Console.WriteLine($"  Progress:   {summary.OverallProgressPercentage:F1}%");
        Console.WriteLine();
        
        if (summary.CompletedSkills.Count > 0)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"   Completed Skills ({summary.CompletedSkills.Count}):");
            Console.ResetColor();
            foreach (var (skill, score) in summary.CompletedSkills)
            {
                Console.WriteLine($"      • {skill.Name}: {score}%");
            }
        }

        if (summary.InProgressSkills.Count > 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"   In Progress ({summary.InProgressSkills.Count}):");
            Console.ResetColor();
            foreach (var (skill, score) in summary.InProgressSkills)
            {
                Console.WriteLine($"      • {skill.Name}: {score}% (need {skill.PassingScore}%)");
            }
        }

        if (summary.NotStartedSkills.Count > 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"   Not Started ({summary.NotStartedSkills.Count}):");
            Console.ResetColor();
            foreach (var skill in summary.NotStartedSkills)
            {
                Console.WriteLine($"      • {skill.Name}");
            }
        }
        Console.WriteLine("═══════════════════════════════════════");
    }

    private void UpdateStudent()
    {
        var student = SelectStudent("Update which student?");
        if (student == null) return;

        Console.WriteLine();
        Console.WriteLine($"Updating: {student.Name}");
        Console.WriteLine("  1. Update Name");
        Console.WriteLine("  2. Update Email");
        Console.WriteLine("  0. Cancel");

        var choice = GetUserChoice(0, 2);

        switch (choice)
        {
            case 1:
                Console.Write("Enter new name: ");
                var name = Console.ReadLine()?.Trim();
                if (!string.IsNullOrWhiteSpace(name))
                {
                    student.UpdateName(name);
                    _database?.SaveStudent(student);
                    PrintSuccess($"Name updated to '{name}'");
                }
                break;
            case 2:
                Console.Write("Enter new email: ");
                var email = Console.ReadLine()?.Trim();
                if (!string.IsNullOrWhiteSpace(email))
                {
                    student.UpdateEmail(email);
                    _database?.SaveStudent(student);
                    PrintSuccess($"Email updated to '{email}'");
                }
                break;
        }
    }

    private void DeleteStudent()
    {
        var student = SelectStudent("Delete which student?");
        if (student == null) return;

        Console.Write($"Are you sure you want to delete '{student.Name}'? (y/n): ");
        var confirm = Console.ReadLine()?.Trim().ToLower();

        if (confirm == "y" || confirm == "yes")
        {
            _studentRepository.Delete(student.Id);
            _database?.ArchiveStudent(student.Id);
            PrintSuccess($"Student '{student.Name}' deleted (archived).");
        }
        else
        {
            Console.WriteLine("Deletion cancelled.");
        }
    }

    private void SearchStudents()
    {
        Console.Write("Enter search term: ");
        var term = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(term))
        {
            PrintError("Search term cannot be empty.");
            return;
        }

        var results = _studentRepository.SearchByName(term).ToList();

        if (results.Count == 0)
        {
            Console.WriteLine($"No students found matching '{term}'.");
        }
        else
        {
            Console.WriteLine($"\n Found {results.Count} student(s):");
            foreach (var student in results)
            {
                Console.WriteLine($"  {student}");
            }
        }
    }

    #endregion

    #region Skill Management

    private void SkillManagementMenu()
    {
        bool inSubmenu = true;
        while (inSubmenu)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("──────── Skill Management ────────");
            Console.ResetColor();
            Console.WriteLine("  1. View All Skills");
            Console.WriteLine("  2. View Skills by Category");
            Console.WriteLine("  3. Add New Skill");
            Console.WriteLine("  0. ← Back to Main Menu");
            Console.WriteLine("───────────────────────────────────");

            var choice = GetUserChoice(0, 3);

            switch (choice)
            {
                case 1:
                    ViewAllSkills();
                    break;
                case 2:
                    ViewSkillsByCategory();
                    break;
                case 3:
                    AddNewSkill();
                    break;
                case 0:
                    inSubmenu = false;
                    break;
            }
        }
    }

    private void ViewAllSkills()
    {
        var skills = _skillRepository.GetAll();
        Console.WriteLine();

        if (skills.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("No skills configured yet.");
            Console.ResetColor();
            return;
        }

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"All Skills ({skills.Count} total):");
        Console.ResetColor();
        Console.WriteLine("───────────────────────────────────────────────────────");

        foreach (var skill in skills)
        {
            Console.WriteLine($"  {skill}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"      {skill.Description}");
            Console.ResetColor();
        }
    }

    private void ViewSkillsByCategory()
    {
        var categories = _skillRepository.GetCategories().ToList();

        if (categories.Count == 0)
        {
            Console.WriteLine("No categories available.");
            return;
        }

        Console.WriteLine("\nAvailable categories:");
        for (int i = 0; i < categories.Count; i++)
        {
            Console.WriteLine($"  {i + 1}. {categories[i]}");
        }

        Console.Write("\nSelect category number: ");
        if (int.TryParse(Console.ReadLine(), out int choice) && choice >= 1 && choice <= categories.Count)
        {
            var category = categories[choice - 1];
            var skills = _skillRepository.GetByCategory(category).ToList();

            Console.WriteLine($"\n Skills in '{category}':");
            foreach (var skill in skills)
            {
                Console.WriteLine($"  {skill}");
            }
        }
    }

    private void AddNewSkill()
    {
        Console.WriteLine();
        Console.WriteLine(" Add New Skill");
        Console.WriteLine("─────────────────");

        Console.Write("Skill name: ");
        var name = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            PrintError("Name cannot be empty.");
            return;
        }

        Console.Write("Description: ");
        var description = Console.ReadLine()?.Trim() ?? "";

        Console.Write("Category: ");
        var category = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(category))
        {
            PrintError("Category cannot be empty.");
            return;
        }

        Console.Write("Passing score (0-100, default 70): ");
        var scoreInput = Console.ReadLine()?.Trim();
        int passingScore = 70;
        if (!string.IsNullOrWhiteSpace(scoreInput) && int.TryParse(scoreInput, out int parsed))
        {
            passingScore = Math.Clamp(parsed, 0, 100);
        }

        var skill = _skillRepository.Add(name, description, category, passingScore);
        _database?.SaveSkill(skill);
        PrintSuccess($"Skill '{skill.Name}' added with ID: {skill.Id}");
    }

    #endregion

    #region Update Progress

    private void UpdateProgressMenu()
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("──────── Update Progress ────────");
        Console.ResetColor();

        var student = SelectStudent("Select student to update:");
        if (student == null) return;

        var skill = SelectSkill("Select skill to update:");
        if (skill == null) return;

        var currentProgress = student.GetSkillProgress(skill.Id);
        if (currentProgress != null)
        {
            Console.WriteLine($"\nCurrent score for {skill.Name}: {currentProgress.CurrentScore}%");
        }
        else
        {
            Console.WriteLine($"\nNo progress recorded yet for {skill.Name}");
        }

        Console.Write("Enter new score (0-100): ");
        if (int.TryParse(Console.ReadLine(), out int score) && score >= 0 && score <= 100)
        {
            student.UpdateSkillProgress(skill.Id, score);
            _database?.SaveStudent(student);
            
            var status = score >= skill.PassingScore ? " PASSED" : " In Progress";
            PrintSuccess($"Progress updated: {skill.Name} = {score}% {status}");
        }
        else
        {
            PrintError("Invalid score. Please enter a number between 0 and 100.");
        }
    }

    #endregion

    #region Reports

    private void ViewReportsMenu()
    {
        bool inSubmenu = true;
        while (inSubmenu)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("──────── Reports & Analytics ────────");
            Console.ResetColor();
            Console.WriteLine("  1. Individual Progress Report");
            Console.WriteLine("  2. Certification Readiness Check");
            Console.WriteLine("  3. Class Overview Statistics");
            Console.WriteLine("  4. Skills Needing Attention");
            Console.WriteLine("  0. ← Back to Main Menu");
            Console.WriteLine("──────────────────────────────────────");

            var choice = GetUserChoice(0, 4);

            switch (choice)
            {
                case 1:
                    ShowIndividualReport();
                    break;
                case 2:
                    CheckCertificationReadiness();
                    break;
                case 3:
                    ShowClassStatistics();
                    break;
                case 4:
                    ShowSkillsNeedingAttention();
                    break;
                case 0:
                    inSubmenu = false;
                    break;
            }
        }
    }

    private void ShowIndividualReport()
    {
        var student = SelectStudent("Generate report for which student?");
        if (student == null) return;

        var summary = _progressAnalyzer.GenerateProgressSummary(student);

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔══════════════════════════════════════════╗");
        Console.WriteLine($"║    PROGRESS REPORT: {student.Name,-20} ║");
        Console.WriteLine("╚══════════════════════════════════════════╝");
        Console.ResetColor();

        // Progress bar
        Console.Write("  Overall Progress: ");
        DrawProgressBar(summary.OverallProgressPercentage);
        Console.WriteLine($" {summary.OverallProgressPercentage:F1}%");
        Console.WriteLine();

        Console.WriteLine($"   Summary:");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"       Completed:    {summary.CompletedSkills.Count}");
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"       In Progress:  {summary.InProgressSkills.Count}");
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"       Not Started:  {summary.NotStartedSkills.Count}");
        Console.ResetColor();

        Console.WriteLine();
        Console.WriteLine("  Press any key to continue...");
        Console.ReadKey(true);
    }

    private void CheckCertificationReadiness()
    {
        var student = SelectStudent("Check readiness for which student?");
        if (student == null) return;

        var program = SelectProgram("Check readiness for which program?");
        if (program == null) return;

        var readiness = _progressAnalyzer.CheckCertificationReadiness(student, program);

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔══════════════════════════════════════════════════╗");
        Console.WriteLine("║          CERTIFICATION READINESS REPORT          ║");
        Console.WriteLine("╚══════════════════════════════════════════════════╝");
        Console.ResetColor();

        Console.WriteLine($"  Student:  {student.Name}");
        Console.WriteLine($"  Program:  {program.Name}");
        Console.WriteLine();

        Console.Write("  Readiness: ");
        DrawProgressBar(readiness.ReadinessPercentage);
        Console.WriteLine($" {readiness.ReadinessPercentage:F1}%");
        Console.WriteLine();

        if (readiness.IsReadyForCertification)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("   STATUS: READY FOR CERTIFICATION! ");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(" STATUS: NOT YET READY");
            Console.ResetColor();
            Console.WriteLine($"  (Requires {program.MinimumPassingPercentage}% completion)");
        }

        Console.WriteLine();

        if (readiness.CompletedSkills.Count > 0)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  Completed ({readiness.CompletedSkills.Count}):");
            Console.ResetColor();
            foreach (var skill in readiness.CompletedSkills)
            {
                Console.WriteLine($"      • {skill.Name}");
            }
        }

        if (readiness.IncompleteSkills.Count > 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  Incomplete ({readiness.IncompleteSkills.Count}):");
            Console.ResetColor();
            foreach (var skill in readiness.IncompleteSkills)
            {
                var progress = student.GetSkillProgress(skill.Id);
                var current = progress?.CurrentScore ?? 0;
                Console.WriteLine($"      • {skill.Name}: {current}% / {skill.PassingScore}%");
            }
        }

        Console.WriteLine();
        Console.WriteLine("  Press any key to continue...");
        Console.ReadKey(true);
    }

    private void ShowClassStatistics()
    {
        var students = _studentRepository.GetAll();

        if (students.Count == 0)
        {
            Console.WriteLine("\n No students to analyze.");
            return;
        }

        var stats = _progressAnalyzer.CalculateClassStatistics(students);

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔══════════════════════════════════════════╗");
        Console.WriteLine("║         CLASS OVERVIEW STATISTICS        ║");
        Console.WriteLine("╚══════════════════════════════════════════╝");
        Console.ResetColor();

        Console.WriteLine($"  Total Students:    {stats.TotalStudents}");
        Console.WriteLine($"  Average Progress:  {stats.AverageProgress:F1}%");
        Console.WriteLine($"  Highest Progress:  {stats.HighestProgress:F1}%");
        Console.WriteLine($"  Lowest Progress:   {stats.LowestProgress:F1}%");
        Console.WriteLine();

        if (stats.TopPerformers.Count > 0)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(" Top Performers:");
            Console.ResetColor();
            int rank = 1;
            foreach (var (student, progress) in stats.TopPerformers)
            {
                Console.WriteLine($"      {rank}. {student.Name} - {progress:F1}%");
                rank++;
            }
        }

        if (stats.StudentsNeedingSupport.Count > 0)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  Students Needing Support (< 50%):");
            Console.ResetColor();
            foreach (var (student, progress) in stats.StudentsNeedingSupport)
            {
                Console.WriteLine($"      • {student.Name} - {progress:F1}%");
            }
        }

        Console.WriteLine();
        Console.WriteLine("  Press any key to continue...");
        Console.ReadKey(true);
    }

    private void ShowSkillsNeedingAttention()
    {
        var student = SelectStudent("Check skills for which student?");
        if (student == null) return;

        var skillsNeeding = _progressAnalyzer.GetSkillsNeedingAttention(student, 5).ToList();

        Console.WriteLine();

        if (skillsNeeding.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  All skills are at passing level!");
            Console.ResetColor();
            return;
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"   Skills Needing Attention for {student.Name}:");
        Console.ResetColor();
        Console.WriteLine("  ─────────────────────────────────────────");

        foreach (var (skill, score, gap) in skillsNeeding)
        {
            Console.Write($"  • {skill.Name}: ");
            DrawProgressBar(score, 20);
            Console.WriteLine($" {score}% (need +{gap}%)");
        }
    }

    #endregion

    #region Training Programs

    private void TrainingProgramMenu()
    {
        bool inSubmenu = true;
        while (inSubmenu)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine("──────── Training Programs ────────");
            Console.ResetColor();
            Console.WriteLine("  1. View All Programs");
            Console.WriteLine("  2. View Program Details");
            Console.WriteLine("  3. Add New Program");
            Console.WriteLine("  4. Add Skill to Program");
            Console.WriteLine("  0. ← Back to Main Menu");
            Console.WriteLine("────────────────────────────────────");

            var choice = GetUserChoice(0, 4);

            switch (choice)
            {
                case 1:
                    ViewAllPrograms();
                    break;
                case 2:
                    ViewProgramDetails();
                    break;
                case 3:
                    AddNewProgram();
                    break;
                case 4:
                    AddSkillToProgram();
                    break;
                case 0:
                    inSubmenu = false;
                    break;
            }
        }
    }

    private void ViewAllPrograms()
    {
        var programs = _programRepository.GetAll();

        if (programs.Count == 0)
        {
            Console.WriteLine("\n No training programs configured.");
            return;
        }

        Console.WriteLine($"\n🎓 Training Programs ({programs.Count}):");
        foreach (var program in programs)
        {
            Console.WriteLine($"  {program}");
        }
    }

    private void ViewProgramDetails()
    {
        var program = SelectProgram("View details for which program?");
        if (program == null) return;

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine($"═══════════ {program.Name} ═══════════");
        Console.ResetColor();
        Console.WriteLine($"  Description: {program.Description}");
        Console.WriteLine($"  Minimum Passing: {program.MinimumPassingPercentage}%");
        Console.WriteLine($"  Required Skills: {program.RequiredSkillIds.Count}");
        Console.WriteLine();

        foreach (var skillId in program.RequiredSkillIds)
        {
            var skill = _skillRepository.GetById(skillId);
            if (skill != null)
            {
                Console.WriteLine($"    • {skill.Name} ({skill.Category}) - Pass: {skill.PassingScore}%");
            }
        }
    }

    private void AddNewProgram()
    {
        Console.WriteLine("\n Add New Training Program");
        Console.WriteLine("────────────────────────────");

        Console.Write("Program name: ");
        var name = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            PrintError("Name cannot be empty.");
            return;
        }

        Console.Write("Description: ");
        var description = Console.ReadLine()?.Trim() ?? "";

        Console.Write("Minimum passing percentage (default 100): ");
        var passInput = Console.ReadLine()?.Trim();
        int minPass = 100;
        if (!string.IsNullOrWhiteSpace(passInput) && int.TryParse(passInput, out int parsed))
        {
            minPass = Math.Clamp(parsed, 0, 100);
        }

        var program = _programRepository.Add(name, description, minPass);
        _database?.SaveProgram(program);
        PrintSuccess($"Program '{program.Name}' created with ID: {program.Id}");
    }

    private void AddSkillToProgram()
    {
        var program = SelectProgram("Add skill to which program?");
        if (program == null) return;

        var skill = SelectSkill("Which skill to add?");
        if (skill == null) return;

        program.AddRequiredSkill(skill.Id);
        _database?.SaveProgram(program);
        PrintSuccess($"Added '{skill.Name}' to '{program.Name}'");
    }

    #endregion

    #region Helper Methods

    private int GetUserChoice(int min, int max)
    {
        while (true)
        {
            Console.Write("\nEnter choice: ");
            if (int.TryParse(Console.ReadLine(), out int choice) && choice >= min && choice <= max)
            {
                return choice;
            }
            PrintError($"Please enter a number between {min} and {max}.");
        }
    }

    private Student? SelectStudent(string prompt)
    {
        var students = _studentRepository.GetAll();

        if (students.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("\n No students available.");
            Console.ResetColor();
            return null;
        }

        Console.WriteLine($"\n{prompt}");
        for (int i = 0; i < students.Count; i++)
        {
            Console.WriteLine($"  {i + 1}. {students[i].Name}");
        }
        Console.WriteLine("  0. Cancel");

        var choice = GetUserChoice(0, students.Count);
        return choice == 0 ? null : students[choice - 1];
    }

    private Skill? SelectSkill(string prompt)
    {
        var skills = _skillRepository.GetAll();

        if (skills.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("\n No skills available.");
            Console.ResetColor();
            return null;
        }

        Console.WriteLine($"\n{prompt}");
        for (int i = 0; i < skills.Count; i++)
        {
            Console.WriteLine($"  {i + 1}. {skills[i].Name} ({skills[i].Category})");
        }
        Console.WriteLine("  0. Cancel");

        var choice = GetUserChoice(0, skills.Count);
        return choice == 0 ? null : skills[choice - 1];
    }

    private TrainingProgram? SelectProgram(string prompt)
    {
        var programs = _programRepository.GetAll();

        if (programs.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("\n No training programs available.");
            Console.ResetColor();
            return null;
        }

        Console.WriteLine($"\n{prompt}");
        for (int i = 0; i < programs.Count; i++)
        {
            Console.WriteLine($"  {i + 1}. {programs[i].Name}");
        }
        Console.WriteLine("  0. Cancel");

        var choice = GetUserChoice(0, programs.Count);
        return choice == 0 ? null : programs[choice - 1];
    }

    private void DrawProgressBar(double percentage, int width = 30)
    {
        int filled = (int)(percentage / 100 * width);
        int empty = width - filled;

        Console.Write("[");
        
        if (percentage >= 70)
            Console.ForegroundColor = ConsoleColor.Green;
        else if (percentage >= 40)
            Console.ForegroundColor = ConsoleColor.Yellow;
        else
            Console.ForegroundColor = ConsoleColor.Red;

        Console.Write(new string('█', filled));
        Console.ResetColor();
        Console.Write(new string('░', empty));
        Console.Write("]");
    }

    private void PrintSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n {message}");
        Console.ResetColor();
    }

    private void PrintError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\n {message}");
        Console.ResetColor();
    }

    #endregion
}
