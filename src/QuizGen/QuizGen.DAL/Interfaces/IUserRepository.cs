namespace QuizGen.DAL.Interfaces;

using QuizGen.DAL.Models;
using System.Threading.Tasks;

public interface IUserRepository : IRepository<User>
{
    Task<User> GetByUsernameAsync(string username);
    Task<bool> UsernameExistsAsync(string username);
}