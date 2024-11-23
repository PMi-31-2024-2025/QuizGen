using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizGen.DAL.Models;

[Table("users")]
public class User : BaseEntity
{
    public User()
    {
        Quizzes = new List<Quiz>();
        QuizTries = new List<QuizTry>();
    }

    [Required]
    [Column("name")]
    public required string Name { get; set; }

    [Required]
    [Column("username")]
    public required string Username { get; set; }

    [Required]
    [Column("password_hash")]
    public required string PasswordHash { get; set; }

    [Column("openai_api_key")]
    public string? OpenAiApiKey { get; set; }

    [Column("gpt_model")]
    public string GptModel { get; set; } = "gpt-4o-mini";

    // Navigation properties
    public ICollection<Quiz> Quizzes { get; set; }
    public ICollection<QuizTry> QuizTries { get; set; }
}
