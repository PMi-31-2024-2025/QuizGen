using QuizGen.BLL.Models.Base;
using QuizGen.BLL.Models.Question;

namespace QuizGen.BLL.Services.Interfaces;

public interface IQuestionService
{
    Task<ServiceResult<QuestionDto>> CreateQuestionAsync(int quizId, string text, string type, string explanation);
    Task<ServiceResult<QuestionDto>> GetQuestionByIdAsync(int id);
    Task<ServiceResult<IEnumerable<QuestionDto>>> GetQuestionsByQuizAsync(int quizId);
    Task<ServiceResult<IEnumerable<QuestionDto>>> GetQuestionsByTypeAsync(string type);
}