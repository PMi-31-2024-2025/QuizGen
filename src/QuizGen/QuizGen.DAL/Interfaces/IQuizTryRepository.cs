namespace QuizGen.DAL.Interfaces;

using System.Collections.Generic;
using System.Threading.Tasks;
using QuizGen.DAL.Models;

public interface IQuizTryRepository : IRepository<QuizTry>
{
    Task<IEnumerable<QuizTry>> GetByUserIdAsync(int userId);
    Task<IEnumerable<QuizTry>> GetByQuizIdAsync(int quizId);
    Task<IEnumerable<QuizTry>> GetCompletedByUserIdAsync(int userId);
}