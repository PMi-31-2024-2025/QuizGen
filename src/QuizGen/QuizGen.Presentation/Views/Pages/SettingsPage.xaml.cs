using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using QuizGen.BLL.Services.Interfaces;
using System;
using QuizGen.BLL.Models.Auth;

namespace QuizGen.Presentation.Views.Pages;

public sealed partial class SettingsPage : Page
{
    private readonly IAuthStateService _authStateService;
    private readonly IAuthService _authService;
    private readonly IServiceProvider _serviceProvider;

    public SettingsPage()
    {
        InitializeComponent();
        _serviceProvider = ((App)Application.Current).ServiceProvider;
        _authStateService = _serviceProvider.GetRequiredService<IAuthStateService>();
        _authService = _serviceProvider.GetRequiredService<IAuthService>();

        LoadUserSettings();
    }

    private void LoadUserSettings()
    {
        var user = _authStateService.CurrentUser;
        if (user != null)
        {
            NameBox.Text = user.Name;
            UsernameBox.Text = user.Username;
            UseLocalModelSwitch.IsOn = user.UseLocalModel;
            OpenAiKeyBox.Text = user.OpenAiApiKey ?? string.Empty;
        }
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var user = _authStateService.CurrentUser;
        if (user != null)
        {
            var result = await _authService.UpdateProfileAsync(
                user.UserId,
                NameBox.Text,
                OpenAiKeyBox.Text,
                UseLocalModelSwitch.IsOn
            );

            if (result.Success)
            {
                // Refresh current user data
                var loginResult = await _authService.LoginAsync(new LoginRequest 
                { 
                    Username = user.Username,
                    Password = "" // We don't need password for refresh
                });

                if (loginResult.Success)
                {
                    _authStateService.SetCurrentUser(loginResult.Data);
                    await _authStateService.SaveStateAsync();

                    var dialog = new ContentDialog
                    {
                        Title = "Settings Saved",
                        Content = "Your settings have been updated successfully.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await dialog.ShowAsync();
                }
            }
        }
    }

    private async void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        _authStateService.ClearCurrentUser();
        await _authStateService.SaveStateAsync();

        var loginWindow = new LoginWindow(_serviceProvider);
        loginWindow.Activate();
        
        App.MainWindow?.Close();
    }
}