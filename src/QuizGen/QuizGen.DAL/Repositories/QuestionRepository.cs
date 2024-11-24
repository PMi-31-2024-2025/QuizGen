namespace QuizGen.DAL.Repositories;

using Microsoft.EntityFrameworkCore;
using QuizGen.DAL.Context;
using QuizGen.DAL.Interfaces;
using QuizGen.DAL.Models;

public class QuestionRepository : BaseRepository<Question>, IQuestionRepository
{
    public QuestionRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Question>> GetByQuizIdAsync(int quizId)
    {
        return await _dbSet
            .Where(q => q.QuizId == quizId)
            .Include(q => q.Answers)
            .ToListAsync();
    }

    public async Task<IEnumerable<Question>> GetByTypeAsync(string type)
    {
        return await _dbSet
            .Where(q => q.Type == type)
            .Include(q => q.Answers)
            .ToListAsync();
    }

    public override async Task<Question> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(q => q.Answers)
            .FirstOrDefaultAsync(q => q.Id == id);
    }
}