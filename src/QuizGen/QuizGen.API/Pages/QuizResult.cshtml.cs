using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuizGen.BLL.Models.QuizTry;
using QuizGen.BLL.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace QuizGen.API.Pages;

public class QuizResultModel : PageModel
{
    private readonly IQuizTryService _quizTryService;
    private readonly ILogger<QuizResultModel> _logger;

    public QuizTryResultDto? QuizResult { get; set; }
    public string? ErrorMessage { get; set; }

    [BindProperty(SupportsGet = true)]
    public int AttemptId { get; set; }

    public QuizResultModel(
        IQuizTryService quizTryService,
        ILogger<QuizResultModel> logger)
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

        if (AttemptId <= 0)
        {
            ErrorMessage = "Invalid attempt ID";
            return Page();
        }

        try
        {
            // Get the quiz details to verify ownership
            var detailsResult = await _quizTryService.GetQuizTryDetailsAsync(AttemptId);
            if (!detailsResult.Success)
            {
                ErrorMessage = detailsResult.Message;
                return Page();
            }

            // Verify that the attempt belongs to the current user
            if (detailsResult.Data.UserId != int.Parse(userId))
            {
                ErrorMessage = "You are not authorized to view this quiz result";
                return Page();
            }

            // Get the quiz result
            var result = await _quizTryService.CalculateAndSaveScoreAsync(AttemptId);
            if (!result.Success)
            {
                ErrorMessage = result.Message;
                return Page();
            }

            QuizResult = result.Data;
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading quiz result for attempt {AttemptId}", AttemptId);
            ErrorMessage = "An error occurred while loading the quiz result";
            return Page();
        }
    }
} 