namespace QuizGen.DAL.Repositories;

using Microsoft.EntityFrameworkCore;
using QuizGen.DAL.Context;
using QuizGen.DAL.Interfaces;
using QuizGen.DAL.Models;

public class QuizRepository : BaseRepository<Quiz>, IQuizRepository
{
    public QuizRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Quiz>> GetByAuthorIdAsync(int authorId)
    {
        return await _dbSet
            .Where(q => q.AuthorId == authorId)
            .Include(q => q.Questions)
            .Include(q => q.QuizTries)
            .ToListAsync();
    }

    public async Task<IEnumerable<Quiz>> GetByDifficultyAsync(string difficulty)
    {
        return await _dbSet
            .Where(q => q.Difficulty == difficulty)
            .Include(q => q.Questions)
            .Include(q => q.QuizTries)
            .ToListAsync();
    }

    public override async Task<Quiz> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(q => q.Questions)
            .Include(q => q.QuizTries)
            .FirstOrDefaultAsync(q => q.Id == id);
    }
}