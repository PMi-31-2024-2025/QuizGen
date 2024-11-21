namespace QuizGen.DAL.Interfaces;

using System.Collections.Generic;
using System.Threading.Tasks;
using QuizGen.DAL.Models;

public interface IQuestionRepository : IRepository<Question>
{
    Task<IEnumerable<Question>> GetByQuizIdAsync(int quizId);
    Task<IEnumerable<Question>> GetByTypeAsync(string type);
}