namespace QuizGen.BLL.Models.QuizTry;

public class QuizTryDetailsDto
{
    public int Id { get; set; }
    public string QuizName { get; set; }
    public string QuizPrompt { get; set; }
    public string Difficulty { get; set; }
    public int CurrentQuestionIndex { get; set; }
    public int TotalQuestions { get; set; }
    public List<QuizTryQuestionDto> Questions { get; set; }
    public DateTime StartedAt { get; set; }
}

public class QuizTryQuestionDto
{
    public int Id { get; set; }
    public string Text { get; set; }
    public string Type { get; set; }
    public List<QuizTryAnswerOptionDto> AnswerOptions { get; set; }
    public string Explanation { get; set; }
}

public class QuizTryAnswerOptionDto
{
    public int Id { get; set; }
    public string Text { get; set; }
    public bool IsSelected { get; set; }
}