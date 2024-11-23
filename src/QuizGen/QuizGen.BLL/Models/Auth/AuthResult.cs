namespace QuizGen.BLL.Models.Auth;

public class AuthResult
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? OpenAiApiKey { get; set; }
    public string GptModel { get; set; } = string.Empty;
}