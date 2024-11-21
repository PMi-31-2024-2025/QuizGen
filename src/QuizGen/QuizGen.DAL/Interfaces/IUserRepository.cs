namespace QuizGen.DAL.Interfaces;

using System.Collections.Generic;
using System.Threading.Tasks;
using QuizGen.DAL.Models;

public interface IUserRepository : IRepository<User>
{
    Task<User> GetByUsernameAsync(string username);
    Task<bool> UsernameExistsAsync(string username);
}