using QuizGen.BLL.Models.Base;
using QuizGen.BLL.Models.QuizTry;

namespace QuizGen.BLL.Services.Interfaces;

public interface IQuizTryService
{
    Task<ServiceResult<QuizTryDto>> StartQuizTryAsync(int quizId, int userId);
    Task<ServiceResult<QuizTryDto>> FinishQuizTryAsync(int quizTryId);
    Task<ServiceResult<QuizTryDto>> GetQuizTryByIdAsync(int id);
    Task<ServiceResult<IEnumerable<QuizTryDto>>> GetQuizTriesByUserAsync(int userId);
    Task<ServiceResult<IEnumerable<QuizTryDto>>> GetQuizTriesByQuizAsync(int quizId);
    Task<ServiceResult<IEnumerable<QuizTryDto>>> GetCompletedQuizTriesByUserAsync(int userId);
    Task<ServiceResult<QuizTryDetailsDto>> GetQuizTryDetailsAsync(int quizTryId);
    Task<ServiceResult<bool>> DeleteQuizTryAsync(int quizTryId);
    Task<ServiceResult<QuizTryResultDto>> CalculateAndSaveScoreAsync(int quizTryId);
    Task<ServiceResult<bool>> SaveAnswersAsync(int quizTryId, int questionId, List<int> selectedAnswerIds);
} 