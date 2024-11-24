using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;
using QuizGen.BLL.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using QuizGen.BLL.Models.QuizTry;
using QuizGen.Presentation.Views.Windows;
using Windows.Storage.Pickers;
using Windows.Storage;
using QuizGen.BLL.Models.Base;
using System.Collections.Generic;

namespace QuizGen.Presentation.Views.Pages;

public sealed partial class QuizHistoryPage : Page
{
    private readonly IQuizTryService _quizTryService;
    private readonly IAuthStateService _authStateService;
    private readonly IQuizExportService _quizExportService;

    public QuizHistoryPage()
    {
        InitializeComponent();
        
        var serviceProvider = ((App)Application.Current).ServiceProvider;
        _quizTryService = serviceProvider.GetRequiredService<IQuizTryService>();
        _authStateService = serviceProvider.GetRequiredService<IAuthStateService>();
        _quizExportService = serviceProvider.GetRequiredService<IQuizExportService>();

        Loaded += QuizHistoryPage_Loaded;
    }

    private async void QuizHistoryPage_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadQuizTries();
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

    private string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalHours >= 1)
        {
            int hours = (int)duration.TotalHours;
            int minutes = duration.Minutes;
            return $"{hours}h {minutes}m";
        }
        else
        {
            return $"{duration.Minutes}m";
        }
    }

    private async Task LoadQuizTries()
    {
        try
        {
            var userId = _authStateService.CurrentCredentials?.UserId ?? 
                throw new InvalidOperationException("User not authenticated");

            // Load statistics
            var statsResult = await _quizTryService.GetUserStatisticsAsync(userId);
            if (statsResult.Success)
            {
                TotalTriesText.Text = statsResult.Data.TotalTries.ToString();
                AverageScoreText.Text = $"{statsResult.Data.AverageScore:F1}%";
                AverageTimeText.Text = FormatDuration(statsResult.Data.AverageTime);
                PassRateText.Text = $"{statsResult.Data.PassRate:F1}%";
            }

            // Load quiz tries
            var triesResult = await _quizTryService.GetUserQuizTriesAsync(userId);
            if (triesResult.Success)
            {
                var sortedTries = triesResult.Data
                    .OrderByDescending(t => t.StartedAt)
                    .Select(t => new
                    {
                        Id = t.Id,
                        QuizName = t.QuizName,
                        Score = $"{t.Score:F0}",
                        TotalQuestions = t.TotalQuestions,
                        Difficulty = MapDifficulty(t.Difficulty),
                        QuestionTypes = string.Join(", ", t.QuestionTypes.Select(MapQuestionType)),
                        Duration = FormatDuration(t.FinishedAt - t.StartedAt),
                        StartedAt = t.StartedAt.ToString("g")
                    }).ToList();

                QuizTryList.ItemsSource = sortedTries;
                EmptyStateMessage.Visibility = !sortedTries.Any() 
                    ? Visibility.Visible 
                    : Visibility.Collapsed;
            }
            else
            {
                await ShowError("Failed to load quiz tries", triesResult.Message);
            }
        }
        catch (Exception ex)
        {
            await ShowError("Failed to load quiz tries", ex.Message);
        }
    }

    private async void ViewResultsButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is int quizTryId)
        {
            try
            {
                var result = await _quizTryService.GetQuizTryByIdAsync(quizTryId);
                if (result.Success)
                {
                    var resultWindow = new QuizResultWindow(result.Data);
                    resultWindow.Activate();
                }
                else
                {
                    await ShowError("Failed to load quiz results", result.Message);
                }
            }
            catch (Exception ex)
            {
                await ShowError("Failed to load quiz results", ex.Message);
            }
        }
    }

    private async void DeleteTryButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is int quizTryId)
        {
            var dialog = new ContentDialog
            {
                Title = "Confirm Delete",
                Content = "Are you sure you want to delete this quiz try?",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    var deleteResult = await _quizTryService.DeleteQuizTryAsync(quizTryId);
                    if (deleteResult.Success)
                    {
                        await LoadQuizTries(); // Refresh the list
                    }
                    else
                    {
                        await ShowError("Failed to delete quiz try", deleteResult.Message);
                    }
                }
                catch (Exception ex)
                {
                    await ShowError("Failed to delete quiz try", ex.Message);
                }
            }
        }
    }

    private async void ExportTryButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is int tryId)
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

            var dialog = new ContentDialog
            {
                Title = "Export Results",
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

                // Get quiz try details for filename
                var quizTry = ((IEnumerable<dynamic>)QuizTryList.ItemsSource)
                    .First(t => t.Id == tryId);

                var defaultFileName = $"{quizTry.QuizName}_Results_{quizTry.StartedAt:yyyy-MM-dd}";
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
                            exportResult = await _quizExportService.ExportTryAsPdfAsync(tryId);
                        else
                            exportResult = await _quizExportService.ExportTryAsTextAsync(tryId);

                        if (exportResult.Success)
                        {
                            await FileIO.WriteBytesAsync(file, exportResult.Data);
                            MainWindow.Instance.ShowToast("Results exported successfully!");
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