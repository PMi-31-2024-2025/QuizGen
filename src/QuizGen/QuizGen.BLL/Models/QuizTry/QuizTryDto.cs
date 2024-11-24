using QuizGen.BLL.Models.QuizAnswer;

namespace QuizGen.BLL.Models.QuizTry;

public class QuizTryDto
{
    public int Id { get; set; }
    public int QuizId { get; set; }
    public int UserId { get; set; }
    public string QuizPrompt { get; set; }
    public string UserName { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public ICollection<QuizAnswerDto> Answers { get; set; }
    public DateTime CreatedAt { get; set; }
}