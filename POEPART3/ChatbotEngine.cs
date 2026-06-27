using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;   // REQUIRED NUGET INSTALLED FIRST

namespace POEPART3
{
    // --- ONLY ONE definition each ---
    public class QuizQuestion
    {
        public string QuestionText { get; set; }
        public List<string> Options { get; set; }
        public string CorrectAnswer { get; set; }
        public string Explanation { get; set; }
    }

    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ReminderDate { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class ActivityLogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Action { get; set; }
    }

    public static class DatabaseHelper
    {
        private const string DbFile = "AzeeBotTasks.db";

        public static void InitializeDatabase()
        {
            using var conn = new SqliteConnection($"Data Source={DbFile}");
            conn.Open();
            string sql = @"
                CREATE TABLE IF NOT EXISTS Tasks (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Title TEXT NOT NULL,
                    Description TEXT,
                    ReminderDate TEXT,
                    IsCompleted INTEGER DEFAULT 0
                );";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.ExecuteNonQuery();
        }

        public static void AddTask(string title, string description, string reminder = null)
        {
            using var conn = new SqliteConnection($"Data Source={DbFile}");
            conn.Open();
            using var cmd = new SqliteCommand(
                "INSERT INTO Tasks(Title,Description,ReminderDate) VALUES(@t,@d,@r);", conn);
            cmd.Parameters.AddWithValue("@t", title);
            cmd.Parameters.AddWithValue("@d", description);
            cmd.Parameters.AddWithValue("@r", reminder ?? (object)DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        public static List<TaskItem> GetAllTasks()
        {
            var list = new List<TaskItem>();
            using var conn = new SqliteConnection($"Data Source={DbFile}");
            conn.Open();
            using var rd = new SqliteCommand("SELECT * FROM Tasks ORDER BY Id DESC", conn).ExecuteReader();
            while (rd.Read())
            {
                list.Add(new TaskItem
                {
                    Id = rd.GetInt32(0),
                    Title = rd.GetString(1),
                    Description = rd.IsDBNull(2) ? "" : rd.GetString(2),
                    ReminderDate = rd.IsDBNull(3) ? null : rd.GetString(3),
                    IsCompleted = rd.GetInt32(4) == 1
                });
            }
            return list;
        }

        public static bool MarkComplete(int id)
        {
            using var conn = new SqliteConnection($"Data Source={DbFile}");
            conn.Open();
            using var cmd = new SqliteCommand("UPDATE Tasks SET IsCompleted=1 WHERE Id=@id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            return cmd.ExecuteNonQuery() > 0;
        }

        public static bool DeleteTask(int id)
        {
            using var conn = new SqliteConnection($"Data Source={DbFile}");
            conn.Open();
            using var cmd = new SqliteCommand("DELETE FROM Tasks WHERE Id=@id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            return cmd.ExecuteNonQuery() > 0;
        }
    }

    // === ChatbotEngine — CLEAN, NO extra leftovers ===
    public delegate string BotResponseDelegate(string input);

    public class ChatbotEngine
    {
        private readonly string userName;
        private readonly Dictionary<string, string> generalResponses;
        private readonly Dictionary<string, string> securityTopics;

        // Flow states
        private bool awaitingTaskDescription;
        private string pendingTaskTitle;
        private bool awaitingReminder, awaitingReminderDate;
        private bool quizActive;
        private int currentQuestionIndex, score;
        private List<QuizQuestion> quizQuestions;
        private readonly List<ActivityLogEntry> activityLog = new List<ActivityLogEntry>();
        private readonly Random rnd = new Random();

        public ChatbotEngine(string userName)
        {
            this.userName = string.IsNullOrWhiteSpace(userName) ? "Friend" : userName;
            DatabaseHelper.InitializeDatabase();

            generalResponses = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"hello", $"Hello {userName}! Ready to learn cybersecurity?"},
                {"hi", $"Hi {userName}! How can I help?"},
                {"how are you", "All good — ready to teach safe habits!"},
                {"who are you", "I am AzeeBot — cybersecurity assistant"},
                {"thank", "You are welcome. Stay safe online"},
                {"bye", $"Goodbye {userName}! Come back soon"}
            };

            securityTopics = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"phishing", "Fake messages stealing information — never click unknown links"},
                {"password", "Long + mix letters/numbers/symbols + do not reuse"},
                {"2fa", "Two‑factor authentication = extra safe layer"},
                {"otp", "OTP is secret — never share it!"},
                {"malware", "Bad software — only download from trusted places"}
            };

            InitializeQuizQuestions();
        }

