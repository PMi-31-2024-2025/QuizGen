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
    private readonly IAuthStateService _authStateService;

    public AuthService(IUserRepository userRepository, IAuthStateService authStateService)
    {
        _userRepository = userRepository;
        _authStateService = authStateService;
    }

    public async Task<ServiceResult<AuthResult>> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username);

        if (user == null)
            return ServiceResult<AuthResult>.CreateError("Invalid username or password");

        var passwordHash = HashPassword(request.Password);
        if (user.PasswordHash != passwordHash)
            return ServiceResult<AuthResult>.CreateError("Invalid username or password");

        var authResult = MapToAuthResult(user);
        
        // Store credentials for auto-login
        var credentials = new StoredCredentials
        {
            UserId = user.Id,
            Username = user.Username,
            HashedPassword = user.PasswordHash
        };
        _authStateService.SetCredentials(credentials);
        await _authStateService.SaveStateAsync();

        return ServiceResult<AuthResult>.CreateSuccess(authResult);
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
                GptModel = "gpt-4o-mini"
            };

            await _userRepository.AddAsync(user);
            return ServiceResult<AuthResult>.CreateSuccess(MapToAuthResult(user));
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

    public async Task<ServiceResult<bool>> UpdateProfileAsync(int userId, string name, string openAiApiKey, string gptModel)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return ServiceResult<bool>.CreateError("User not found");

        user.Name = name;
        user.OpenAiApiKey = openAiApiKey;
        user.GptModel = gptModel;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        return ServiceResult<bool>.CreateSuccess(true);
    }

    public async Task<ServiceResult<AuthResult>> AutoLoginAsync(StoredCredentials credentials)
    {
        var user = await _userRepository.GetByIdAsync(credentials.UserId);
        if (user == null || user.Username != credentials.Username || 
            user.PasswordHash != credentials.HashedPassword)
        {
            return ServiceResult<AuthResult>.CreateError("Stored credentials are invalid");
        }

        return ServiceResult<AuthResult>.CreateSuccess(MapToAuthResult(user));
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
            GptModel = user.GptModel
        };
    }
}