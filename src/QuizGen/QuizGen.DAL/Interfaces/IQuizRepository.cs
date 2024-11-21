namespace QuizGen.DAL.Interfaces;

using System.Collections.Generic;
using System.Threading.Tasks;
using QuizGen.DAL.Models;

public interface IQuizRepository : IRepository<Quiz>
{
    Task<IEnumerable<Quiz>> GetByAuthorIdAsync(int authorId);
    Task<IEnumerable<Quiz>> GetByDifficultyAsync(string difficulty);
}