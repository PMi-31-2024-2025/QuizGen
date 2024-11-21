namespace QuizGen.DAL.Repositories;

using QuizGen.DAL.Context;
using QuizGen.DAL.Interfaces;
using QuizGen.DAL.Models;
using Microsoft.EntityFrameworkCore;

public class QuizTryRepository : BaseRepository<QuizTry>, IQuizTryRepository
{
    public QuizTryRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<QuizTry>> GetByUserIdAsync(int userId)
    {
        return await _dbSet
            .Where(qt => qt.UserId == userId)
            .Include(qt => qt.Quiz)
            .Include(qt => qt.QuizAnswers)
            .ToListAsync();
    }

    public async Task<IEnumerable<QuizTry>> GetByQuizIdAsync(int quizId)
    {
        return await _dbSet
            .Where(qt => qt.QuizId == quizId)
            .Include(qt => qt.User)
            .Include(qt => qt.QuizAnswers)
            .ToListAsync();
    }

    public async Task<IEnumerable<QuizTry>> GetCompletedByUserIdAsync(int userId)
    {
        return await _dbSet
            .Where(qt => qt.UserId == userId && qt.FinishedAt != null)
            .Include(qt => qt.Quiz)
            .Include(qt => qt.QuizAnswers)
            .ToListAsync();
    }
}