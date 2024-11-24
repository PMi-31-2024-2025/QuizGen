using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using QuizGen.BLL.Services.Interfaces;
using QuizGen.Presentation.Views.Windows;
using System;

namespace QuizGen.Presentation.Views.Pages;

public sealed partial class SettingsPage : Page
{
    private readonly IAuthStateService _authStateService;
    private readonly IAuthService _authService;
    private readonly IServiceProvider _serviceProvider;

    public string OpenAiKey { get; set; } = string.Empty;
    public string GptModel { get; set; } = string.Empty;

    public SettingsPage()
    {
        InitializeComponent();
        _serviceProvider = ((App)Application.Current).ServiceProvider;
        _authStateService = _serviceProvider.GetRequiredService<IAuthStateService>();
        _authService = _serviceProvider.GetRequiredService<IAuthService>();

        LoadUserSettings();
    }

    private async void LoadUserSettings()
    {
        if (_authStateService.CurrentCredentials != null)
        {
            var userResult = await _authService.AutoLoginAsync(_authStateService.CurrentCredentials);
            if (userResult.Success)
            {
                var user = userResult.Data;
                NameBox.Text = user.Name;
                UsernameBox.Text = user.Username;
                OpenAiKey = user.OpenAiApiKey ?? string.Empty;
                OpenAiKeyBox.Password = OpenAiKey;
                GptModel = user.GptModel;
                ModelComboBox.SelectedValue = GptModel;
            }
        }
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (_authStateService.CurrentCredentials != null)
        {
            var result = await _authService.UpdateProfileAsync(
                _authStateService.CurrentCredentials.UserId,
                NameBox.Text,
                OpenAiKeyBox.Password,
                ModelComboBox.SelectedValue?.ToString() ?? "gpt-4o-mini"
            );

            if (result.Success)
            {
                var dialog = new ContentDialog
                {
                    Title = "Success",
                    Content = "Settings saved successfully",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                _ = await dialog.ShowAsync();
            }
        }
    }

    private async void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        _authStateService.ClearCredentials();
        await _authStateService.SaveStateAsync();

        var loginWindow = new LoginWindow(_serviceProvider);
        loginWindow.Activate();

        var mainWindow = App.MainWindow;
        if (mainWindow != null)
        {
            mainWindow.Close();
            App.MainWindow = loginWindow;
        }
    }
}