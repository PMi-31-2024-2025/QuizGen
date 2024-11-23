using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using System.Text.Json;
using QuizGen.BLL.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace QuizGen.Presentation.Views.Pages;

public sealed partial class CreateQuizPage : Page
{
    private readonly IQuizService _quizService;
    private readonly IAuthStateService _authStateService;
    private readonly IAuthService _authService;

    public CreateQuizPage()
    {
        InitializeComponent();
        
        var serviceProvider = ((App)Application.Current).ServiceProvider;
        _quizService = serviceProvider.GetRequiredService<IQuizService>();
        _authStateService = serviceProvider.GetRequiredService<IAuthStateService>();
        _authService = serviceProvider.GetRequiredService<IAuthService>();

        InitializeDefaultValues();
        UpdateGreeting();
        
        // Add size changed handler
        SizeChanged += CreateQuizPage_SizeChanged;
    }

    private void InitializeDefaultValues()
    {
        QuestionCountComboBox.SelectedIndex = 1;  // 6 questions
        DifficultyComboBox.SelectedIndex = 1;     // Medium
        QuestionTypeComboBox.SelectedIndex = 0;   // Single Selection
    }

    private async void UpdateGreeting()
    {
        if (_authStateService.CurrentCredentials != null)
        {
            var userResult = await _authService.AutoLoginAsync(_authStateService.CurrentCredentials);
            if (userResult.Success)
            {
                var timeOfDay = DateTime.Now.Hour switch
                {
                    >= 5 and < 12 => "Good morning",
                    >= 12 and < 18 => "Good afternoon",
                    >= 18 and < 22 => "Good evening",
                    _ => "Good night"
                };
                
                GreetingText.Text = $"{timeOfDay}, {userResult.Data.Name}";
            }
        }
    }

    private void TopicBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        GenerateButton.IsEnabled = !string.IsNullOrWhiteSpace(TopicBox.Text);
    }

    private async void GenerateButton_Click(object sender, RoutedEventArgs e)
    {
        LoadingOverlay.Visibility = Visibility.Visible;
        GenerateButton.IsEnabled = false;

        try
        {
            var questionCount = int.Parse(((ComboBoxItem)QuestionCountComboBox.SelectedItem).Content.ToString().Split(' ')[0]);
            var difficulty = ((ComboBoxItem)DifficultyComboBox.SelectedItem).Content.ToString().Split(' ')[0].ToLower();
            var questionType = ((ComboBoxItem)QuestionTypeComboBox.SelectedItem).Content.ToString() switch
            {
                "Single choice questions" => "single-select",
                "Multiple choice questions" => "multi-select",
                "True/False questions" => "true-false",
                _ => throw new ArgumentException("Invalid question type")
            };

            var result = await _quizService.CreateQuizAsync(
                _authStateService.CurrentCredentials?.UserId ?? throw new InvalidOperationException("User not authenticated"),
                TopicBox.Text,
                difficulty,
                questionCount,
                new[] { questionType }
            );

            if (result.Success)
            {
                // Reset inputs to default state
                TopicBox.Text = string.Empty;
                InitializeDefaultValues();
                // TODO: Navigate to My Quizzes page when ready
            }
            else
            {
                var dialog = new ContentDialog
                {
                    Title = "Something went wrong!",
                    Content = "Quiz generation failed. Check your prompt and try again.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                LoadingOverlay.Visibility = Visibility.Collapsed;
                await dialog.ShowAsync();
            }
        }
        catch (Exception ex)
        {
            var dialog = new ContentDialog
            {
                Title = "Something went wrong!",
                Content = "Quiz generation failed. Check your prompt and try again.",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            LoadingOverlay.Visibility = Visibility.Collapsed;
            await dialog.ShowAsync();
        }
        finally
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;
            GenerateButton.IsEnabled = !string.IsNullOrWhiteSpace(TopicBox.Text);
        }
    }

    private void CreateQuizPage_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (ControlsGrid == null) return;

        // If width is less than 700px, switch to vertical layout
        if (ActualWidth < 700)
        {
            Grid.SetRow(QuestionCountComboBox, 0);
            Grid.SetColumn(QuestionCountComboBox, 0);
            Grid.SetRow(DifficultyComboBox, 1);
            Grid.SetColumn(DifficultyComboBox, 0);
            Grid.SetRow(QuestionTypeComboBox, 2);
            Grid.SetColumn(QuestionTypeComboBox, 0);

            ControlsGrid.RowDefinitions.Clear();
            ControlsGrid.ColumnDefinitions.Clear();
            ControlsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            ControlsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            ControlsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            ControlsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            QuestionCountComboBox.Margin = new Thickness(0, 0, 0, 8);
            DifficultyComboBox.Margin = new Thickness(0, 0, 0, 8);
            QuestionTypeComboBox.Margin = new Thickness(0);
        }
        else
        {
            Grid.SetRow(QuestionCountComboBox, 0);
            Grid.SetColumn(QuestionCountComboBox, 0);
            Grid.SetRow(DifficultyComboBox, 0);
            Grid.SetColumn(DifficultyComboBox, 1);
            Grid.SetRow(QuestionTypeComboBox, 0);
            Grid.SetColumn(QuestionTypeComboBox, 2);

            ControlsGrid.RowDefinitions.Clear();
            ControlsGrid.ColumnDefinitions.Clear();
            ControlsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            ControlsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            ControlsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            QuestionCountComboBox.Margin = new Thickness(0, 0, 8, 0);
            DifficultyComboBox.Margin = new Thickness(8, 0, 8, 0);
            QuestionTypeComboBox.Margin = new Thickness(8, 0, 0, 0);
        }
    }
} 