namespace QuizGen.DAL.Repositories;

using QuizGen.DAL.Context;
using QuizGen.DAL.Interfaces;
using QuizGen.DAL.Models;
using Microsoft.EntityFrameworkCore;

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
}
