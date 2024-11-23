using QuizGen.BLL.Models.Base;
using QuizGen.BLL.Models.Quiz;

namespace QuizGen.BLL.Services.Interfaces;

public interface IQuizService
{
    Task<ServiceResult<QuizDto>> CreateQuizAsync(int authorId, string prompt, string difficulty, int numQuestions, string[] allowedTypes);
    Task<ServiceResult<QuizDto>> GetQuizByIdAsync(int id);
    Task<ServiceResult<IEnumerable<QuizDto>>> GetQuizzesByAuthorAsync(int authorId);
    Task<ServiceResult<IEnumerable<QuizDto>>> GetQuizzesByDifficultyAsync(string difficulty);
    Task<ServiceResult<bool>> DeleteQuizAsync(int id);
} 