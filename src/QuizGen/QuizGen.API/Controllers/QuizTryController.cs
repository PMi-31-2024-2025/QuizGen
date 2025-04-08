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

    [HttpPost("{quizId}/start")]
    public async Task<IActionResult> StartQuizTry(int quizId)
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        var quizResult = await _quizService.GetQuizByIdAsync(quizId);
        if (!quizResult.Success)
        {
            return NotFound(quizResult.Message);
        }

        if (quizResult.Data.AuthorId == userId)
        {
            return Forbid();
        }

        try
        {
            var result = await _quizTryService.StartQuizTryAsync(quizId, userId);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return CreatedAtAction(
                nameof(GetQuizTryDetails),
                new { quizId = quizId, attemptId = result.Data.Id },
                result.Data);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyQuizTries()
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
        {
            return Unauthorized("User is not authenticated");
        }

        var result = await _quizTryService.GetQuizTriesByUserAsync(userId);
        if (!result.Success)
        {
            return BadRequest(result.Message);
        }

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
    public async Task<IActionResult> SubmitAnswer(int quizId, int attemptId, SubmitAnswerRequest request)
    {
        if (request == null || request.QuestionId <= 0 || request.AnswerId <= 0)
        {
            return BadRequest("Invalid question ID or answer ID");
        }

        var userId = GetCurrentUserId();
        if (userId == 0)
        {
            return Unauthorized("User is not authenticated");
        }

        var quizTryResult = await _quizTryService.GetQuizTryDetailsAsync(attemptId);
        if (!quizTryResult.Success)
        {
            return NotFound(quizTryResult.Message);
        }

        if (quizTryResult.Data.UserId != userId)
        {
            return Forbid();
        }

        try
        {
            var result = await _quizAnswerService.CreateQuizAnswerAsync(attemptId, request.QuestionId, request.AnswerId);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return CreatedAtAction(
                nameof(GetQuizTryAnswers),
                new { quizId = quizId, attemptId = attemptId },
                result.Data);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{quizId}/attempts/{attemptId}/answers")]
    public async Task<IActionResult> GetQuizTryAnswers(int quizId, int attemptId)
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
        {
            return Unauthorized("User is not authenticated");
        }

        var quizTryResult = await _quizTryService.GetQuizTryDetailsAsync(attemptId);
        if (!quizTryResult.Success)
        {
            return NotFound(quizTryResult.Message);
        }

        if (quizTryResult.Data.UserId != userId)
        {
            return Forbid();
        }

        try
        {
            var result = await _quizAnswerService.GetQuizAnswersByQuizTryAsync(attemptId);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
} 