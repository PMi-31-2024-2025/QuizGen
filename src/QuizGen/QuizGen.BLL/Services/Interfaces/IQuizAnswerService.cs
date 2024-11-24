using QuizGen.BLL.Models.Base;
using QuizGen.BLL.Models.QuizAnswer;

namespace QuizGen.BLL.Services.Interfaces;

public interface IQuizAnswerService
{
    Task<ServiceResult<QuizAnswerDto>> CreateQuizAnswerAsync(int quizTryId, int questionId, int answerId);
    Task<ServiceResult<QuizAnswerDto>> GetQuizAnswerByIdAsync(int id);
    Task<ServiceResult<IEnumerable<QuizAnswerDto>>> GetQuizAnswersByQuizTryAsync(int quizTryId);
    Task<ServiceResult<IEnumerable<QuizAnswerDto>>> GetQuizAnswersByQuestionAsync(int questionId);
}