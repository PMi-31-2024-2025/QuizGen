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
    private readonly IServiceProvider _serviceProvider;

    public CreateQuizPage()
    {
        InitializeComponent();
        InitializeDefaultValues();
        
        _serviceProvider = ((App)Application.Current).ServiceProvider;
        _quizService = _serviceProvider.GetRequiredService<IQuizService>();
    }

    private void InitializeDefaultValues()
    {
        QuestionCountComboBox.SelectedIndex = 1;  // 6 questions
        DifficultyComboBox.SelectedIndex = 1;     // Medium
        QuestionTypeComboBox.SelectedIndex = 0;   // Single Selection
    }

    private async void GenerateButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TopicBox.Text))
        {
            var dialog = new ContentDialog
            {
                Title = "Validation Error",
                Content = "Please enter a topic for the quiz",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
            return;
        }

        LoadingOverlay.Visibility = Visibility.Visible;
        GenerateButton.IsEnabled = false;

        try
        {
            var questionCount = int.Parse(((ComboBoxItem)QuestionCountComboBox.SelectedItem).Content.ToString().Split(' ')[0]);
            var difficulty = ((ComboBoxItem)DifficultyComboBox.SelectedItem).Content.ToString().ToLower();
            var questionType = ((ComboBoxItem)QuestionTypeComboBox.SelectedItem).Content.ToString() switch
            {
                "Single Selection" => "single-select",
                "Multiple Selection" => "multi-select",
                "True/False" => "true-false",
                _ => throw new ArgumentException("Invalid question type")
            };

            var result = await _quizService.CreateQuizAsync(
                1, // TODO: Get actual user ID
                TopicBox.Text,
                difficulty,
                questionCount,
                new[] { questionType }
            );

            var jsonString = JsonSerializer.Serialize(result, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });

            var dialog = new ContentDialog
            {
                Title = result.Success ? "Quiz Generated Successfully" : "Generation Failed",
                Content = new ScrollViewer
                {
                    Content = new TextBlock
                    {
                        Text = jsonString,
                        TextWrapping = TextWrapping.Wrap,
                        FontFamily = new("Cascadia Code")
                    },
                    MaxHeight = 400
                },
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }
        catch (Exception ex)
        {
            var dialog = new ContentDialog
            {
                Title = "Error",
                Content = $"An error occurred: {ex.Message}",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }
        finally
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;
            GenerateButton.IsEnabled = true;
        }
    }
} 