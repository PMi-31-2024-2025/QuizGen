using Microsoft.AspNetCore.Mvc;
using QuizGen.BLL.Models.Quiz;
using QuizGen.BLL.Services.Interfaces;

namespace QuizGen.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuizController : ControllerBase
{
    private readonly IQuizService _quizService;

    public QuizController(IQuizService quizService)
    {
        _quizService = quizService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateQuiz([FromBody] CreateQuizRequest request)
    {
        var result = await _quizService.CreateQuizAsync(
            request.AuthorId,
            request.Topic,
            request.Difficulty,
            request.NumQuestions,
            request.AllowedTypes
        );

        if (!result.Success)
            return BadRequest(result.Message);

        return CreatedAtAction(nameof(GetQuiz), new { id = result.Data.Id }, result.Data);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetQuiz(int id)
    {
        var result = await _quizService.GetQuizByIdAsync(id);
        if (!result.Success)
            return NotFound(result.Message);

        return Ok(result.Data);
    }

    [HttpGet("author/{authorId}")]
    public async Task<IActionResult> GetQuizzesByAuthor(int authorId)
    {
        var result = await _quizService.GetQuizzesByAuthorAsync(authorId);
        if (!result.Success)
            return BadRequest(result.Message);

        return Ok(result.Data);
    }

    [HttpGet("difficulty/{difficulty}")]
    public async Task<IActionResult> GetQuizzesByDifficulty(string difficulty)
    {
        var result = await _quizService.GetQuizzesByDifficultyAsync(difficulty);
        if (!result.Success)
            return BadRequest(result.Message);

        return Ok(result.Data);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteQuiz(int id)
    {
        var result = await _quizService.DeleteQuizAsync(id);
        if (!result.Success)
            return NotFound(result.Message);

        return NoContent();
    }
} 