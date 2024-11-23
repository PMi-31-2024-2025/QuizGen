using QuizGen.BLL.Models.Auth;
using QuizGen.BLL.Models.Base;

namespace QuizGen.BLL.Services.Interfaces;

public interface IAuthService
{
    Task<ServiceResult<AuthResult>> LoginAsync(LoginRequest request);
    Task<ServiceResult<AuthResult>> AutoLoginAsync(StoredCredentials credentials);
    Task<ServiceResult<AuthResult>> RegisterAsync(RegisterRequest request);
    Task<ServiceResult<bool>> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    Task<ServiceResult<bool>> UpdateProfileAsync(int userId, string name, string openAiApiKey, string gptModel);
}