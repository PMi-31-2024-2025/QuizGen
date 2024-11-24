public class QuizTryListItemDto
{
    public int Id { get; set; }
    public string QuizName { get; set; }
    public string Difficulty { get; set; }
    public string[] QuestionTypes { get; set; }
    public int TotalQuestions { get; set; }
    public double Score { get; set; }
    public TimeSpan Duration { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime FinishedAt { get; set; }
}