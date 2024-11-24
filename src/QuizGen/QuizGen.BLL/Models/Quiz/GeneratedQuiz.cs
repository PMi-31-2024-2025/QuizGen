using System.Text.Json.Serialization;

public class GeneratedQuiz
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("questions")]
    public List<GeneratedQuestion> Questions { get; set; } = new();
}

public class GeneratedQuestion
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("options")]
    public List<GeneratedOption> Options { get; set; } = new();

    [JsonPropertyName("explanation")]
    public string Explanation { get; set; } = string.Empty;
}

public class GeneratedOption
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("isCorrect")]
    public bool IsCorrect { get; set; }
}