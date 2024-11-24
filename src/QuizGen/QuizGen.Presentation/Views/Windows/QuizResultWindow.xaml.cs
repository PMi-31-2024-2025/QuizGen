using Microsoft.UI.Text;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using QuizGen.BLL.Models.QuizTry;
using QuizGen.Presentation.Helpers;
using Windows.Graphics;
using Windows.UI;

namespace QuizGen.Presentation.Views.Windows;

public sealed partial class QuizResultWindow : Window
{
    public QuizResultWindow(QuizTryResultDto result)
    {
        InitializeComponent();
        InitializeWindow();
        DisplayResults(result);
    }

    private void InitializeWindow()
    {
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

    private void DisplayResults(QuizTryResultDto result)
    {
        // Update score display
        ScoreRing.Value = result.Score;
        ScoreRing.Foreground = new SolidColorBrush(
            result.Score >= 50
                ? Color.FromArgb(255, 78, 175, 74)
                : Color.FromArgb(255, 249, 82, 82));

        ScorePercentText.Text = $"{result.Score:F0}%";
        ScoreResultText.Text = result.Score >= 50 ? "Quiz Passed" : "Quiz Failed";

        // Display question review
        foreach (var question in result.Questions)
        {
            var questionPanel = new StackPanel
            {
                Spacing = 8,
                Background = Application.Current.Resources["CardBackgroundFillColorDefaultBrush"] as Brush,
                BorderBrush = Application.Current.Resources["CardStrokeColorDefaultBrush"] as Brush,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(16),
                CornerRadius = new CornerRadius(8)
            };

            // Question header with icon and score
            var headerPanel = new Grid();
            headerPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            headerPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var icon = new FontIcon
            {
                Glyph = question.IsCorrect ? "\uE73E" : "\uE711",
                Foreground = new SolidColorBrush(
                    question.IsCorrect
                        ? Color.FromArgb(255, 78, 175, 74)
                        : Color.FromArgb(255, 249, 82, 82)),
                FontSize = 20,
                Margin = new Thickness(0, 0, 16, 0)
            };
            Grid.SetColumn(icon, 0);

            var questionText = new TextBlock
            {
                Text = question.Text,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 600,
                Style = Application.Current.Resources["BodyStrongTextBlockStyle"] as Style
            };
            Grid.SetColumn(questionText, 1);

            var scoreText = new TextBlock
            {
                Text = $"+{question.Score:F1}% to total score ({question.CorrectPercentage:F1}% correct)",
                Foreground = new SolidColorBrush(
                    question.Score > 0
                        ? Color.FromArgb(180, 78, 175, 74)
                        : Color.FromArgb(180, 255, 255, 255)),
                Style = Application.Current.Resources["CaptionTextBlockStyle"] as Style,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(16, 2, 0, 0),
                Opacity = question.Score > 0 ? 1 : 0.7
            };
            Grid.SetColumn(scoreText, 2);

            headerPanel.Children.Add(icon);
            headerPanel.Children.Add(questionText);
            headerPanel.Children.Add(scoreText);

            questionPanel.Children.Add(headerPanel);

            // Add answers review
            var answersPanel = new StackPanel
            {
                Spacing = 8,
                Margin = new Thickness(32, 8, 0, 0)
            };

            foreach (var answer in question.Answers)
            {
                var answerStackPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 8
                };

                var answerText = new TextBlock
                {
                    Text = answer.Text,
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = new SolidColorBrush(
                        answer.IsCorrect ? Color.FromArgb(255, 78, 175, 74) :
                        Color.FromArgb(255, 128, 128, 128)),
                    Style = Application.Current.Resources["BodyTextBlockStyle"] as Style
                };
                answerStackPanel.Children.Add(answerText);

                if (answer.WasSelected)
                {
                    var yourAnswerText = new TextBlock
                    {
                        Text = "(your answer)",
                        Foreground = new SolidColorBrush(Color.FromArgb(255, 128, 128, 128)),
                        Style = Application.Current.Resources["CaptionTextBlockStyle"] as Style,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(8, 0, 0, 0),
                        Opacity = 0.7
                    };
                    answerStackPanel.Children.Add(yourAnswerText);
                }

                answersPanel.Children.Add(answerStackPanel);
            }
            questionPanel.Children.Add(answersPanel);

            // Add explanation if available
            if (!string.IsNullOrEmpty(question.Explanation))
            {
                var explanationPanel = new StackPanel
                {
                    Spacing = 4,
                    Margin = new Thickness(32, 8, 0, 0)
                };

                explanationPanel.Children.Add(new TextBlock
                {
                    Text = "Explanation:",
                    FontWeight = FontWeights.SemiBold,
                    Style = Application.Current.Resources["CaptionTextBlockStyle"] as Style
                });

                explanationPanel.Children.Add(new TextBlock
                {
                    Text = question.Explanation,
                    TextWrapping = TextWrapping.Wrap,
                    Style = Application.Current.Resources["CaptionTextBlockStyle"] as Style,
                    Opacity = 0.8
                });

                questionPanel.Children.Add(explanationPanel);
            }

            QuestionsReviewPanel.Children.Add(questionPanel);
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private AppWindow GetAppWindow()
    {
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
        return AppWindow.GetFromWindowId(windowId);
    }
}