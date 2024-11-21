using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizGen.DAL.Models;

[Table("quiz_answers")]
public class QuizAnswer : BaseEntity
{
    [Required]
    [Column("quiz_try_id")]
    public required int QuizTryId { get; set; }

    [Required]
    [Column("question_id")]
    public required int QuestionId { get; set; }

    [Required]
    [Column("answer_id")]
    public required int AnswerId { get; set; }

    // Navigation properties
    public QuizTry QuizTry { get; set; }
    public Question Question { get; set; }
    public Answer Answer { get; set; }
}
