namespace QuizGen.BLL.Models.Auth;

public class UpdateProfileRequest
{
    public string Name { get; set; } = string.Empty;
    public string OpenAiApiKey { get; set; } = string.Empty;
    public string GptModel { get; set; } = string.Empty;
} 