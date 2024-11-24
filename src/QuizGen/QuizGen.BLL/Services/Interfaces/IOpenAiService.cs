using QuizGen.BLL.Models.Base;

public interface IOpenAiService
{
    Task<ServiceResult<GeneratedQuiz>> GenerateQuizAsync(
        QuizGenerationRequest request,
        string apiKey,
        string model = "gpt-4o-mini");
    bool IsGenerationFailed(GeneratedQuiz quiz);
}