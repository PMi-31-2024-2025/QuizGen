namespace QuizGen.BLL.Models.QuizAnswer;

public class QuizAnswerDto
{
    public int Id { get; set; }
    public int QuizTryId { get; set; }
    public int QuestionId { get; set; }
    public int AnswerId { get; set; }
    public string QuestionText { get; set; }
    public string AnswerText { get; set; }
    public bool IsCorrect { get; set; }
    public DateTime CreatedAt { get; set; }
} 