using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizGen.DAL.Models;

[Table("questions")]
public class Question : BaseEntity
{
    public Question()
    {
        Answers = new List<Answer>();
        QuizAnswers = new List<QuizAnswer>();
    }

    [Required]
    [Column("quiz_id")]
    public required int QuizId { get; set; }

    [Required]
    [Column("text")]
    public required string Text { get; set; }

    [Required]
    [Column("type")]
    public required string Type { get; set; }

    [Column("explanation")]
    public string? Explanation { get; set; }

    // Navigation properties
    public Quiz Quiz { get; set; }
    public ICollection<Answer> Answers { get; set; }
    public ICollection<QuizAnswer> QuizAnswers { get; set; }
}
