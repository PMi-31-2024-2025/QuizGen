using Microsoft.AspNetCore.Mvc;
using QuizGen.BLL.Services.Interfaces;

namespace QuizGen.API.Controllers;

[ApiController]
[Route("api/quiz-exports")]
public class QuizExportController : ControllerBase
{
    private readonly IQuizExportService _quizExportService;

    public QuizExportController(IQuizExportService quizExportService)
    {
        _quizExportService = quizExportService;
    }

    [HttpGet("quiz-tries/{quizTryId}/pdf")]
    public async Task<IActionResult> ExportQuizTryAsPdf(int quizTryId)
    {
        var result = await _quizExportService.ExportTryAsPdfAsync(quizTryId);
        if (!result.Success)
            return BadRequest(result.Message);

        return File(result.Data, "application/pdf", $"quiz-try-{quizTryId}.pdf");
    }

    [HttpGet("quiz-tries/{quizTryId}/text")]
    public async Task<IActionResult> ExportQuizTryAsText(int quizTryId)
    {
        var result = await _quizExportService.ExportTryAsTextAsync(quizTryId);
        if (!result.Success)
            return BadRequest(result.Message);

        return File(result.Data, "text/plain", $"quiz-try-{quizTryId}.txt");
    }
} 