        public string GetResponse(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "Please type something…";
            input = input.ToLower().Trim();

            // Handle active flows FIRST
            if (quizActive) return HandleQuizInput(input);
            if (awaitingTaskDescription) return HandleTaskDescription(input);
            if (awaitingReminder) return HandleReminderChoice(input);
            if (awaitingReminderDate) return HandleReminderDate(input);

            LogActivity("User: " + input);

            if (input == "exit" || input == "quit")
                return $"Bye {userName}! Stay safe.";

            // Commands
            if (Matches(input, "quiz", "start quiz")) return StartQuiz();
            if (Matches(input, "add task", "new task")) return StartNewTask(input);
            if (Matches(input, "show tasks", "list tasks")) return ListAllTasks();
            if (Matches(input, "log", "history")) return GetActivityLog();

            if (Regex.IsMatch(input, @"mark.*complete\s+\d+"))
            {
                var id = int.Parse(Regex.Match(input, @"\d+").Value);
                return DatabaseHelper.MarkComplete(id) ? "Marked done" : "Not found";
            }
            if (Regex.IsMatch(input, @"delete.*task\s+\d+"))
            {
                var id = int.Parse(Regex.Match(input, @"\d+").Value);
                return DatabaseHelper.DeleteTask(id) ? "Deleted" : "Not found";
            }

            foreach (var kv in generalResponses)
                if (input.Contains(kv.Key)) return kv.Value;
            foreach (var kv in securityTopics)
                if (input.Contains(kv.Key)) return kv.Value;

            return "Ask about security, tasks, quiz or help";
        }

        private bool Matches(string text, params string[] keys)
            => keys.Any(k => text.Contains(k, StringComparison.OrdinalIgnoreCase));

        private void LogActivity(string act)
        {
            activityLog.Insert(0, new ActivityLogEntry { Timestamp = DateTime.Now, Action = act });
            if (activityLog.Count > 10) activityLog.RemoveAt(activityLog.Count - 1);
        }
        private string GetActivityLog()
            => activityLog.Any()
                ? "Log:\n" + string.Join("\n", activityLog.Select((x, i) => $"{i + 1}. {x.Timestamp:HH:mm} {x.Action}")
                : "No activity yet");

        // Task logic
        private string StartNewTask(string input)
        {
            pendingTaskTitle = Regex.Replace(input, @"add task|new task", "", RegexOptions.IgnoreCase).Trim();
            if (string.IsNullOrWhiteSpace(pendingTaskTitle)) return "What is the task?";
            awaitingTaskDescription = true;
            return $"Task: {pendingTaskTitle} — add description:";
        }
        private string HandleTaskDescription(string txt)
        {
            awaitingTaskDescription = false;
            DatabaseHelper.AddTask(pendingTaskTitle, txt);
            LogActivity("Added task: " + pendingTaskTitle);
            awaitingReminder = true;
            return "Saved. Set reminder? yes/no";
        }
        private string HandleReminderChoice(string txt)
        {
            awaitingReminder = false;
            if (txt.StartsWith("y"))
            { awaitingReminderDate = true; return "When? tomorrow / in 3 days / 2026‑07‑05"; }
            return "OK — no reminder";
        }
        private string HandleReminderDate(string txt)
        {
            awaitingReminderDate = false;
            var dt = ParseRem(txt);
            DatabaseHelper.AddTask(pendingTaskTitle, "", dt.ToString("yyyy-MM-dd"));
            return $"Reminder set for: {dt:yyyy-MM-dd}";
        }
        private DateTime ParseRem(string txt)
        {
            if (txt.Contains("tomorrow")) return DateTime.Now.AddDays(1);
            var m = Regex.Match(txt, @"in (\d+) days");
            if (m.Success) return DateTime.Now.AddDays(int.Parse(m.Groups[1].Value));
            return DateTime.TryParse(txt, out var d) ? d : DateTime.Now.AddDays(7);
        }
        private string ListAllTasks()
        {
            var list = DatabaseHelper.GetAllTasks();
            return list.Any()
                ? "Tasks:\n" + string.Join("\n", list.Select(x =>
                    $"{x.Id}. {x.Title} | Rem:{x.ReminderDate ?? "-"} | {(x.IsCompleted ? "Done" : "Pending")}"))
                : "No tasks saved";
        }

        // Quiz logic
        private void InitializeQuizQuestions()
        {
            quizQuestions = new List<QuizQuestion>
            {
                new QuizQuestion
                {
                    QuestionText = "What to do on suspicious email?",
                    Options = new List<string>{"A Reply","B Delete","C Report phish","D Ignore"},
                    CorrectAnswer = "C",
                    Explanation = "Report helps block scammers"
                },
                new QuizQuestion
                {
                    QuestionText = "Strongest password style?",
                    Options = new List<string>{"A simple name","B 123456","C mix letters+numbers+symbols","D qwerty"},
                    CorrectAnswer = "C",
                    Explanation = "Long & mixed = hard to crack"
                }
            };
        }
        private string StartQuiz()
        { quizActive = true; currentQuestionIndex = 0; score = 0; return "Quiz start — answer A/B/C/D\n" + GetCurrentQ(); }
        private string GetCurrentQ()
        {
            var q = quizQuestions[currentQuestionIndex];
            return $"Q{currentQuestionIndex + 1}/{quizQuestions.Count}:\n{q.QuestionText}\n{string.Join("\n", q.Options)}";
        }
        private string HandleQuizInput(string ans)
        {
            var q = quizQuestions[currentQuestionIndex];
            bool ok = ans.Trim().Equals(q.CorrectAnswer, StringComparison.OrdinalIgnoreCase);
            score += ok ? 1 : 0;
            string res = ok ? "Correct" : $"Wrong — correct answer: {q.CorrectAnswer}";
            res += $" | {q.Explanation}\n";
            currentQuestionIndex++;
            if (currentQuestionIndex >= quizQuestions.Count)
            { quizActive = false; res += $"\nFinal Score: {score}/{quizQuestions.Count}"; }
            else res += GetCurrentQ();
            return res;
        }
    }
}