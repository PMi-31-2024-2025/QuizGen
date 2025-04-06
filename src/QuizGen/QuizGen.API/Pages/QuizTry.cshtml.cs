using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuizGen.BLL.Models.QuizTry;
using QuizGen.BLL.Services.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace QuizGen.API.Pages;

public class QuizTryModel : PageModel
{
    private readonly IQuizTryService _quizTryService;
    private readonly ILogger<QuizTryModel> _logger;

    public QuizTryDetailsDto? QuizTry { get; set; }
    public string? ErrorMessage { get; set; }

    [BindProperty(SupportsGet = true)]
    public int QuizId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? AttemptId { get; set; }

    [BindProperty]
    public int? SelectedAnswerId { get; set; }

    [BindProperty]
    public List<int> SelectedAnswerIds { get; set; } = new();

    public QuizTryModel(
        IQuizTryService quizTryService,
        ILogger<QuizTryModel> logger)
    {
        _quizTryService = quizTryService;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        // Check if user is authenticated
        var authToken = Request.Cookies["AuthToken"];
        var userId = Request.Cookies["UserId"];

        if (string.IsNullOrEmpty(authToken) || string.IsNullOrEmpty(userId))
        {
            return RedirectToPage("/Login");
        }

        if (QuizId <= 0)
        {
            ErrorMessage = "Invalid quiz ID";
            return Page();
        }

        try
        {
            if (AttemptId.HasValue)
            {
                // Continue existing attempt
                var result = await _quizTryService.GetQuizTryDetailsAsync(AttemptId.Value);
                if (!result.Success)
                {
                    ErrorMessage = result.Message;
                    return Page();
                }

                // Verify that the attempt belongs to the current user
                if (result.Data.UserId != int.Parse(userId))
                {
                    ErrorMessage = "You are not authorized to continue this quiz attempt";
                    return Page();
                }

                QuizTry = result.Data;
            }
            else
            {
                // Start new attempt
                var startResult = await _quizTryService.StartQuizTryAsync(QuizId, int.Parse(userId));
                if (!startResult.Success)
                {
                    ErrorMessage = startResult.Message;
                    return Page();
                }

                // Get the quiz details for the new attempt
                var detailsResult = await _quizTryService.GetQuizTryDetailsAsync(startResult.Data.Id);
                if (!detailsResult.Success)
                {
                    ErrorMessage = detailsResult.Message;
                    return Page();
                }

                QuizTry = detailsResult.Data;
                AttemptId = startResult.Data.Id;
            }

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading quiz for quiz {QuizId} and attempt {AttemptId}", QuizId, AttemptId);
            ErrorMessage = "An error occurred while loading the quiz";
            return Page();
        }
    }

    public async Task<IActionResult> OnPostSubmitAnswerAsync()
    {
        // Check if user is authenticated
        var authToken = Request.Cookies["AuthToken"];
        var userId = Request.Cookies["UserId"];

        if (string.IsNullOrEmpty(authToken) || string.IsNullOrEmpty(userId))
        {
            return RedirectToPage("/Login");
        }

        // Get current quiz details
        var currentDetails = await _quizTryService.GetQuizTryDetailsAsync(AttemptId.Value);
        if (!currentDetails.Success)
        {
            ErrorMessage = currentDetails.Message;
            return Page();
        }

        QuizTry = currentDetails.Data;
        var currentQuestion = QuizTry.Questions[QuizTry.CurrentQuestionIndex];

        try
        {
            List<int> selectedAnswers;
            // Validate answer selection
            if (currentQuestion.Type == "multi-select")
            {
                if (!SelectedAnswerIds.Any())
                {
                    ErrorMessage = "Please select at least one answer";
                    // Get fresh quiz details to show the current question again
                    var refreshResult = await _quizTryService.GetQuizTryDetailsAsync(AttemptId.Value);
                    if (refreshResult.Success)
                    {
                        QuizTry = refreshResult.Data;
                    }
                    return Page();
                }
                selectedAnswers = SelectedAnswerIds;
            }
            else
            {
                if (!SelectedAnswerId.HasValue)
                {
                    ErrorMessage = "Please select an answer";
                    // Get fresh quiz details to show the current question again
                    var refreshResult = await _quizTryService.GetQuizTryDetailsAsync(AttemptId.Value);
                    if (refreshResult.Success)
                    {
                        QuizTry = refreshResult.Data;
                    }
                    return Page();
                }
                selectedAnswers = new List<int> { SelectedAnswerId.Value };
            }

            // Save the answer
            var result = await _quizTryService.SaveAnswersAsync(
                QuizTry.Id,
                currentQuestion.Id,
                selectedAnswers);

            if (!result.Success)
            {
                ErrorMessage = result.Message;
                return Page();
            }

            // Check if this was the last question
            if (QuizTry.CurrentQuestionIndex >= QuizTry.TotalQuestions - 1)
            {
                // Finish the quiz
                var finishResult = await _quizTryService.FinishQuizTryAsync(QuizTry.Id);
                if (!finishResult.Success)
                {
                    ErrorMessage = finishResult.Message;
                    return Page();
                }

                return RedirectToPage("QuizResult", new { attemptId = QuizTry.Id });
            }

            // Clear the selected answers
            SelectedAnswerId = null;
            SelectedAnswerIds.Clear();

            // Get the next question
            var nextQuestionResult = await _quizTryService.GetQuizTryDetailsAsync(QuizTry.Id);
            if (!nextQuestionResult.Success)
            {
                ErrorMessage = nextQuestionResult.Message;
                return Page();
            }

            QuizTry = nextQuestionResult.Data;
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting answer for quiz try {QuizTryId}", QuizTry?.Id);
            ErrorMessage = "An error occurred while submitting your answer";
            // Get fresh quiz details to show the current question again
            var refreshResult = await _quizTryService.GetQuizTryDetailsAsync(AttemptId.Value);
            if (refreshResult.Success)
            {
                QuizTry = refreshResult.Data;
            }
            return Page();
        }
    }
} 