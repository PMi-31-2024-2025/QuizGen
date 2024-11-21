namespace QuizGen.DAL.Repositories;

using QuizGen.DAL.Context;
using QuizGen.DAL.Interfaces;
using QuizGen.DAL.Models;
using Microsoft.EntityFrameworkCore;

public class QuizAnswerRepository : BaseRepository<QuizAnswer>, IQuizAnswerRepository
{
    public QuizAnswerRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<QuizAnswer>> GetByQuizTryIdAsync(int quizTryId)
    {
        return await _dbSet
            .Where(qa => qa.QuizTryId == quizTryId)
            .Include(qa => qa.Question)
            .Include(qa => qa.Answer)
            .ToListAsync();
    }

    public async Task<IEnumerable<QuizAnswer>> GetByQuestionIdAsync(int questionId)
    {
        return await _dbSet
            .Where(qa => qa.QuestionId == questionId)
            .Include(qa => qa.QuizTry)
            .Include(qa => qa.Answer)
            .ToListAsync();
    }
}