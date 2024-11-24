namespace QuizGen.DAL.Interfaces;

using QuizGen.DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IQuizAnswerRepository : IRepository<QuizAnswer>
{
    Task<IEnumerable<QuizAnswer>> GetByQuizTryIdAsync(int quizTryId);
    Task<IEnumerable<QuizAnswer>> GetByQuestionIdAsync(int questionId);
}