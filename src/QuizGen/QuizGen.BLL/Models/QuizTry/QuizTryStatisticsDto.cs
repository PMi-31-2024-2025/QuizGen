public class QuizTryStatisticsDto
{
    public int TotalTries { get; set; }
    public double AverageScore { get; set; }
    public TimeSpan AverageTime { get; set; }
    public double PassRate { get; set; }
} 