namespace QuizGen.DAL.Interfaces;

using QuizGen.DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IQuestionRepository : IRepository<Question>
{
    Task<IEnumerable<Question>> GetByQuizIdAsync(int quizId);
    Task<IEnumerable<Question>> GetByTypeAsync(string type);
}