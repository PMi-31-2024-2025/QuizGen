namespace QuizGen.DAL.Repositories;

using Microsoft.EntityFrameworkCore;
using QuizGen.DAL.Context;
using QuizGen.DAL.Interfaces;
using System.Linq;

public abstract class BaseRepository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    protected BaseRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }
    
    public virtual async Task<IEnumerable<T>> GetByIdsAsync(IEnumerable<int> ids)
    {
        // This is a default implementation. For entity types where the primary key has a different name
        // than 'Id', this method should be overridden in the specific repository class.
        var idsList = ids.ToList();
        if (!idsList.Any())
            return new List<T>();
            
        // Note: This is a naive implementation and may not work for all entities.
        // Specific repositories should override this method based on their entity structures.
        return await _dbSet.FindAsync(idsList.Select(id => (object)id).ToArray())
            .AsTask()
            .ContinueWith(t => t.Result != null ? new List<T> { t.Result } : new List<T>());
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        var result = await _dbSet.AddAsync(entity);
        _ = await _context.SaveChangesAsync();
        return result.Entity;
    }

    public virtual async Task UpdateAsync(T entity)
    {
        _ = _dbSet.Update(entity);
        _ = await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _ = _dbSet.Remove(entity);
            _ = await _context.SaveChangesAsync();
        }
    }
}