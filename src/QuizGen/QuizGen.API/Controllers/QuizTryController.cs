using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizGen.BLL.Models.QuizAnswer;
using QuizGen.BLL.Models.QuizTry;
using QuizGen.BLL.Services.Interfaces;

namespace QuizGen.API.Controllers;

[Authorize]
[Route("api/quizzes")]
public class QuizTryController : BaseController
{
    private readonly IQuizTryService _quizTryService;
    private readonly IQuizAnswerService _quizAnswerService;
    private readonly IQuizService _quizService;

    public QuizTryController(
        IQuizTryService quizTryService,
        IQuizAnswerService quizAnswerService,
        IQuizService quizService)
    {
        _quizTryService = quizTryService;
        _quizAnswerService = quizAnswerService;
        _quizService = quizService;
    }

    [HttpPost("{quizId}/attempts")]
    public async Task<IActionResult> StartQuizTry(int quizId)
    {
        int currentUserId = GetCurrentUserId();
        if (currentUserId == 0)
            return Unauthorized("Invalid user credentials");

        // Verify quiz exists and user has access to it
        var quizResult = await _quizService.GetQuizByIdAsync(quizId);
        if (!quizResult.Success)
            return NotFound("Quiz not found");

        // Verify the user owns the quiz or has permission to take it
        if (!IsResourceOwner(quizResult.Data.AuthorId))
            return Forbid("You are not authorized to take this quiz");

        var result = await _quizTryService.StartQuizTryAsync(quizId, currentUserId);
        if (!result.Success)
            return BadRequest(result.Message);

        return CreatedAtAction(nameof(GetQuizTryDetails), new { quizId = quizId, attemptId = result.Data.Id }, result.Data);
    }

    [HttpGet("attempts")]
    public async Task<IActionResult> GetMyQuizTries()
    {
        int currentUserId = GetCurrentUserId();
        if (currentUserId == 0)
            return Unauthorized("Invalid user credentials");

        var result = await _quizTryService.GetQuizTriesByUserAsync(currentUserId);
        if (!result.Success)
            return BadRequest(result.Message);

        return Ok(result.Data);
    }

    [HttpGet("{quizId}/attempts")]
    public async Task<IActionResult> GetQuizAttempts(int quizId)
    {
        int currentUserId = GetCurrentUserId();
        if (currentUserId == 0)
            return Unauthorized("Invalid user credentials");
            
        // Verify the user owns the quiz
        var quizResult = await _quizService.GetQuizByIdAsync(quizId);
        if (!quizResult.Success)
            return NotFound("Quiz not found");
            
        if (!IsResourceOwner(quizResult.Data.AuthorId))
            return Forbid("You are not authorized to view attempts for this quiz");

        var result = await _quizTryService.GetQuizTriesByQuizAsync(quizId);
        if (!result.Success)
            return BadRequest(result.Message);

        // Filter attempts to only those from the current user
        var userAttempts = result.Data.Where(a => a.UserId == currentUserId).ToList();
        
        return Ok(userAttempts);
    }

    [HttpGet("{quizId}/attempts/{attemptId}")]
    public async Task<IActionResult> GetQuizTryDetails(int quizId, int attemptId)
    {
        int currentUserId = GetCurrentUserId();
        if (currentUserId == 0)
            return Unauthorized("Invalid user credentials");

        var result = await _quizTryService.GetQuizTryDetailsAsync(attemptId);
        if (!result.Success)
            return NotFound(result.Message);

        // Verify that the attempt belongs to the current user
        if (result.Data.UserId != currentUserId)
            return Forbid("You are not authorized to view this quiz attempt");
            
        // Verify that the quiz ID matches the attempt's quiz ID
        if (result.Data.QuizId != quizId)
            return BadRequest("Quiz ID does not match the attempt's quiz ID");

        return Ok(result.Data);
    }

    [HttpPost("{quizId}/attempts/{attemptId}/answers")]
    public async Task<IActionResult> SubmitAnswer(int quizId, int attemptId, [FromBody] SubmitAnswerRequest request)
    {
        int currentUserId = GetCurrentUserId();
        if (currentUserId == 0)
            return Unauthorized("Invalid user credentials");

        // Verify that the attempt exists and belongs to the current user
        var attemptResult = await _quizTryService.GetQuizTryDetailsAsync(attemptId);
        if (!attemptResult.Success)
            return NotFound("Quiz attempt not found");
            
        if (attemptResult.Data.UserId != currentUserId)
            return Forbid("You are not authorized to submit answers for this quiz attempt");
            
        // Verify that the quiz ID matches the attempt's quiz ID
        if (attemptResult.Data.QuizId != quizId)
            return BadRequest("Quiz ID does not match the attempt's quiz ID");

        var result = await _quizAnswerService.CreateQuizAnswerAsync(
            attemptId,
            request.QuestionId,
            request.AnswerId);

        if (!result.Success)
            return BadRequest(result.Message);

        return CreatedAtAction(
            nameof(GetQuizTryAnswers),
            new { quizId = quizId, attemptId = attemptId },
            result.Data);
    }

    [HttpGet("{quizId}/attempts/{attemptId}/answers")]
    public async Task<IActionResult> GetQuizTryAnswers(int quizId, int attemptId)
    {
        int currentUserId = GetCurrentUserId();
        if (currentUserId == 0)
            return Unauthorized("Invalid user credentials");

        // Verify that the attempt exists and belongs to the current user
        var attemptResult = await _quizTryService.GetQuizTryDetailsAsync(attemptId);
        if (!attemptResult.Success)
            return NotFound("Quiz attempt not found");
            
        if (attemptResult.Data.UserId != currentUserId)
            return Forbid("You are not authorized to view answers for this quiz attempt");
            
        // Verify that the quiz ID matches the attempt's quiz ID
        if (attemptResult.Data.QuizId != quizId)
            return BadRequest("Quiz ID does not match the attempt's quiz ID");

        var result = await _quizAnswerService.GetQuizAnswersByQuizTryAsync(attemptId);
        if (!result.Success)
            return BadRequest(result.Message);

        return Ok(result.Data);
    }
} 