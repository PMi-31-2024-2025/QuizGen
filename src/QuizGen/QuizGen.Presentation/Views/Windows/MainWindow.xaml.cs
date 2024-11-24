using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Windowing;
using Windows.Graphics;
using System;
using QuizGen.Presentation.Views.Pages;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using QuizGen.Presentation.Helpers;

namespace QuizGen.Presentation.Views.Windows;

public sealed partial class MainWindow : Window
{
    internal readonly IServiceProvider ServiceProvider;
    private InfoBar _toastNotification;

    public MainWindow(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        ServiceProvider = serviceProvider;
        InitializeWindow();
        InitializeToastNotification();
        
        // Navigate to home page by default
        NavigateTo("CreateQuizPage");
    }

    public static MainWindow Instance { get; private set; }

    private void InitializeWindow()
    {
        Instance = this;
        var appWindow = GetAppWindow();
        if (appWindow != null)
        {
            // Set minimum size (16:9 ratio)
            WindowMinSizeHelper.SetMinSize(this, 960, 540);

            appWindow.SetPresenter(AppWindowPresenterKind.Default);

            // Get the screen dimensions
            var displayArea = DisplayArea.GetFromWindowId(appWindow.Id, DisplayAreaFallback.Primary);
            var screenHeight = displayArea.OuterBounds.Height;
            var screenWidth = displayArea.OuterBounds.Width;

            // Calculate window dimensions (80% of screen height with 16:9 ratio)
            var windowHeight = (int)(screenHeight * 0.8);
            var windowWidth = (int)(windowHeight * (16.0 / 9.0));

            // Resize window
            appWindow.Resize(new SizeInt32(windowWidth, windowHeight));

            // Center the window
            var x = (screenWidth - windowWidth) / 2;
            var y = (screenHeight - windowHeight) / 2;
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

    private AppWindow GetAppWindow()
    {
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
        return AppWindow.GetFromWindowId(windowId);
    }

    private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.IsSettingsSelected)
        {
            ContentFrame.Navigate(typeof(SettingsPage));
            return;
        }

        if (args.SelectedItem is NavigationViewItem selectedItem)
        {
            NavigateTo(selectedItem.Tag.ToString());
        }
    }

    public void NavigateTo(string tag)
    {
        // Find the navigation item with matching tag
        var menuItem = NavView.MenuItems
            .OfType<NavigationViewItem>()
            .FirstOrDefault(item => item.Tag.ToString() == tag);

        if (menuItem != null)
        {
            NavView.SelectedItem = menuItem;
            
            switch (tag)
            {
                case "CreateQuizPage":
                    ContentFrame.Navigate(typeof(CreateQuizPage));
                    break;
                case "MyQuizzesPage":
                    ContentFrame.Navigate(typeof(MyQuizzesPage));
                    break;
                case "QuizHistoryPage":
                    ContentFrame.Navigate(typeof(QuizHistoryPage));
                    break;
            }
        }
    }

    private void InitializeToastNotification()
    {
        _toastNotification = new InfoBar
        {
            IsOpen = false,
            Severity = InfoBarSeverity.Success,
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 24, 0, 0),
            MaxWidth = 400,
            IsClosable = false,
            Background = Application.Current.Resources["CardBackgroundFillColorDefaultBrush"] as Brush,
            BorderBrush = Application.Current.Resources["CardStrokeColorDefaultBrush"] as Brush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8)
        };

        // Add the InfoBar to the root Grid, above NavigationView
        ((Grid)Content).Children.Insert(0, _toastNotification);
    }

    public void ShowToast(string message, InfoBarSeverity severity = InfoBarSeverity.Success)
    {
        _toastNotification.Message = message;
        _toastNotification.Severity = severity;
        _toastNotification.IsOpen = true;

        // Auto-hide after 3 seconds
        DispatcherQueue.TryEnqueue(async () =>
        {
            await Task.Delay(3000);
            _toastNotification.IsOpen = false;
        });
    }
}
