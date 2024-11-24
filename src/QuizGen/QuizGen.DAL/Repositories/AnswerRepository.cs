namespace QuizGen.DAL.Repositories;

using Microsoft.EntityFrameworkCore;
using QuizGen.DAL.Context;
using QuizGen.DAL.Interfaces;
using QuizGen.DAL.Models;

public class AnswerRepository : BaseRepository<Answer>, IAnswerRepository
{
    public AnswerRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Answer>> GetByQuestionIdAsync(int questionId)
    {
        return await _dbSet
            .Where(a => a.QuestionId == questionId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Answer>> GetCorrectAnswersByQuestionIdAsync(int questionId)
    {
        return await _dbSet
            .Where(a => a.QuestionId == questionId && a.IsCorrect)
            .ToListAsync();
    }

    public async Task<IList<Answer>> GetByQuizIdAsync(int quizId)
    {
        return await _context.Answers
            .Include(a => a.Question)
            .Where(a => a.Question.QuizId == quizId)
            .ToListAsync();
    }
}
