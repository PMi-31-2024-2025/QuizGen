using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using QuizGen.BLL.Models.QuizTry;
using QuizGen.BLL.Services.Interfaces;
using QuizGen.Presentation.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Graphics;
using Windows.UI;

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
                _ = await _quizTryService.DeleteQuizTryAsync(_quizTryId);
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
            ProgressText.Text = $"{index * 100 / _quizDetails.TotalQuestions:F0}%";

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
        _ = await _quizTryService.SaveAnswersAsync(_quizTryId, currentQuestion.Id, selectedAnswerIds);
    }

    private async Task FinishQuiz()
    {
        try
        {
            LoadingOverlay.Visibility = Visibility.Visible;
            var result = await _quizTryService.CalculateAndSaveScoreAsync(_quizTryId);

            if (result.Success)
            {
                _isFinished = true;
                var resultWindow = new QuizResultWindow(result.Data);
                resultWindow.Activate();
                Close();
            }
            else
            {
                await ShowError("Failed to finish quiz", result.Message);
            }
        }
        catch (Exception ex)
        {
            await ShowError("Failed to finish quiz", ex.Message);
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
        _ = await dialog.ShowAsync();
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
}