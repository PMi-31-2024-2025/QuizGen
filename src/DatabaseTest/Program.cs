using Microsoft.Extensions.Configuration;

class Program
{
    static void Main(string[] args)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        string defaultConnectionString = configuration.GetConnectionString("DefaultConnection");
        string adminConnectionString = configuration.GetConnectionString("AdminConnection");

        var dbHelper = new DatabaseHelper(defaultConnectionString);

        dbHelper.InitializeDatabase(adminConnectionString);
        Console.WriteLine('\n');

        dbHelper.AddSampleUsers();
        dbHelper.AddSampleQuizzes();
        dbHelper.AddSampleQuestions();
        dbHelper.AddSampleAnswers();
        dbHelper.AddSampleQuizTries();
        dbHelper.AddSampleQuizAnswers();
        Console.WriteLine('\n');

        dbHelper.DisplayUsers();
        Console.WriteLine('\n');

        dbHelper.DisplayQuizzes();
        Console.WriteLine('\n');

        dbHelper.DisplayQuestions();
        Console.WriteLine('\n');

        dbHelper.DisplayAnswers();
        Console.WriteLine('\n');

        dbHelper.DisplayQuizTries();
        Console.WriteLine('\n');

        dbHelper.DisplayQuizAnswers();
    }
}
