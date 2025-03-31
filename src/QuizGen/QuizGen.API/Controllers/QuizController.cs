using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizGen.BLL.Models.Quiz;
using QuizGen.BLL.Services.Interfaces;

namespace QuizGen.API.Controllers;

[Authorize]
public class QuizController : BaseController
{
    private readonly IQuizService _quizService;

    public QuizController(IQuizService quizService)
    {
        _quizService = quizService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateQuiz([FromBody] CreateQuizRequest request)
    {
        
        int currentUserId = GetCurrentUserId();
        if (currentUserId == 0)
            return Unauthorized("Invalid user credentials");

        
        var result = await _quizService.CreateQuizAsync(
            currentUserId,  
            request.Topic,
            request.Difficulty,
            request.NumQuestions,
            request.AllowedTypes
        );

        if (!result.Success)
            return BadRequest(result.Message);

        return CreatedAtAction(nameof(GetQuiz), new { id = result.Data.Id }, result.Data);
    }

    [HttpGet]
    public async Task<IActionResult> GetMyQuizzes()
    {
        int currentUserId = GetCurrentUserId();
        if (currentUserId == 0)
            return Unauthorized("Invalid user credentials");

        var result = await _quizService.GetQuizzesByAuthorAsync(currentUserId);
        if (!result.Success)
            return BadRequest(result.Message);

        return Ok(result.Data);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetQuiz(int id)
    {
        var result = await _quizService.GetQuizByIdAsync(id);
        if (!result.Success)
            return NotFound(result.Message);

        // Check if the user owns this quiz
        if (!IsResourceOwner(result.Data.AuthorId))
            return Forbid("You are not authorized to access this quiz");

        return Ok(result.Data);
    }

    [HttpGet("difficulty/{difficulty}")]
    public async Task<IActionResult> GetQuizzesByDifficulty(string difficulty)
    {
        int currentUserId = GetCurrentUserId();
        if (currentUserId == 0)
            return Unauthorized("Invalid user credentials");

        // Get all quizzes by difficulty
        var result = await _quizService.GetQuizzesByDifficultyAsync(difficulty);
        if (!result.Success)
            return BadRequest(result.Message);

        // Filter to only return the current user's quizzes
        var userQuizzes = result.Data.Where(q => q.AuthorId == currentUserId).ToList();
        
        return Ok(userQuizzes);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteQuiz(int id)
    {
        // First get the quiz to verify ownership
        var getResult = await _quizService.GetQuizByIdAsync(id);
        if (!getResult.Success)
            return NotFound(getResult.Message);

        // Check if the user owns this quiz
        if (!IsResourceOwner(getResult.Data.AuthorId))
            return Forbid("You are not authorized to delete this quiz");

        // Delete the quiz
        var result = await _quizService.DeleteQuizAsync(id);
        if (!result.Success)
            return NotFound(result.Message);

        return NoContent();
    }
} 