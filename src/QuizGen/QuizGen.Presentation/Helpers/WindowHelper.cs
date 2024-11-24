using Microsoft.UI.Xaml;
using QuizGen.Presentation.Views.Windows;
using System.Collections.Generic;

namespace QuizGen.Presentation.Helpers;

public static class WindowHelper
{
    public static Window MainWindow { get; private set; }
    private static readonly List<Window> _windows = new();

    public static void TrackWindow(Window window)
    {
        _windows.Add(window);
        if (window is MainWindow)
        {
            MainWindow = window;
        }
    }

    public static void CloseMainWindow()
    {
        MainWindow?.Close();
    }
}