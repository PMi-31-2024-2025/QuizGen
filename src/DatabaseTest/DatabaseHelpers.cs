using Npgsql;

public class DatabaseHelper
{
    private readonly string _connectionString;

    public DatabaseHelper(string connectionString)
    {
        _connectionString = connectionString;
    }

    public void InitializeDatabase(string adminConnectionString)
    {
        using (var adminConnection = new NpgsqlConnection(adminConnectionString))
        {
            adminConnection.Open();

            string checkDbQuery = "SELECT 1 FROM pg_database WHERE datname = 'quizgen'";
            using (var checkCommand = new NpgsqlCommand(checkDbQuery, adminConnection))
            {
                var result = checkCommand.ExecuteScalar();
                if (result == null)
                {
                    Console.WriteLine("Database 'quizgen' does not exist. Creating...");
                    string createDbQuery = "CREATE DATABASE quizgen";
                    using (var createCommand = new NpgsqlCommand(createDbQuery, adminConnection))
                    {
                        createCommand.ExecuteNonQuery();
                        Console.WriteLine("Database 'quizgen' created successfully.");
                    }
                }
            }
        }

        RunSqlScript("CreateDatabase.sql");
    }

    private void RunSqlScript(string scriptPath)
    {
        if (!File.Exists(scriptPath))
        {
            Console.WriteLine($"File '{scriptPath}' not found.");
            return;
        }

        string script = File.ReadAllText(scriptPath);

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            using (var command = new NpgsqlCommand(script, connection))
            {
                command.ExecuteNonQuery();
                Console.WriteLine("Database schema created successfully.");
            }
        }
    }

    public void DisplayUsers()
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            string query = "SELECT * FROM users";
            using (var command = new NpgsqlCommand(query, connection))
            using (var reader = command.ExecuteReader())
            {
                Console.WriteLine("Users:");
                while (reader.Read())
                {
                    Console.WriteLine($"ID: {reader["id"]}, Name: {reader["name"]}, Username: {reader["username"]}");
                }
            }
        }
    }

    public void DisplayQuizzes()
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            string query = "SELECT * FROM quizzes";
            using (var command = new NpgsqlCommand(query, connection))
            using (var reader = command.ExecuteReader())
            {
                Console.WriteLine("Quizzes:");
                while (reader.Read())
                {
                    Console.WriteLine($"ID: {reader["id"]}, Author ID: {reader["author_id"]}, Prompt: {reader["prompt"]}, " +
                                      $"Difficulty: {reader["difficulty"]}, Num Questions: {reader["num_questions"]}");
                }
            }
        }
    }

    public void DisplayQuestions()
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            string query = "SELECT * FROM questions";
            using (var command = new NpgsqlCommand(query, connection))
            using (var reader = command.ExecuteReader())
            {
                Console.WriteLine("Questions:");
                while (reader.Read())
                {
                    Console.WriteLine($"ID: {reader["id"]}, Quiz ID: {reader["quiz_id"]}, Type: {reader["type"]}, Question: {reader["question"]}");
                }
            }
        }
    }

    public void DisplayAnswers()
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            string query = "SELECT * FROM answers";
            using (var command = new NpgsqlCommand(query, connection))
            using (var reader = command.ExecuteReader())
            {
                Console.WriteLine("Answers:");
                while (reader.Read())
                {
                    Console.WriteLine($"ID: {reader["id"]}, Question ID: {reader["question_id"]}, Text: {reader["text"]}, Correct: {reader["correct"]}");
                }
            }
        }
    }

    public void DisplayQuizTries()
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            string query = "SELECT * FROM quiz_tries";
            using (var command = new NpgsqlCommand(query, connection))
            using (var reader = command.ExecuteReader())
            {
                Console.WriteLine("Quiz Tries:");
                while (reader.Read())
                {
                    Console.WriteLine($"ID: {reader["id"]}, Quiz ID: {reader["quiz_id"]}, User ID: {reader["user_id"]}, " +
                                      $"Started At: {reader["started_at"]}, Finished At: {reader["finished_at"]}");
                }
            }
        }
    }

    public void DisplayQuizAnswers()
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            string query = "SELECT * FROM quiz_answers";
            using (var command = new NpgsqlCommand(query, connection))
            using (var reader = command.ExecuteReader())
            {
                Console.WriteLine("Quiz Answers:");
                while (reader.Read())
                {
                    Console.WriteLine($"ID: {reader["id"]}, Quiz Try ID: {reader["quiz_try_id"]}, Question ID: {reader["question_id"]}, " +
                                      $"Answer ID: {reader["answer_id"]}");
                }
            }
        }
    }

    public void AddSampleUsers(int count = 30)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            for (int i = 0; i < count; i++)
            {
                string name = $"User{i + 1}";
                string username = $"user{i + 1}";
                string passwordHash = "hashed_password";

                string checkQuery = "SELECT COUNT(*) FROM users WHERE username = @username";
                using (var checkCommand = new NpgsqlCommand(checkQuery, connection))
                {
                    checkCommand.Parameters.AddWithValue("@username", username);
                    int exists = Convert.ToInt32(checkCommand.ExecuteScalar());
                    if (exists > 0) continue;
                }

                string query = "INSERT INTO users (name, username, password_hash, created_at, updated_at) VALUES (@name, @username, @password_hash, NOW(), NOW())";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password_hash", passwordHash);
                    command.ExecuteNonQuery();
                }
            }
        }
        Console.WriteLine($"{count} sample users added.");
    }

    public void AddSampleQuizzes(int count = 10)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            for (int i = 0; i < count; i++)
            {
                string prompt = $"Quiz {i + 1}";
                string difficulty = i % 3 == 0 ? "easy" : i % 3 == 1 ? "medium" : "hard";
                int numQuestions = 10;
                string[] allowedTypes = { "single-select", "multi-select", "true/false" };

                string query = "INSERT INTO quizzes (author_id, prompt, difficulty, num_questions, allowed_types, created_at, updated_at) " +
                               "VALUES ((SELECT id FROM users ORDER BY RANDOM() LIMIT 1), @prompt, @difficulty, @numQuestions, @allowedTypes, NOW(), NOW())";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@prompt", prompt);
                    command.Parameters.AddWithValue("@difficulty", difficulty);
                    command.Parameters.AddWithValue("@numQuestions", numQuestions);
                    command.Parameters.AddWithValue("@allowedTypes", allowedTypes);
                    command.ExecuteNonQuery();
                }
            }
        }
        Console.WriteLine($"{count} sample quizzes added.");
    }

    public void AddSampleQuestions(int count = 50)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            for (int i = 0; i < count; i++)
            {
                string questionText = $"Sample question {i + 1}";
                string type = i % 3 == 0 ? "single-select" : i % 3 == 1 ? "multi-select" : "true/false";

                string query = "INSERT INTO questions (quiz_id, type, question, created_at, updated_at) " +
                               "VALUES ((SELECT id FROM quizzes ORDER BY RANDOM() LIMIT 1), @type, @question, NOW(), NOW())";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@type", type);
                    command.Parameters.AddWithValue("@question", questionText);
                    command.ExecuteNonQuery();
                }
            }
        }
        Console.WriteLine($"{count} sample questions added.");
    }

    public void AddSampleAnswers(int count = 100)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            for (int i = 0; i < count; i++)
            {
                string answerText = $"Answer {i + 1}";
                bool isCorrect = i % 4 == 0;

                string query = "INSERT INTO answers (question_id, text, correct) " +
                               "VALUES ((SELECT id FROM questions ORDER BY RANDOM() LIMIT 1), @text, @correct)";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@text", answerText);
                    command.Parameters.AddWithValue("@correct", isCorrect);
                    command.ExecuteNonQuery();
                }
            }
        }
        Console.WriteLine($"{count} sample answers added.");
    }

    public void AddSampleQuizTries(int count = 20)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            for (int i = 0; i < count; i++)
            {
                string query = "INSERT INTO quiz_tries (quiz_id, user_id, started_at, finished_at, created_at, updated_at) " +
                               "VALUES ((SELECT id FROM quizzes ORDER BY RANDOM() LIMIT 1), " +
                               "(SELECT id FROM users ORDER BY RANDOM() LIMIT 1), NOW(), NOW() + interval '5 minutes', NOW(), NOW())";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
        Console.WriteLine($"{count} sample quiz tries added.");
    }

    public void AddSampleQuizAnswers(int count = 100)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            for (int i = 0; i < count; i++)
            {
                string query = "INSERT INTO quiz_answers (quiz_try_id, question_id, answer_id, created_at, updated_at) " +
                               "VALUES ((SELECT id FROM quiz_tries ORDER BY RANDOM() LIMIT 1), " +
                               "(SELECT id FROM questions ORDER BY RANDOM() LIMIT 1), " +
                               "(SELECT id FROM answers ORDER BY RANDOM() LIMIT 1), NOW(), NOW())";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
        Console.WriteLine($"{count} sample quiz answers added.");
    }
}
