namespace QuizGen.DAL.Repositories;

using Microsoft.EntityFrameworkCore;
using QuizGen.DAL.Context;
using QuizGen.DAL.Interfaces;
using QuizGen.DAL.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    public async Task<User> GetByUsernameAsync(string username)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await _dbSet.AnyAsync(u => u.Username == username);
    }
    
    public override async Task<IEnumerable<User>> GetByIdsAsync(IEnumerable<int> ids)
    {
        var idsList = ids.ToList();
        if (!idsList.Any())
            return new List<User>();
            
        return await _dbSet
            .Where(u => idsList.Contains(u.Id))
            .ToListAsync();
    }
}