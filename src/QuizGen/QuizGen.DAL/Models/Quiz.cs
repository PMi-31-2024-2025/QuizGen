using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizGen.DAL.Models;

[Table("quizzes")]
public class Quiz : BaseEntity
{
    public Quiz()
    {
        Questions = new List<Question>();
        QuizTries = new List<QuizTry>();
    }

    [Required]
    [Column("author_id")]
    public required int AuthorId { get; set; }

    [Required]
    [Column("name")]
    public required string Name { get; set; }

    [Required]
    [Column("prompt")]
    public required string Prompt { get; set; }

    [Required]
    [Column("difficulty")]
    public required string Difficulty { get; set; }

    [Required]
    [Column("num_questions")]
    public required int NumQuestions { get; set; }

    [Required]
    [Column("allowed_types")]
    public required string[] AllowedTypes { get; set; }

    // Navigation properties
    public User Author { get; set; }
    public ICollection<Question> Questions { get; set; }
    public ICollection<QuizTry> QuizTries { get; set; }
}