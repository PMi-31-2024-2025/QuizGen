using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;
using QuizGen.BLL.Models.Base;
using System.Text.Json;

public class OpenAiService : IOpenAiService
{
    public OpenAiService()
    {
        // Empty constructor since we'll create client per request
    }

    public async Task<ServiceResult<GeneratedQuiz>> GenerateQuizAsync(
        QuizGenerationRequest request, 
        string apiKey,
        string model = "gpt-4o-mini")
    {
        var _client = new OpenAIClient(apiKey);
        
        string systemPrompt = """
You are a professional quiz generator for the QuizGen AI application. Your task is to create educational quizzes that are accurate, engaging, and appropriate for the specified difficulty level. Follow these strict rules:

1. Difficulty levels must match exactly one of: easy, medium, hard, expert
    - Easy: Basic concept understanding
    - Medium: Applied knowledge
    - Hard: Complex problem solving
    - Expert: Advanced concepts and edge cases

2. Question rules:
    - Single-select: Exactly one correct answer
    - Multi-select: Two or more correct answers
    - True-false: Always two options ("True"/"False")
    - All questions must have 2-6 options
    - Each question must have a clear, concise explanation

You will receive input in this format:
{
    "topic": "string",
    "difficulty": "easy", "medium", "hard", "expert",
    "questionCount": 3, 6, 9, 12,
    "allowedTypes": ["single-select", "multi-select", "true-false"]
}

Generate questions that are factually correct and unambiguous. Each question should test a different aspect of the topic.

If user's prompt does not make sense, or do not match the specified input format (it should always be JSON, I provided earlier), then set all fields to empty ({"name": "", "questions": []})
""";

        var responseSchema = BinaryData.FromBytes("""
            {
                "type": "object",
                "properties": {
                "quiz": {
                    "type": "object",
                    "properties": {
                    "name": {
                        "type": "string",
                        "description": "Short subject or topic of the quiz."
                    },
                    "questions": {
                        "type": "array",
                        "description": "A list of questions included in the quiz.",
                        "items": {
                        "type": "object",
                        "properties": {
                            "text": {
                            "type": "string",
                            "description": "The text of the question."
                            },
                            "type": {
                            "type": "string",
                            "description": "The type of question.",
                            "enum": [
                                "single-select",
                                "multi-select",
                                "true-false"
                            ]
                            },
                            "options": {
                            "type": "array",
                            "description": "The available options for the question.",
                            "items": {
                                "type": "object",
                                "properties": {
                                "text": {
                                    "type": "string",
                                    "description": "The text of the option."
                                },
                                "isCorrect": {
                                    "type": "boolean",
                                    "description": "Indicates if this option is the correct answer."
                                }
                                },
                                "required": [
                                "text",
                                "isCorrect"
                                ],
                                "additionalProperties": false
                            }
                            },
                            "explanation": {
                            "type": "string",
                            "description": "Explanation of the correct answer for the question."
                            }
                        },
                        "required": [
                            "text",
                            "type",
                            "options",
                            "explanation"
                        ],
                        "additionalProperties": false
                        }
                    }
                    },
                    "required": [
                    "name",
                    "questions"
                    ],
                    "additionalProperties": false
                }
                },
                "required": [
                "quiz"
                ],
                "additionalProperties": false
            }
            """u8.ToArray());

        try
        {
            var messageRequest = JsonSerializer.Serialize(request);
            List<ChatMessage> messages =
            [
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(messageRequest),
            ];

            ChatCompletionOptions options = new()
            {
                ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                    jsonSchemaFormatName: "quiz_generation",
                    jsonSchema: responseSchema,
                    jsonSchemaIsStrict: true
                )
            };

            var client = _client.GetChatClient(model);
            ChatCompletion completion = await client.CompleteChatAsync(messages, options);

            using JsonDocument structuredJson = JsonDocument.Parse(completion.Content[0].Text);
            var response = JsonSerializer.Deserialize<GeneratedQuizResponse>(structuredJson);

            if (response?.Quiz == null || IsGenerationFailed(response.Quiz))
            {
                return ServiceResult<GeneratedQuiz>.CreateError($"Error generating quiz: Invalid AI response: {completion.Content[0].Text}");
            }

            return ServiceResult<GeneratedQuiz>.CreateSuccess(response.Quiz);
        }
        catch (Exception ex)
        {
            return ServiceResult<GeneratedQuiz>.CreateError($"Error generating quiz: {ex.Message}");
        }
    }

    public bool IsGenerationFailed(GeneratedQuiz quiz)
    {
        return string.IsNullOrEmpty(quiz.Name) || !quiz.Questions.Any();
    }
}