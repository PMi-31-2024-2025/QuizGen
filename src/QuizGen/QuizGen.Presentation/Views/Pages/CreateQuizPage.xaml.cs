using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using System.Text.Json;
using QuizGen.BLL.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Collections.Generic;

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
        
        // Initialize checkboxes
        SingleChoiceCheckBox.IsChecked = true;
        MultiChoiceCheckBox.IsChecked = true;
        TrueFalseCheckBox.IsChecked = true;
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
        if (GenerateButton == null) return;
        
        var anyTypeSelected = SingleChoiceCheckBox.IsChecked == true ||
                             MultiChoiceCheckBox.IsChecked == true ||
                             TrueFalseCheckBox.IsChecked == true;

        GenerateButton.IsEnabled = !string.IsNullOrWhiteSpace(TopicBox.Text) && anyTypeSelected;
    }

    private async void GenerateButton_Click(object sender, RoutedEventArgs e)
    {
        LoadingOverlay.Visibility = Visibility.Visible;
        GenerateButton.IsEnabled = false;

        try
        {
            var questionCount = int.Parse(((ComboBoxItem)QuestionCountComboBox.SelectedItem).Content.ToString().Split(' ')[0]);
            var difficulty = ((ComboBoxItem)DifficultyComboBox.SelectedItem).Content.ToString().Split(' ')[0].ToLower();
            var questionTypes = new List<string>();
            if (SingleChoiceCheckBox.IsChecked == true) questionTypes.Add("single-select");
            if (MultiChoiceCheckBox.IsChecked == true) questionTypes.Add("multi-select");
            if (TrueFalseCheckBox.IsChecked == true) questionTypes.Add("true-false");

            if (!questionTypes.Any())
            {
                await ShowError("Invalid Selection", "Please select at least one question type");
                return;
            }

            var result = await _quizService.CreateQuizAsync(
                _authStateService.CurrentCredentials?.UserId ?? throw new InvalidOperationException("User not authenticated"),
                TopicBox.Text,
                difficulty,
                questionCount,
                questionTypes.ToArray()
            );

            if (result.Success)
            {
                // Reset inputs to default state
                TopicBox.Text = string.Empty;
                InitializeDefaultValues();
                
                // Show success toast and navigate
                MainWindow.Instance.ShowToast("Quiz generated successfully!");
                MainWindow.Instance.NavigateTo("MyQuizzesPage");
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
                Content = $"Quiz generation failed. Check your prompt and try again. {ex.Message} {ex.InnerException?.Message}",
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

        // If width is less than 700px, adjust margins and spacing
        if (ActualWidth < 700)
        {
            ControlsGrid.RowSpacing = 8;
            QuestionCountComboBox.Margin = new Thickness(0, 0, 0, 8);
            DifficultyComboBox.Margin = new Thickness(0);
        }
        else
        {
            ControlsGrid.RowSpacing = 16;
            QuestionCountComboBox.Margin = new Thickness(0, 0, 8, 0);
            DifficultyComboBox.Margin = new Thickness(8, 0, 0, 0);
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

    private void QuestionType_CheckedChanged(object sender, RoutedEventArgs e)
    {
        if (GenerateButton == null) return;
        
        var anyTypeSelected = SingleChoiceCheckBox.IsChecked == true ||
                             MultiChoiceCheckBox.IsChecked == true ||
                             TrueFalseCheckBox.IsChecked == true;

        GenerateButton.IsEnabled = !string.IsNullOrWhiteSpace(TopicBox.Text) && anyTypeSelected;
    }
} 