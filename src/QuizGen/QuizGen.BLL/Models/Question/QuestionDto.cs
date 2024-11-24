using QuizGen.BLL.Models.Answer;

namespace QuizGen.BLL.Models.Question;

public class QuestionDto
{
    public int Id { get; set; }
    public int QuizId { get; set; }
    public string Text { get; set; }
    public string Type { get; set; }
    public string? Explanation { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<AnswerDto> Answers { get; set; }
}