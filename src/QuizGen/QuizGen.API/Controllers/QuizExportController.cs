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

    public QuizExportController(
        IQuizExportService quizExportService,
        IQuizTryService quizTryService)
    {
        _quizExportService = quizExportService;
        _quizTryService = quizTryService;
    }

    [HttpGet("{quizId}/attempts/{attemptId}/export/pdf")]
    public async Task<IActionResult> ExportQuizTryAsPdf(int quizId, int attemptId)
    {
        int currentUserId = GetCurrentUserId();
        if (currentUserId == 0)
            return Unauthorized("Invalid user credentials");

        // Verify the attempt exists and belongs to the current user
        var attemptResult = await _quizTryService.GetQuizTryDetailsAsync(attemptId);
        if (!attemptResult.Success)
            return NotFound("Quiz attempt not found");
            
        if (attemptResult.Data.UserId != currentUserId)
            return Forbid("You are not authorized to export this quiz attempt");
            
        // Verify the quiz ID matches the attempt's quiz ID
        if (attemptResult.Data.QuizId != quizId)
            return BadRequest("Quiz ID does not match the attempt's quiz ID");

        var result = await _quizExportService.ExportTryAsPdfAsync(attemptId);
        if (!result.Success)
            return BadRequest(result.Message);

        return File(result.Data, "application/pdf", $"quiz-{quizId}-attempt-{attemptId}.pdf");
    }

    [HttpGet("{quizId}/attempts/{attemptId}/export/text")]
    public async Task<IActionResult> ExportQuizTryAsText(int quizId, int attemptId)
    {
        int currentUserId = GetCurrentUserId();
        if (currentUserId == 0)
            return Unauthorized("Invalid user credentials");

        // Verify the attempt exists and belongs to the current user
        var attemptResult = await _quizTryService.GetQuizTryDetailsAsync(attemptId);
        if (!attemptResult.Success)
            return NotFound("Quiz attempt not found");
            
        if (attemptResult.Data.UserId != currentUserId)
            return Forbid("You are not authorized to export this quiz attempt");
            
        // Verify the quiz ID matches the attempt's quiz ID
        if (attemptResult.Data.QuizId != quizId)
            return BadRequest("Quiz ID does not match the attempt's quiz ID");

        var result = await _quizExportService.ExportTryAsTextAsync(attemptId);
        if (!result.Success)
            return BadRequest(result.Message);

        return File(result.Data, "text/plain", $"quiz-{quizId}-attempt-{attemptId}.txt");
    }
} 