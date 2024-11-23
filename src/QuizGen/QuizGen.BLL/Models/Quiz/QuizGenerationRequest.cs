public class QuizGenerationRequest
{
    public string Topic { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public int QuestionCount { get; set; }
    public string[] AllowedTypes { get; set; } = Array.Empty<string>();
}