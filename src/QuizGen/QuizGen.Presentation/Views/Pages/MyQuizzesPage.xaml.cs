using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using QuizGen.BLL.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using QuizGen.BLL.Models.Quiz;
using System.Threading.Tasks;
using System.Linq;

namespace QuizGen.Presentation.Views.Pages;

public sealed partial class MyQuizzesPage : Page
{
    private readonly IQuizService _quizService;
    private readonly IAuthStateService _authStateService;

    public MyQuizzesPage()
    {
        InitializeComponent();
        
        var serviceProvider = ((App)Application.Current).ServiceProvider;
        _quizService = serviceProvider.GetRequiredService<IQuizService>();
        _authStateService = serviceProvider.GetRequiredService<IAuthStateService>();

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
        var dialog = new ContentDialog
        {
            Title = "Coming Soon",
            Content = "Quiz taking functionality will be implemented in a future update.",
            CloseButtonText = "OK",
            XamlRoot = this.XamlRoot
        };
        await dialog.ShowAsync();
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