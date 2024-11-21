namespace QuizGen.BLL.Models.Auth;

public class AuthResult
{
    public int UserId { get; set; }
    public string Username { get; set; }
    public string Name { get; set; }
    public string OpenAiApiKey { get; set; }
    public bool UseLocalModel { get; set; }
}