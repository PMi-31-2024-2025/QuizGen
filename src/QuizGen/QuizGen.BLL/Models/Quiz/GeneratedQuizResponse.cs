using System.Text.Json.Serialization;

public class GeneratedQuizResponse
{
    [JsonPropertyName("quiz")]
    public GeneratedQuiz Quiz { get; set; } = new();
}