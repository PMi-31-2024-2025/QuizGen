using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using QuizGen.BLL.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using QuizGen.BLL.Models.Quiz;
using System.Threading.Tasks;
using System.Linq;
using QuizGen.BLL.Models.Base;
using QuizGen.Presentation.Views.Windows;
using Windows.Storage.Pickers;
using Windows.Storage;

namespace QuizGen.Presentation.Views.Pages;

public sealed partial class MyQuizzesPage : Page
{
    private readonly IQuizService _quizService;
    private readonly IAuthStateService _authStateService;
    private readonly IQuizExportService _quizExportService;

    public MyQuizzesPage()
    {
        InitializeComponent();
        
        var serviceProvider = ((App)Application.Current).ServiceProvider;
        _quizService = serviceProvider.GetRequiredService<IQuizService>();
        _authStateService = serviceProvider.GetRequiredService<IAuthStateService>();
        _quizExportService = serviceProvider.GetRequiredService<IQuizExportService>();

        Loaded += MyQuizzesPage_Loaded;
    }

    private async void MyQuizzesPage_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadQuizzes();
    }

    private string MapDifficulty(string difficulty) => difficulty switch
    {
        "easy" => "Easy",
        "medium" => "Medium",
        "hard" => "Hard",
        "expert" => "Expert",
        _ => difficulty
    };

    private string MapQuestionType(string type) => type switch
    {
        "single-select" => "Single choice",
        "multi-select" => "Multiple choice",
        "true-false" => "True/False",
        _ => type
    };

    private async Task LoadQuizzes()
    {
        try
        {
            var result = await _quizService.GetQuizzesByAuthorAsync(
                _authStateService.CurrentCredentials?.UserId ?? 
                throw new InvalidOperationException("User not authenticated"));

            if (result.Success)
            {
                var sortedQuizzes = result.Data
                    .OrderByDescending(q => q.CreatedAt)
                    .Select(q => new
                    {
                        Id = q.Id,
                        Name = q.Name,
                        Prompt = q.Prompt,
                        Difficulty = MapDifficulty(q.Difficulty),
                        NumQuestions = q.NumQuestions,
                        QuestionTypes = string.Join(", ", q.AllowedTypes.Select(MapQuestionType)),
                        CreatedAt = q.CreatedAt.ToString("g")
                    }).ToList();

                QuizList.ItemsSource = sortedQuizzes;
                
                // Show/hide empty state message based on quiz count
                EmptyStateMessage.Visibility = !sortedQuizzes.Any() 
                    ? Visibility.Visible 
                    : Visibility.Collapsed;
            }
            else
            {
                await ShowError("Failed to load quizzes", result.Message);
            }
        }
        catch (Exception ex)
        {
            await ShowError("Failed to load quizzes", ex.Message);
        }
    }

    private async void DeleteQuizButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is int quizId)
        {
            var dialog = new ContentDialog
            {
                Title = "Confirm Delete",
                Content = "Are you sure you want to delete this quiz?",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var deleteResult = await _quizService.DeleteQuizAsync(quizId);
                if (deleteResult.Success)
                {
                    await LoadQuizzes(); // Refresh the list
                }
                else
                {
                    await ShowError("Failed to delete quiz", deleteResult.Message);
                }
            }
        }
    }

    private async void StartQuizButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is int quizId)
        {
            try
            {
                var quizWindow = new QuizWindow(quizId);
                quizWindow.Closed += async (s, e) => 
                {
                    await LoadQuizzes(); // Refresh list to show updated scores
                };
                quizWindow.Activate();
            }
            catch (Exception ex)
            {
                await ShowError("Failed to start quiz", ex.Message);
            }
        }
    }

    private async void ExportQuizButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is int quizId)
        {
            // Create export options panel
            var optionsPanel = new StackPanel { Spacing = 16 };
            
            // Format selection
            var formatPanel = new RadioButtons();
            formatPanel.Header = "Export Format";
            formatPanel.Items.Add("PDF Document (*.pdf)");
            formatPanel.Items.Add("Text File (*.txt)");
            formatPanel.SelectedIndex = 0;
            optionsPanel.Children.Add(formatPanel);

            // Include answers checkbox
            var includeAnswersCheck = new CheckBox 
            { 
                Content = "Include correct answers",
                IsChecked = false
            };
            optionsPanel.Children.Add(includeAnswersCheck);

            var dialog = new ContentDialog
            {
                Title = "Export Quiz",
                Content = optionsPanel,
                PrimaryButtonText = "Export",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var isPdf = formatPanel.SelectedIndex == 0;
                var includeAnswers = includeAnswersCheck.IsChecked ?? false;

                // Get quiz details for filename
                var quiz = ((IEnumerable<dynamic>)QuizList.ItemsSource)
                    .First(q => q.Id == quizId);

                var defaultFileName = $"{quiz.Name}_{quiz.CreatedAt:yyyy-MM-dd}";
                defaultFileName += isPdf ? ".pdf" : ".txt";

                // Show file picker
                var savePicker = new FileSavePicker
                {
                    SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                    SuggestedFileName = defaultFileName
                };

                // Initialize file picker
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(MainWindow.Instance);
                WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

                if (isPdf)
                    savePicker.FileTypeChoices.Add("PDF Document", new[] { ".pdf" });
                else
                    savePicker.FileTypeChoices.Add("Text File", new[] { ".txt" });

                var file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    try
                    {
                        ServiceResult<byte[]> exportResult;
                        if (isPdf)
                            exportResult = await _quizExportService.ExportAsPdfAsync(quizId, includeAnswers);
                        else
                            exportResult = await _quizExportService.ExportAsTextAsync(quizId, includeAnswers);

                        if (exportResult.Success)
                        {
                            await FileIO.WriteBytesAsync(file, exportResult.Data);
                            MainWindow.Instance.ShowToast("Quiz exported successfully!");
                        }
                        else
                        {
                            await ShowError("Export Failed", exportResult.Message);
                        }
                    }
                    catch (Exception ex)
                    {
                        await ShowError("Export Failed", ex.Message);
                    }
                }
            }
        }
    }

    private async Task ShowError(string title, string message)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = this.XamlRoot
        };
        await dialog.ShowAsync();
    }
}