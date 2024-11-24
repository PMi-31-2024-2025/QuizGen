using QuizGen.BLL.Models.Answer;
using QuizGen.BLL.Models.Base;

namespace QuizGen.BLL.Services.Interfaces;

public interface IAnswerService
{
    Task<ServiceResult<AnswerDto>> CreateAnswerAsync(int questionId, string text, bool isCorrect);
    Task<ServiceResult<AnswerDto>> GetAnswerByIdAsync(int id);
    Task<ServiceResult<IEnumerable<AnswerDto>>> GetAnswersByQuestionAsync(int questionId);
    Task<ServiceResult<IEnumerable<AnswerDto>>> GetCorrectAnswersByQuestionAsync(int questionId);
}