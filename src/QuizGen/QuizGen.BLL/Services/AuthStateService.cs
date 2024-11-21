using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using QuizGen.BLL.Configuration;
using QuizGen.BLL.Models.Auth;
using QuizGen.BLL.Services.Interfaces;

namespace QuizGen.BLL.Services
{
    public class AuthStateService : IAuthStateService
    {
        private AuthResult _currentUser;
        private readonly string _settingsPath;

        public AuthStateService(AppConfig config)
        {
            _settingsPath = Path.Combine(config.LocalSettingsPath, "auth.json");
        }

        public AuthResult CurrentUser => _currentUser;
        public bool IsAuthenticated => _currentUser != null;

        public void SetCurrentUser(AuthResult user)
        {
            _currentUser = user;
        }

        public void ClearCurrentUser()
        {
            _currentUser = null;
        }

        public async Task LoadSavedStateAsync()
        {
            if (File.Exists(_settingsPath))
            {
                var json = await File.ReadAllTextAsync(_settingsPath);
                _currentUser = JsonSerializer.Deserialize<AuthResult>(json);
            }
        }

        public async Task SaveStateAsync()
        {
            var directory = Path.GetDirectoryName(_settingsPath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var json = JsonSerializer.Serialize(_currentUser);
            await File.WriteAllTextAsync(_settingsPath, json);
        }
    }
} 