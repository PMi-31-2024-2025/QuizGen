using QuizGen.BLL.Configuration;
using QuizGen.BLL.Services.Interfaces;
using System.Text.Json;

namespace QuizGen.BLL.Services
{
    public class AuthStateService : IAuthStateService
    {
        private StoredCredentials? _currentCredentials;
        private readonly string _settingsPath;

        public AuthStateService(AppConfig config)
        {
            _settingsPath = Path.Combine(config.LocalSettingsPath, "credentials.json");
        }

        public StoredCredentials? CurrentCredentials => _currentCredentials;
        public bool IsAuthenticated => _currentCredentials != null;

        public void SetCredentials(StoredCredentials credentials)
        {
            _currentCredentials = credentials;
        }

        public void ClearCredentials()
        {
            _currentCredentials = null;
        }

        public async Task LoadSavedStateAsync()
        {
            if (File.Exists(_settingsPath))
            {
                var json = await File.ReadAllTextAsync(_settingsPath);
                _currentCredentials = JsonSerializer.Deserialize<StoredCredentials>(json);
            }
        }

        public async Task SaveStateAsync()
        {
            var directory = Path.GetDirectoryName(_settingsPath);
            if (!Directory.Exists(directory))
                _ = Directory.CreateDirectory(directory);

            var json = JsonSerializer.Serialize(_currentCredentials);
            await File.WriteAllTextAsync(_settingsPath, json);
        }
    }
}