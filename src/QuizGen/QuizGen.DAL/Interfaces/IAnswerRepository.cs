namespace QuizGen.DAL.Interfaces;

using System.Collections.Generic;
using System.Threading.Tasks;
using QuizGen.DAL.Models;

public interface IAnswerRepository : IRepository<Answer>
{
    Task<IEnumerable<Answer>> GetByQuestionIdAsync(int questionId);
    Task<IEnumerable<Answer>> GetCorrectAnswersByQuestionIdAsync(int questionId);
    Task<IList<Answer>> GetByQuizIdAsync(int quizId);
}
