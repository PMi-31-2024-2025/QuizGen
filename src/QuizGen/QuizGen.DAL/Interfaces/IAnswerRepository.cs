namespace QuizGen.DAL.Interfaces;

using QuizGen.DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IAnswerRepository : IRepository<Answer>
{
    Task<IEnumerable<Answer>> GetByQuestionIdAsync(int questionId);
    Task<IEnumerable<Answer>> GetCorrectAnswersByQuestionIdAsync(int questionId);
    Task<IList<Answer>> GetByQuizIdAsync(int quizId);
}
