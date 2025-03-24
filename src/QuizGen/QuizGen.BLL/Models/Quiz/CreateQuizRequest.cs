namespace QuizGen.BLL.Models.Quiz;

public class CreateQuizRequest
{
    public string Topic { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public int NumQuestions { get; set; }
    public string[] AllowedTypes { get; set; } = Array.Empty<string>();
} 