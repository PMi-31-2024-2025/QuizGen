namespace QuizGen.BLL.Services;

using System.Security.Cryptography;
using System.Text;
using QuizGen.BLL.Models.Auth;
using QuizGen.BLL.Models.Base;
using QuizGen.BLL.Services.Interfaces;
using QuizGen.DAL.Interfaces;
using QuizGen.DAL.Models;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;

    public AuthService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<ServiceResult<AuthResult>> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username);

        if (user == null)
            return ServiceResult<AuthResult>.CreateError("Invalid username or password");

        var passwordHash = HashPassword(request.Password);
        if (user.PasswordHash != passwordHash)
            return ServiceResult<AuthResult>.CreateError("Invalid username or password");

        return ServiceResult<AuthResult>.CreateSuccess(MapToAuthResult(user));
    }

    public async Task<ServiceResult<AuthResult>> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var existingUser = await _userRepository.GetByUsernameAsync(request.Username);
            if (existingUser != null)
            {
                return ServiceResult<AuthResult>.CreateError("Username already exists");
            }

            var user = new User
            {
                Username = request.Username,
                PasswordHash = HashPassword(request.Password),
                Name = request.Name,
                OpenAiApiKey = request.OpenAiApiKey,
                UseLocalModel = request.UseLocalModel,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdUser = await _userRepository.AddAsync(user);
            
            return ServiceResult<AuthResult>.CreateSuccess(MapToAuthResult(createdUser));
        }
        catch (Exception ex)
        {
            return ServiceResult<AuthResult>.CreateError($"Registration failed: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return ServiceResult<bool>.CreateError("User not found");

        var currentPasswordHash = HashPassword(currentPassword);
        if (user.PasswordHash != currentPasswordHash)
            return ServiceResult<bool>.CreateError("Current password is incorrect");

        user.PasswordHash = HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        return ServiceResult<bool>.CreateSuccess(true);
    }

    public async Task<ServiceResult<bool>> UpdateProfileAsync(int userId, string name, string openAiApiKey, bool useLocalModel)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return ServiceResult<bool>.CreateError("User not found");

        user.Name = name;
        user.OpenAiApiKey = openAiApiKey;
        user.UseLocalModel = useLocalModel;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        return ServiceResult<bool>.CreateSuccess(true);
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private AuthResult MapToAuthResult(User user)
    {
        return new AuthResult
        {
            UserId = user.Id,
            Username = user.Username,
            Name = user.Name,
            OpenAiApiKey = user.OpenAiApiKey,
            UseLocalModel = user.UseLocalModel
        };
    }
}