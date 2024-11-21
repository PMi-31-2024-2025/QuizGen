namespace QuizGen.BLL.Configuration;

public class AppConfig
{
    public required string DatabaseConnectionString { get; set; }
    public required string LocalSettingsPath { get; set; }
} 