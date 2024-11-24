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

    public async Task<QuizTry> GetQuizTryWithDetailsAsync(int quizTryId)
    {
        return await _context.QuizTries
            .Include(qt => qt.Quiz)
                .ThenInclude(q => q.Questions)
                    .ThenInclude(q => q.Answers)
            .Include(qt => qt.QuizAnswers)
            .FirstOrDefaultAsync(qt => qt.Id == quizTryId);
    }

    public async Task<IEnumerable<QuizTry>> GetActiveQuizTriesByUserAsync(int userId)
    {
        return await _context.QuizTries
            .Where(qt => qt.UserId == userId && !qt.FinishedAt.HasValue)
            .ToListAsync();
    }

    public async Task<bool> DeleteWithAnswersAsync(int quizTryId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Delete associated answers
            var answers = await _context.QuizAnswers
                .Where(qa => qa.QuizTryId == quizTryId)
                .ToListAsync();
            _context.QuizAnswers.RemoveRange(answers);

            // Delete quiz try
            var quizTry = await _context.QuizTries.FindAsync(quizTryId);
            if (quizTry != null)
                _context.QuizTries.Remove(quizTry);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            return false;
        }
    }

    public async Task RemoveAnswersForQuestionAsync(int quizTryId, int questionId)
    {
        var existingAnswers = await _context.QuizAnswers
            .Where(qa => qa.QuizTryId == quizTryId && qa.QuestionId == questionId)
            .ToListAsync();
        
        if (existingAnswers.Any())
        {
            _context.QuizAnswers.RemoveRange(existingAnswers);
            await _context.SaveChangesAsync();
        }
    }

    public async Task AddAnswerAsync(QuizAnswer answer)
    {
        await _context.QuizAnswers.AddAsync(answer);
        await _context.SaveChangesAsync();
    }
}