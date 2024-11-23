using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Windowing;
using Windows.Graphics;
using System;
using QuizGen.Presentation.Views.Pages;

namespace QuizGen.Presentation;

public sealed partial class MainWindow : Window
{
    internal readonly IServiceProvider ServiceProvider;

    public MainWindow(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        ServiceProvider = serviceProvider;
        InitializeWindow();
        
        // Navigate to home page by default
        NavView.SelectedItem = NavView.MenuItems[0];
        ContentFrame.Navigate(typeof(CreateQuizPage));
    }

    private void InitializeWindow()
    {
        var appWindow = GetAppWindow();
        if (appWindow != null)
        {
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
                titleBar.ButtonBackgroundColor = Windows.UI.Color.FromArgb(0, 0, 0, 0);
                titleBar.ButtonInactiveBackgroundColor = Windows.UI.Color.FromArgb(0, 0, 0, 0);
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
            string tag = selectedItem.Tag.ToString();

            switch (tag)
            {
                case "CreateQuizPage":
                    ContentFrame.Navigate(typeof(CreateQuizPage));
                    break;
            }
        }
    }
}
