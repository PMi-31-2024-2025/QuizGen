namespace QuizGen.DAL.Interfaces;

using QuizGen.DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IQuizRepository : IRepository<Quiz>
{
    Task<IEnumerable<Quiz>> GetByAuthorIdAsync(int authorId);
    Task<IEnumerable<Quiz>> GetByDifficultyAsync(string difficulty);
}