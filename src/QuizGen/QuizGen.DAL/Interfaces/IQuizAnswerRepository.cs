namespace QuizGen.DAL.Interfaces;

using System.Collections.Generic;
using System.Threading.Tasks;
using QuizGen.DAL.Models;

public interface IQuizAnswerRepository : IRepository<QuizAnswer>
{
    Task<IEnumerable<QuizAnswer>> GetByQuizTryIdAsync(int quizTryId);
    Task<IEnumerable<QuizAnswer>> GetByQuestionIdAsync(int questionId);
}