namespace QuizGen.BLL.Models.QuizTry;

public class QuizTryResultDto
{
    public int Id { get; set; }
    public string QuizName { get; set; }
    public double Score { get; set; }
    public int TotalQuestions { get; set; }
    public int CorrectAnswers { get; set; }
    public List<QuestionResultDto> Questions { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime FinishedAt { get; set; }
}

public class QuestionResultDto
{
    public int Id { get; set; }
    public string Text { get; set; }
    public bool IsCorrect { get; set; }
    public string Explanation { get; set; }
    public double Score { get; set; }
    public double CorrectPercentage { get; set; }
    public string Type { get; set; }
    public List<AnswerResultDto> Answers { get; set; }
}

public class AnswerResultDto
{
    public int Id { get; set; }
    public string Text { get; set; }
    public bool IsCorrect { get; set; }
    public bool WasSelected { get; set; }
}