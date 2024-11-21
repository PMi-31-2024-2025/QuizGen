using QuizGen.BLL.Models.Auth;

namespace QuizGen.BLL.Services.Interfaces;

public interface IAuthStateService
{
    AuthResult? CurrentUser { get; }
    bool IsAuthenticated { get; }
    void SetCurrentUser(AuthResult user);
    void ClearCurrentUser();
    Task LoadSavedStateAsync();
    Task SaveStateAsync();
} 