namespace QuizGen.BLL.Models.Quiz;

public class QuizDto
{
    public int Id { get; set; }
    public int AuthorId { get; set; }
    public string AuthorName { get; set; }
    public string Prompt { get; set; }
    public string Difficulty { get; set; }
    public int NumQuestions { get; set; }
    public string[] AllowedTypes { get; set; }
    public DateTime CreatedAt { get; set; }
} 