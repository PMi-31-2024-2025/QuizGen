using Microsoft.UI.Xaml;
using System;
using Microsoft.Extensions.DependencyInjection;
using QuizGen.BLL.Configuration;
using QuizGen.BLL.Services.Interfaces;
using QuizGen.Presentation.Views;
using QuizGen.DAL.Extensions;
using QuizGen.BLL.Extensions;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QuizGen.Presentation
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window m_window;
        internal readonly IServiceProvider ServiceProvider;
        private readonly IAuthStateService _authStateService;

        public static Window MainWindow { get; set; }

        public App()
        {
            InitializeComponent();

            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();
            
            _authStateService = ServiceProvider.GetRequiredService<IAuthStateService>();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            var config = new AppConfig
            {
                DatabaseConnectionString = "Server=localhost;Port=5432;Database=quizgen;User Id=postgres;Password=1111;",
                LocalSettingsPath = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "QuizGen")
            };

            services.AddDataAccessLayer(config.DatabaseConnectionString);
            services.AddBusinessLogicLayer(config);
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            await _authStateService.LoadSavedStateAsync();

            if (_authStateService.IsAuthenticated)
            {
                var authService = ServiceProvider.GetRequiredService<IAuthService>();
                var result = await authService.AutoLoginAsync(_authStateService.CurrentCredentials!);
                
                if (result.Success)
                {
                    m_window = new MainWindow(ServiceProvider);
                }
                else
                {
                    _authStateService.ClearCredentials();
                    await _authStateService.SaveStateAsync();
                    m_window = new LoginWindow(ServiceProvider);
                }
            }
            else
            {
                m_window = new LoginWindow(ServiceProvider);
            }

            MainWindow = m_window;
            m_window.Activate();
        }
    }
}
