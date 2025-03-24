using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuizGen.BLL.Services.Interfaces;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;

namespace QuizGen.API.Pages;

public class CreateQuizModel : PageModel
{
    private readonly IQuizService _quizService;
    private readonly IAuthStateService _authStateService;
    private readonly ILogger<CreateQuizModel> _logger;

    [BindProperty]
    [Required(ErrorMessage = "Please enter a topic")]
    public string Topic { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Please select the number of questions")]
    public int QuestionCount { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Please select the difficulty level")]
    public string Difficulty { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Please select at least one question type")]
    public string[] QuestionTypes { get; set; }

    public string ErrorMessage { get; set; }

    public CreateQuizModel(
        IQuizService quizService, 
        IAuthStateService authStateService,
        ILogger<CreateQuizModel> logger)
    {
        _quizService = quizService;
        _authStateService = authStateService;
        _logger = logger;
    }

    public void OnGet()
    {
        _logger.LogInformation("CreateQuiz page loaded");
        // Set default values
        QuestionCount = 6;
        Difficulty = "medium";
        QuestionTypes = new[] { "single-select", "multi-select", "true-false" };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        _logger.LogInformation("Starting quiz creation process");
        
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state: {ValidationErrors}", 
                string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            ErrorMessage = "Please correct the errors below.";
            return Page();
        }

        if (QuestionTypes == null || !QuestionTypes.Any())
        {
            _logger.LogWarning("No question types selected");
            ErrorMessage = "Please select at least one question type.";
            return Page();
        }


            var authToken = Request.Cookies["AuthToken"];
            var userId = Request.Cookies["UserId"]; 
        try
        {
        


            _logger.LogInformation("Creating quiz for user {UserId} with topic: {Topic}, difficulty: {Difficulty}, question count: {QuestionCount}, types: {QuestionTypes}",
                userId, Topic, Difficulty, QuestionCount, string.Join(", ", QuestionTypes));

            var result = await _quizService.CreateQuizAsync(
               Int32.Parse(userId),
                Topic,
                Difficulty,
                QuestionCount,
                QuestionTypes
            );

            if (result.Success)
            {
                _logger.LogInformation("Quiz created successfully for user {UserId}", userId);
                TempData["SuccessMessage"] = "Quiz created successfully!";
                return RedirectToPage("/QuizList");
            }
            else
            {
                _logger.LogWarning("Failed to create quiz for user {UserId}: {ErrorMessage}", 
                    userId, result.Message ?? "Unknown error");
                ErrorMessage = result.Message ?? "Failed to create quiz. Please try again.";
                return Page();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating quiz for user {UserId}: {ErrorMessage}", 
                _authStateService.CurrentCredentials?.UserId, ex.Message);
            ErrorMessage = "An error occurred while creating the quiz. Please try again.";
            return Page();
        }
    }
} 