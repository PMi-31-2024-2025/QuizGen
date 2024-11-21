using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizGen.DAL.Models;

[Table("quiz_tries")]
public class QuizTry : BaseEntity
{
    public QuizTry()
    {
        QuizAnswers = new List<QuizAnswer>();
    }

    [Required]
    [Column("quiz_id")]
    public required int QuizId { get; set; }

    [Required]
    [Column("user_id")]
    public required int UserId { get; set; }

    [Required]
    [Column("started_at")]
    public required DateTime StartedAt { get; set; }

    [Column("finished_at")]
    public DateTime? FinishedAt { get; set; }

    // Navigation properties
    public Quiz Quiz { get; set; }
    public User User { get; set; }
    public ICollection<QuizAnswer> QuizAnswers { get; set; }
}
