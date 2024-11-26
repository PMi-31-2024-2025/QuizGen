using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using QuizGen.BLL.Models.Auth;
using QuizGen.BLL.Services.Interfaces;
using System;
using System.Security.Cryptography;
using System.Text;
using Windows.Graphics;
using Windows.UI;

namespace QuizGen.Presentation.Views.Windows;

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
        InitializeWindow();
    }

    private void InitializeWindow()
    {
        var appWindow = GetAppWindow();
        if (appWindow != null)
        {
            appWindow.SetPresenter(AppWindowPresenterKind.Default);

            // Set fixed size and disable resizing
            appWindow.Resize(new SizeInt32(800, 1200));
            var overlappedPresenter = appWindow.Presenter as OverlappedPresenter;
            if (overlappedPresenter != null)
            {
                overlappedPresenter.IsResizable = false;
                overlappedPresenter.IsMaximizable = false;
            }

            // Center on screen
            var displayArea = DisplayArea.GetFromWindowId(appWindow.Id, DisplayAreaFallback.Primary);
            var x = (displayArea.OuterBounds.Width - 800) / 2;
            var y = (displayArea.OuterBounds.Height - 1200) / 2;
            appWindow.Move(new PointInt32(x, y));

            if (Microsoft.UI.Composition.SystemBackdrops.MicaController.IsSupported())
            {
                this.SystemBackdrop = new Microsoft.UI.Xaml.Media.MicaBackdrop();

                // Enable transparency for titlebar
                var titleBar = appWindow.TitleBar;
                titleBar.ExtendsContentIntoTitleBar = true;
                titleBar.ButtonBackgroundColor = Color.FromArgb(0, 0, 0, 0);
                titleBar.ButtonInactiveBackgroundColor = Color.FromArgb(0, 0, 0, 0);
            }
        }
    }

    private void ValidateLoginFields(object sender, object e)
    {
        LoginButton.IsEnabled = !string.IsNullOrWhiteSpace(LoginUsernameBox.Text) &&
                               !string.IsNullOrWhiteSpace(LoginPasswordBox.Password);
    }

    private void ValidateRegisterFields(object sender, object e)
    {
        RegisterButton.IsEnabled = !string.IsNullOrWhiteSpace(RegisterNameBox.Text) &&
                                 !string.IsNullOrWhiteSpace(RegisterUsernameBox.Text) &&
                                 !string.IsNullOrWhiteSpace(RegisterPasswordBox.Password) &&
                                 TermsCheckBox.IsChecked == true;
    }

    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        ErrorText.Text = string.Empty;
        try
        {
            var result = await _authService.LoginAsync(new LoginRequest
            {
                Username = LoginUsernameBox.Text,
                Password = LoginPasswordBox.Password
            });

            if (result.Success)
            {
                var credentials = new StoredCredentials
                {
                    UserId = result.Data.UserId,
                    Username = result.Data.Username,
                    HashedPassword = HashPassword(LoginPasswordBox.Password)
                };

                _authStateService.SetCredentials(credentials);
                await _authStateService.SaveStateAsync();

                var mainWindow = new MainWindow(_serviceProvider);
                App.MainWindow = mainWindow;
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
            ErrorText.Text = ex.Message;
        }
    }

    private async void RegisterButton_Click(object sender, RoutedEventArgs e)
    {
        ErrorText.Text = string.Empty;
        RegisterButton.IsEnabled = false;

        try
        {
            var result = await _authService.RegisterAsync(new RegisterRequest
            {
                Name = RegisterNameBox.Text,
                Username = RegisterUsernameBox.Text,
                Password = RegisterPasswordBox.Password
            });

            if (result.Success)
            {
                var loginResult = await _authService.LoginAsync(new LoginRequest
                {
                    Username = RegisterUsernameBox.Text,
                    Password = RegisterPasswordBox.Password
                });

                if (loginResult.Success)
                {
                    var credentials = new StoredCredentials
                    {
                        UserId = loginResult.Data.UserId,
                        Username = loginResult.Data.Username,
                        HashedPassword = HashPassword(RegisterPasswordBox.Password)
                    };

                    _authStateService.SetCredentials(credentials);
                    await _authStateService.SaveStateAsync();

                    var mainWindow = new MainWindow(_serviceProvider);
                    App.MainWindow = mainWindow;
                    mainWindow.Activate();
                    Close();
                }
            }
            else
            {
                ErrorText.Text = result.Message;
            }
        }
        catch (Exception ex)
        {
            ErrorText.Text = ex.Message;
        }
        finally
        {
            RegisterButton.IsEnabled = true;
        }
    }

    private AppWindow GetAppWindow()
    {
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
        return AppWindow.GetFromWindowId(windowId);
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private void ShowRegisterButton_Click(object sender, RoutedEventArgs e)
    {
        LoginForm.Visibility = Visibility.Collapsed;
        RegisterForm.Visibility = Visibility.Visible;
        ErrorText.Text = string.Empty;
    }

    private void ShowLoginButton_Click(object sender, RoutedEventArgs e)
    {
        RegisterForm.Visibility = Visibility.Collapsed;
        LoginForm.Visibility = Visibility.Visible;
        ErrorText.Text = string.Empty;
    }
}