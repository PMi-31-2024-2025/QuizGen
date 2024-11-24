using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using QuizGen.BLL.Models.QuizTry;
using QuizGen.BLL.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using Windows.Graphics;
using Microsoft.UI.Text;

namespace QuizGen.Presentation.Views.Windows;

public sealed partial class QuizWindow : Window
{
    private readonly IQuizTryService _quizTryService;
    private readonly IAuthStateService _authStateService;
    private readonly int _quizId;
    private int _quizTryId;
    private int _currentQuestionIndex;
    private QuizTryDetailsDto _quizDetails;
    private bool _isFinished = false;
    private bool _isClosing = false;

    public QuizWindow(int quizId)
    {
        InitializeComponent();
        
        var serviceProvider = ((App)Application.Current).ServiceProvider;
        _quizTryService = serviceProvider.GetRequiredService<IQuizTryService>();
        _authStateService = serviceProvider.GetRequiredService<IAuthStateService>();
        
        _quizId = quizId;
        InitializeWindow();
        LoadQuiz();
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
                titleBar.ButtonBackgroundColor = Color.FromArgb(0, 0, 0, 0);
                titleBar.ButtonInactiveBackgroundColor = Color.FromArgb(0, 0, 0, 0);
            }
        }

        // Handle closing
        this.Closed += QuizWindow_Closed;
    }

    private async void QuizWindow_Closed(object sender, WindowEventArgs args)
    {
        if (!_isFinished && _quizTryId != 0 && !_isClosing)
        {
            args.Handled = true; // Prevent immediate closing
            
            var dialog = new ContentDialog
            {
                Title = "Quit Quiz?",
                Content = "All progress will be lost. Are you sure you want to quit?",
                PrimaryButtonText = "Quit",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = Content.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                _isClosing = true; // Prevent recursive calls
                await _quizTryService.DeleteQuizTryAsync(_quizTryId);
                this.Close();
            }
        }
    }

    private async void LoadQuiz()
    {
        try
        {
            LoadingOverlay.Visibility = Visibility.Visible;

            var startResult = await _quizTryService.StartQuizTryAsync(
                _quizId, 
                _authStateService.CurrentCredentials?.UserId ?? 
                    throw new InvalidOperationException("User not authenticated")
            );

            if (startResult.Success)
            {
                _quizTryId = startResult.Data.Id;
                var detailsResult = await _quizTryService.GetQuizTryDetailsAsync(_quizTryId);
                
                if (detailsResult.Success)
                {
                    _quizDetails = detailsResult.Data;
                    UpdateQuizInfoView();
                }
                else
                {
                    await ShowError("Failed to load quiz details", detailsResult.Message);
                    Close();
                }
            }
            else
            {
                await ShowError("Failed to start quiz", startResult.Message);
                Close();
            }
        }
        catch (Exception ex)
        {
            await ShowError("Failed to load quiz", ex.Message);
            Close();
        }
        finally
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;
        }
    }

    private void UpdateQuizInfoView()
    {
        QuizNameText.Text = _quizDetails.QuizName;
        QuizPromptText.Text = _quizDetails.QuizPrompt;
        DifficultyValueText.Text = _quizDetails.Difficulty;
        QuestionCountValueText.Text = _quizDetails.TotalQuestions.ToString();
        
        QuizProgress.Value = 0;
        QuizProgress.Maximum = _quizDetails.TotalQuestions;
    }

    private async void StartButton_Click(object sender, RoutedEventArgs e)
    {
        QuizInfoView.Visibility = Visibility.Collapsed;
        QuestionView.Visibility = Visibility.Visible;
        ProgressSection.Visibility = Visibility.Visible;
        await ShowQuestion(0);
    }

    private async Task ShowQuestion(int index)
    {
        if (index >= 0 && index < _quizDetails.TotalQuestions)
        {
            _currentQuestionIndex = index;
            var question = _quizDetails.Questions[index];
            
            QuestionText.Text = question.Text;
            QuestionCountText.Text = $"Question {index + 1} of {_quizDetails.TotalQuestions}";
            QuizProgress.Value = index;
            ProgressText.Text = $"{(index * 100 / _quizDetails.TotalQuestions):F0}%";

            AnswersPanel.Children.Clear();

            foreach (var answer in question.AnswerOptions)
            {
                if (question.Type == "multi-select")
                {
                    var checkBox = new CheckBox
                    {
                        Content = answer.Text,
                        Tag = answer.Id,
                        IsChecked = answer.IsSelected,
                        Margin = new Thickness(0)
                    };
                    checkBox.Checked += (s, e) => UpdateNavigationButtons();
                    checkBox.Unchecked += (s, e) => UpdateNavigationButtons();
                    AnswersPanel.Children.Add(checkBox);
                }
                else // single-select or true-false
                {
                    var radioButton = new RadioButton
                    {
                        Content = answer.Text,
                        Tag = answer.Id,
                        IsChecked = answer.IsSelected,
                        Margin = new Thickness(0)
                    };
                    radioButton.Checked += (s, e) => UpdateNavigationButtons();
                    AnswersPanel.Children.Add(radioButton);
                }
            }

            UpdateNavigationButtons();
        }
    }

    private async void BackButton_Click(object sender, RoutedEventArgs e)
    {
        await SaveCurrentAnswers();
        await ShowQuestion(_currentQuestionIndex - 1);
    }

    private async void NextButton_Click(object sender, RoutedEventArgs e)
    {
        await SaveCurrentAnswers();

        if (_currentQuestionIndex == _quizDetails.TotalQuestions - 1)
        {
            await FinishQuiz();
        }
        else
        {
            await ShowQuestion(_currentQuestionIndex + 1);
        }
    }

    private async Task SaveCurrentAnswers()
    {
        var currentQuestion = _quizDetails.Questions[_currentQuestionIndex];
        var selectedAnswerIds = new List<int>();

        foreach (var child in AnswersPanel.Children)
        {
            if (child is RadioButton radio && radio.IsChecked == true)
            {
                selectedAnswerIds.Add((int)radio.Tag);
            }
            else if (child is CheckBox checkbox && checkbox.IsChecked == true)
            {
                selectedAnswerIds.Add((int)checkbox.Tag);
            }
        }

        // Update the question's selected answers
        foreach (var answer in currentQuestion.AnswerOptions)
        {
            answer.IsSelected = selectedAnswerIds.Contains(answer.Id);
        }

        // Save answers to database
        await _quizTryService.SaveAnswersAsync(_quizTryId, currentQuestion.Id, selectedAnswerIds);
    }

    private async Task FinishQuiz()
    {
        try
        {
            LoadingOverlay.Visibility = Visibility.Visible;
            await SaveCurrentAnswers();

            var result = await _quizTryService.CalculateAndSaveScoreAsync(_quizTryId);
            if (result.Success)
            {
                _isFinished = true;
                QuestionView.Visibility = Visibility.Collapsed;
                ProgressSection.Visibility = Visibility.Collapsed;
                ResultsView.Visibility = Visibility.Visible;
                
                // Update score display
                var score = result.Data.Score;
                ScoreRing.Value = score;
                ScoreRing.Foreground = new SolidColorBrush(
                    score >= 50 
                        ? Color.FromArgb(255, 16, 124, 16)
                        : Color.FromArgb(255, 184, 0, 0));
                
                ScorePercentText.Text = $"{score:F0}%";
                ScoreResultText.Text = score >= 50 ? "Quiz Passed" : "Quiz Failed";
                
                // Display question review
                QuestionsReviewPanel.Children.Clear();
                foreach (var question in result.Data.Questions)
                {
                    var questionPanel = new StackPanel { Spacing = 16 };
                    
                    // Create header panel with icon and question text
                    var headerPanel = new Grid();
                    headerPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                    headerPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    headerPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                    // Add icon
                    var icon = new FontIcon
                    {
                        Glyph = question.IsCorrect ? "\uE73E" : "\uE711",
                        Foreground = new SolidColorBrush(
                            question.IsCorrect 
                                ? Color.FromArgb(255, 16, 124, 16)
                                : Color.FromArgb(255, 184, 0, 0)),
                        FontSize = 20,
                        VerticalAlignment = VerticalAlignment.Top,
                        Margin = new Thickness(0, 2, 8, 0)
                    };
                    Grid.SetColumn(icon, 0);

                    // Add question text
                    var questionText = new TextBlock
                    {
                        Text = question.Text,
                        TextWrapping = TextWrapping.Wrap,
                        MaxWidth = 600,
                        Style = Application.Current.Resources["BodyStrongTextBlockStyle"] as Style
                    };
                    Grid.SetColumn(questionText, 1);

                    // Add score text
                    var scoreText = new TextBlock
                    {
                        Text = question.Score > 0 
                            ? $"+{question.Score:F1}% to total score ({question.CorrectPercentage:F0}% correct)"
                            : $"+0% to total score (0% correct)",
                        Foreground = new SolidColorBrush(
                            question.Score > 0 
                                ? Color.FromArgb(255, 16, 124, 16)
                                : Color.FromArgb(255, 128, 128, 128)),
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

                    // Add answers review with no left margin
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
                                answer.IsCorrect ? Color.FromArgb(255, 16, 124, 16) : 
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
            else
            {
                var dialog = new ContentDialog
                {
                    Title = "Something went wrong!",
                    Content = $"Failed to calculate quiz results. {result.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = Content.XamlRoot
                };
                await dialog.ShowAsync();
            }
        }
        catch (Exception ex)
        {
            var dialog = new ContentDialog
            {
                Title = "Something went wrong!",
                Content = $"Failed to finish quiz. {ex.Message}",
                CloseButtonText = "OK",
                XamlRoot = Content.XamlRoot
            };
            await dialog.ShowAsync();
        }
        finally
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private async Task ShowError(string title, string message)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = Content.XamlRoot
        };
        await dialog.ShowAsync();
    }

    private AppWindow GetAppWindow()
    {
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
        return AppWindow.GetFromWindowId(windowId);
    }

    private void UpdateNavigationButtons()
    {
        BackButton.IsEnabled = _currentQuestionIndex > 0;
        
        bool hasSelection = false;
        foreach (var child in AnswersPanel.Children)
        {
            if (child is RadioButton radio && radio.IsChecked == true)
            {
                hasSelection = true;
                break;
            }
            else if (child is CheckBox checkbox && checkbox.IsChecked == true)
            {
                hasSelection = true;
                break;
            }
        }

        NextButton.IsEnabled = hasSelection;
        NextButton.Content = _currentQuestionIndex == _quizDetails.TotalQuestions - 1 
            ? "Finish Quiz" 
            : "Next Question";
    }

    // Continue with UpdateQuizInfoView and other methods...
} 