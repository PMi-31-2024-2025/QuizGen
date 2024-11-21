using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizGen.DAL.Models;

[Table("answers")]
public class Answer : BaseEntity
{
    public Answer()
    {
        QuizAnswers = new List<QuizAnswer>();
    }

    [Required]
    [Column("question_id")]
    public required int QuestionId { get; set; }

    [Required]
    [Column("text")]
    public required string Text { get; set; }

    [Required]
    [Column("is_correct")]
    public required bool IsCorrect { get; set; }

    // Navigation properties
    public Question Question { get; set; }
    public ICollection<QuizAnswer> QuizAnswers { get; set; }
}