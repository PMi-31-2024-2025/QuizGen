using Microsoft.AspNetCore.Mvc;
using QuizGen.BLL.Models.QuizAnswer;
using QuizGen.BLL.Models.QuizTry;
using QuizGen.BLL.Services.Interfaces;

namespace QuizGen.API.Controllers;

[ApiController]
[Route("api/quiz-tries")]
public class QuizTryController : ControllerBase
{
    private readonly IQuizTryService _quizTryService;
    private readonly IQuizAnswerService _quizAnswerService;

    public QuizTryController(
        IQuizTryService quizTryService,
        IQuizAnswerService quizAnswerService)
    {
        _quizTryService = quizTryService;
        _quizAnswerService = quizAnswerService;
    }

    [HttpPost]
    public async Task<IActionResult> StartQuizTry([FromBody] StartQuizTryRequest request)
    {
        var result = await _quizTryService.StartQuizTryAsync(request.QuizId, request.UserId);
        if (!result.Success)
            return BadRequest(result.Message);

        return CreatedAtAction(nameof(GetQuizTryDetails), new { id = result.Data.Id }, result.Data);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetQuizTryDetails(int id)
    {
        var result = await _quizTryService.GetQuizTryDetailsAsync(id);
        if (!result.Success)
            return NotFound(result.Message);

        return Ok(result.Data);
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserQuizTries(int userId)
    {
        var result = await _quizTryService.GetQuizTriesByUserAsync(userId);
        if (!result.Success)
            return BadRequest(result.Message);

        return Ok(result.Data);
    }

    [HttpGet("quiz/{quizId}")]
    public async Task<IActionResult> GetQuizTries(int quizId)
    {
        var result = await _quizTryService.GetQuizTriesByQuizAsync(quizId);
        if (!result.Success)
            return BadRequest(result.Message);

        return Ok(result.Data);
    }

    [HttpPost("{quizTryId}/answers")]
    public async Task<IActionResult> SubmitAnswer(int quizTryId, [FromBody] SubmitAnswerRequest request)
    {
        var result = await _quizAnswerService.CreateQuizAnswerAsync(
            quizTryId,
            request.QuestionId,
            request.AnswerId);

        if (!result.Success)
            return BadRequest(result.Message);

        return CreatedAtAction(
            nameof(GetQuizTryAnswers),
            new { quizTryId = quizTryId },
            result.Data);
    }

    [HttpGet("{quizTryId}/answers")]
    public async Task<IActionResult> GetQuizTryAnswers(int quizTryId)
    {
        var result = await _quizAnswerService.GetQuizAnswersByQuizTryAsync(quizTryId);
        if (!result.Success)
            return BadRequest(result.Message);

        return Ok(result.Data);
    }
} 