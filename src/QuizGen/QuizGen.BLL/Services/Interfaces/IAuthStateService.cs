using QuizGen.BLL.Models.Auth;

namespace QuizGen.BLL.Services.Interfaces;

public interface IAuthStateService
{
    StoredCredentials? CurrentCredentials { get; }
    bool IsAuthenticated { get; }
    void SetCredentials(StoredCredentials credentials);
    void ClearCredentials();
    Task LoadSavedStateAsync();
    Task SaveStateAsync();
} 