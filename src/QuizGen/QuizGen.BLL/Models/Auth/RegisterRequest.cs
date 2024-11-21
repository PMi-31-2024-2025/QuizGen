namespace QuizGen.BLL.Models.Auth;

public class RegisterRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string Name { get; set; }
    public string OpenAiApiKey { get; set; }
    public bool UseLocalModel { get; set; }
}