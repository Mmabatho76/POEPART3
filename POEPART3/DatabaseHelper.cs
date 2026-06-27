using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POEPART3
{
    public static class DatabaseHelper
    {
        private const string DbFileName = "AzeeBotTasks.db";

        public static void InitializeDatabase()
        {
            using var conn = new SqliteConnection($"Data Source={DbFileName}");
            conn.Open();
            string createTable = @"
                CREATE TABLE IF NOT EXISTS Tasks (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Title TEXT NOT NULL,
                    Description TEXT,
                    ReminderDate TEXT,
                    IsCompleted INTEGER DEFAULT 0
                );";
            using var cmd = new SqliteCommand(createTable, conn);
            cmd.ExecuteNonQuery();
        }

        public static void AddTask(string title, string description, string reminderDate = null)
        {
            using var conn = new SqliteConnection($"Data Source={DbFileName}");
            conn.Open();
            using var cmd = new SqliteCommand(
                "INSERT INTO Tasks (Title, Description, ReminderDate) VALUES (@t, @d, @r);", conn);
            cmd.Parameters.AddWithValue("@t", title);
            cmd.Parameters.AddWithValue("@d", description);
            cmd.Parameters.AddWithValue("@r", reminderDate ?? (object)DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        public static List<TaskItem> GetAllTasks()
        {
            List<TaskItem> tasks = new List<TaskItem>();
            using var conn = new SqliteConnection($"Data Source={DbFileName}");
            conn.Open();
            using var cmd = new SqliteCommand("SELECT Id, Title, Description, ReminderDate, IsCompleted FROM Tasks ORDER BY Id DESC", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                tasks.Add(new TaskItem
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    ReminderDate = reader.IsDBNull(3) ? null : reader.GetString(3),
                    IsCompleted = reader.GetInt32(4) == 1
                });
            }
            return tasks;
        }

        public static bool MarkTaskComplete(int taskId)
        {
            using var conn = new SqliteConnection($"Data Source={DbFileName}");
            conn.Open();
            using var cmd = new SqliteCommand("UPDATE Tasks SET IsCompleted = 1 WHERE Id = @id", conn);
            cmd.Parameters.AddWithValue("@id", taskId);
            return cmd.ExecuteNonQuery() > 0;
        }

        public static bool DeleteTask(int taskId)
        {
            using var conn = new SqliteConnection($"Data Source={DbFileName}");
            conn.Open();
            using var cmd = new SqliteCommand("DELETE FROM Tasks WHERE Id = @id", conn);
            cmd.Parameters.AddWithValue("@id", taskId);
            return cmd.ExecuteNonQuery() > 0;
        }
    }

    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ReminderDate { get; set; }
        public bool IsCompleted { get; set; }
    }

}
