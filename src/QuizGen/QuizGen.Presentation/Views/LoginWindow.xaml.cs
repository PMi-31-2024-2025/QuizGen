using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using QuizGen.BLL.Models.Auth;
using QuizGen.BLL.Services.Interfaces;
using System;


namespace QuizGen.Presentation.Views;

public sealed partial class LoginWindow : Window
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAuthService _authService;
    private readonly IAuthStateService _authStateService;

    public LoginWindow(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
        _authService = serviceProvider.GetRequiredService<IAuthService>();
        _authStateService = serviceProvider.GetRequiredService<IAuthStateService>();
    }

    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        ErrorText.Text = string.Empty;
        LoginButton.IsEnabled = false;

        try
        {
            var result = await _authService.LoginAsync(new LoginRequest
            {
                Username = UsernameBox.Text,
                Password = PasswordBox.Password
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
            ErrorText.Text = "An error occurred during login. Please try again.";
        }
        finally
        {
            LoginButton.IsEnabled = true;
        }
    }

    private void RegisterLink_Click(object sender, RoutedEventArgs e)
    {
        var registerWindow = new RegisterWindow(_serviceProvider);
        registerWindow.Activate();
        Close();
    }
} 