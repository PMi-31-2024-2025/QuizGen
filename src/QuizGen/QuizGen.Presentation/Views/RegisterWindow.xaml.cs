using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using QuizGen.BLL.Models.Auth;
using QuizGen.BLL.Services.Interfaces;
using System;

namespace QuizGen.Presentation.Views;

public sealed partial class RegisterWindow : Window
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAuthService _authService;
    private readonly IAuthStateService _authStateService;

    public RegisterWindow(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
        _authService = serviceProvider.GetRequiredService<IAuthService>();
        _authStateService = serviceProvider.GetRequiredService<IAuthStateService>();
    }

    private async void RegisterButton_Click(object sender, RoutedEventArgs e)
    {
        ErrorText.Text = string.Empty;
        RegisterButton.IsEnabled = false;

        try
        {
            if (string.IsNullOrWhiteSpace(UsernameBox.Text) || 
                string.IsNullOrWhiteSpace(PasswordBox.Password) ||
                string.IsNullOrWhiteSpace(NameBox.Text))
            {
                ErrorText.Text = "Please fill in all required fields";
                return;
            }

            var result = await _authService.RegisterAsync(new RegisterRequest
            {
                Username = UsernameBox.Text.Trim(),
                Password = PasswordBox.Password,
                Name = NameBox.Text.Trim(),
                OpenAiApiKey = string.IsNullOrWhiteSpace(OpenAiKeyBox.Text) ? null : OpenAiKeyBox.Text.Trim(),
                UseLocalModel = UseLocalModelCheck.IsChecked ?? false
            });

            if (result.Success)
            {
                _authStateService.SetCurrentUser(result.Data);
                await _authStateService.SaveStateAsync();

                var mainWindow = new MainWindow(_serviceProvider);
                mainWindow.Activate();
                Close();
            }
            else
            {
                ErrorText.Text = result.Message;
            }
        }
        catch (Exception ex)
        {
            ErrorText.Text = $"Registration error: {ex.Message}";
        }
        finally
        {
            RegisterButton.IsEnabled = true;
        }
    }

    private void LoginLink_Click(object sender, RoutedEventArgs e)
    {
        var loginWindow = new LoginWindow(_serviceProvider);
        loginWindow.Activate();
        Close();
    }
} 