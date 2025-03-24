using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizGen.BLL.Services.Interfaces;

namespace QuizGen.API.Controllers;

[Authorize]
[Route("api/quizzes")]
public class QuizExportController : BaseController
{
    private readonly IQuizExportService _quizExportService;
    private readonly IQuizTryService _quizTryService;
    private readonly IQuizService _quizService;

    public QuizExportController(
        IQuizExportService quizExportService,
        IQuizTryService quizTryService,
        IQuizService quizService)
    {
        _quizExportService = quizExportService;
        _quizTryService = quizTryService;
        _quizService = quizService;
    }

    /// <summary>
    /// Exports a quiz to PDF format
    /// </summary>
    /// <param name="quizId">The ID of the quiz to export</param>
    /// <param name="includeAnswers">Whether to include correct answers (defaults to true)</param>
    /// <returns>PDF file of the quiz</returns>
    [HttpGet("{quizId}/export/pdf")]
    public async Task<IActionResult> ExportQuizAsPdf(int quizId, [FromQuery] bool includeAnswers = true)
    {
        int currentUserId = GetCurrentUserId();
        if (currentUserId == 0)
            return Unauthorized("Invalid user credentials");

        // Verify the quiz exists
        var quizResult = await _quizService.GetQuizByIdAsync(quizId);
        if (!quizResult.Success)
            return NotFound("Quiz not found");

        // Check if the user is the quiz author or has taken the quiz
        bool isAuthorized = quizResult.Data.AuthorId == currentUserId;
        
        if (!isAuthorized)
        {
            // Check if the user has any attempts for this quiz
            var userTriesResult = await _quizTryService.GetQuizTriesByQuizAsync(quizId);
            if (!userTriesResult.Success || !userTriesResult.Data.Any(t => t.UserId == currentUserId))
                return Forbid("You are not authorized to export this quiz");
        }

        var result = await _quizExportService.ExportAsPdfAsync(quizId, includeAnswers);
        if (!result.Success)
            return BadRequest(result.Message);

        return File(result.Data, "application/pdf", $"quiz-{quizId}.pdf");
    }

    /// <summary>
    /// Exports a quiz to text format
    /// </summary>
    /// <param name="quizId">The ID of the quiz to export</param>
    /// <param name="includeAnswers">Whether to include correct answers (defaults to true)</param>
    /// <returns>Text file of the quiz</returns>
    [HttpGet("{quizId}/export/text")]
    public async Task<IActionResult> ExportQuizAsText(int quizId, [FromQuery] bool includeAnswers = true)
    {
        int currentUserId = GetCurrentUserId();
        if (currentUserId == 0)
            return Unauthorized("Invalid user credentials");

        // Verify the quiz exists
        var quizResult = await _quizService.GetQuizByIdAsync(quizId);
        if (!quizResult.Success)
            return NotFound("Quiz not found");

        // Check if the user is the quiz author or has taken the quiz
        bool isAuthorized = quizResult.Data.AuthorId == currentUserId;
        
        if (!isAuthorized)
        {
            // Check if the user has any attempts for this quiz
            var userTriesResult = await _quizTryService.GetQuizTriesByQuizAsync(quizId);
            if (!userTriesResult.Success || !userTriesResult.Data.Any(t => t.UserId == currentUserId))
                return Forbid("You are not authorized to export this quiz");
        }

        var result = await _quizExportService.ExportAsTextAsync(quizId, includeAnswers);
        if (!result.Success)
            return BadRequest(result.Message);

        return File(result.Data, "text/plain", $"quiz-{quizId}.txt");
    }

    /// <summary>
    /// Exports a quiz attempt to PDF format
    /// </summary>
    /// <param name="quizId">The ID of the quiz</param>
    /// <param name="attemptId">The ID of the quiz attempt to export</param>
    /// <returns>PDF file of the quiz attempt</returns>
    [HttpGet("{quizId}/attempts/{attemptId}/export/pdf")]
    public async Task<IActionResult> ExportQuizTryAsPdf(int quizId, int attemptId)
    {
        int currentUserId = GetCurrentUserId();
        if (currentUserId == 0)
            return Unauthorized("Invalid user credentials");

        // Verify the quiz exists
        var quizResult = await _quizService.GetQuizByIdAsync(quizId);
        if (!quizResult.Success)
            return NotFound("Quiz not found");

        // Verify the attempt exists
        var attemptResult = await _quizTryService.GetQuizTryDetailsAsync(attemptId);
        if (!attemptResult.Success)
            return NotFound("Quiz attempt not found");
            
        // Verify the quiz ID matches the attempt's quiz ID
        if (attemptResult.Data.QuizId != quizId)
            return BadRequest("Quiz ID does not match the attempt's quiz ID");

        // Check if the user is the quiz author or the one who took the quiz
        bool isAuthorized = quizResult.Data.AuthorId == currentUserId || 
                           attemptResult.Data.UserId == currentUserId;
                           
        if (!isAuthorized)
            return Forbid("You are not authorized to export this quiz attempt");

        var result = await _quizExportService.ExportTryAsPdfAsync(attemptId);
        if (!result.Success)
            return BadRequest(result.Message);

        return File(result.Data, "application/pdf", $"quiz-{quizId}-attempt-{attemptId}.pdf");
    }

    /// <summary>
    /// Exports a quiz attempt to text format
    /// </summary>
    /// <param name="quizId">The ID of the quiz</param>
    /// <param name="attemptId">The ID of the quiz attempt to export</param>
    /// <returns>Text file of the quiz attempt</returns>
    [HttpGet("{quizId}/attempts/{attemptId}/export/text")]
    public async Task<IActionResult> ExportQuizTryAsText(int quizId, int attemptId)
    {
        int currentUserId = GetCurrentUserId();
        if (currentUserId == 0)
            return Unauthorized("Invalid user credentials");

        // Verify the quiz exists
        var quizResult = await _quizService.GetQuizByIdAsync(quizId);
        if (!quizResult.Success)
            return NotFound("Quiz not found");

        // Verify the attempt exists
        var attemptResult = await _quizTryService.GetQuizTryDetailsAsync(attemptId);
        if (!attemptResult.Success)
            return NotFound("Quiz attempt not found");
            
        // Verify the quiz ID matches the attempt's quiz ID
        if (attemptResult.Data.QuizId != quizId)
            return BadRequest("Quiz ID does not match the attempt's quiz ID");

        // Check if the user is the quiz author or the one who took the quiz
        bool isAuthorized = quizResult.Data.AuthorId == currentUserId || 
                           attemptResult.Data.UserId == currentUserId;
                           
        if (!isAuthorized)
            return Forbid("You are not authorized to export this quiz attempt");

        var result = await _quizExportService.ExportTryAsTextAsync(attemptId);
        if (!result.Success)
            return BadRequest(result.Message);

        return File(result.Data, "text/plain", $"quiz-{quizId}-attempt-{attemptId}.txt");
    }
} 