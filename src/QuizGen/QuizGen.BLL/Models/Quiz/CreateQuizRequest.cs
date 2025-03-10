namespace QuizGen.BLL.Models.Quiz;

public class CreateQuizRequest
{
    public int AuthorId { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public int NumQuestions { get; set; }
    public string[] AllowedTypes { get; set; } = Array.Empty<string>();
} 