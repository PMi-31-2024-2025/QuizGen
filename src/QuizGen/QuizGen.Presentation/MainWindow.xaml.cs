using Microsoft.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;
using QuizGen.BLL.Services.Interfaces;
using QuizGen.Presentation.Views;
using QuizGen.DAL.Extensions;
using QuizGen.BLL.Extensions;
using System;

namespace QuizGen.Presentation;

public sealed partial class MainWindow : Window
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAuthStateService _authStateService;

    public MainWindow(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
        _authStateService = serviceProvider.GetRequiredService<IAuthStateService>();

        InitializeUser();
    }

    private void InitializeUser()
    {
        if (_authStateService.CurrentUser != null)
        {
            UserNameText.Text = $"Welcome, {_authStateService.CurrentUser.Name}!";
        }
    }

    private async void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        _authStateService.ClearCurrentUser();
        await _authStateService.SaveStateAsync();

        var loginWindow = new LoginWindow(_serviceProvider);
        loginWindow.Activate();
        Close();
    }
}
