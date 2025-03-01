namespace QuizGen.DAL.Interfaces;

using QuizGen.DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IQuizTryRepository : IRepository<QuizTry>
{
    Task<IEnumerable<QuizTry>> GetByUserIdAsync(int userId);
    Task<IEnumerable<QuizTry>> GetByQuizIdAsync(int quizId);
    Task<IEnumerable<QuizTry>> GetCompletedByUserIdAsync(int userId);
    Task<QuizTry> GetQuizTryWithDetailsAsync(int quizTryId);
    Task<IEnumerable<QuizTry>> GetActiveQuizTriesByUserAsync(int userId);
    Task<bool> DeleteWithAnswersAsync(int quizTryId);
    Task RemoveAnswersForQuestionAsync(int quizTryId, int questionId);
    Task AddAnswerAsync(QuizAnswer answer);
    Task<IEnumerable<QuizTry>> GetCompletedQuizTriesByUserAsync(int userId);
    Task<IList<QuizAnswer>> GetAnswersAsync(int quizTryId);
}