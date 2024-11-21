using QuizGen.BLL.Models.Base;
using QuizGen.BLL.Models.QuizTry;
using QuizGen.BLL.Models.Answer;
using QuizGen.BLL.Models.QuizAnswer;
using QuizGen.BLL.Services.Interfaces;
using QuizGen.DAL.Interfaces;
using QuizGen.DAL.Models;

namespace QuizGen.BLL.Services;

public class QuizTryService : IQuizTryService
{
    private readonly IQuizTryRepository _quizTryRepository;
    private readonly IQuizRepository _quizRepository;
    private readonly IUserRepository _userRepository;
    private readonly IQuizAnswerRepository _quizAnswerRepository;

    public QuizTryService(
        IQuizTryRepository quizTryRepository,
        IQuizRepository quizRepository,
        IUserRepository userRepository,
        IQuizAnswerRepository quizAnswerRepository)
    {
        _quizTryRepository = quizTryRepository;
        _quizRepository = quizRepository;
        _userRepository = userRepository;
        _quizAnswerRepository = quizAnswerRepository;
    }

    public async Task<ServiceResult<QuizTryDto>> StartQuizTryAsync(int quizId, int userId)
    {
        try
        {
            var quiz = await _quizRepository.GetByIdAsync(quizId);
            if (quiz == null)
                return ServiceResult<QuizTryDto>.CreateError("Quiz not found");

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return ServiceResult<QuizTryDto>.CreateError("User not found");

            var quizTry = new QuizTry
            {
                QuizId = quizId,
                UserId = userId,
                StartedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdQuizTry = await _quizTryRepository.AddAsync(quizTry);
            return ServiceResult<QuizTryDto>.CreateSuccess(await MapToDto(createdQuizTry));
        }
        catch (Exception ex)
        {
            return ServiceResult<QuizTryDto>.CreateError($"Failed to start quiz try: {ex.Message}");
        }
    }

    public async Task<ServiceResult<QuizTryDto>> FinishQuizTryAsync(int quizTryId)
    {
        try
        {
            var quizTry = await _quizTryRepository.GetByIdAsync(quizTryId);
            if (quizTry == null)
                return ServiceResult<QuizTryDto>.CreateError("QuizTry not found");

            quizTry.FinishedAt = DateTime.UtcNow;
            quizTry.UpdatedAt = DateTime.UtcNow;

            await _quizTryRepository.UpdateAsync(quizTry);
            return ServiceResult<QuizTryDto>.CreateSuccess(await MapToDto(quizTry));
        }
        catch (Exception ex)
        {
            return ServiceResult<QuizTryDto>.CreateError($"Failed to finish quiz try: {ex.Message}");
        }
    }

    public async Task<ServiceResult<QuizTryDto>> GetQuizTryByIdAsync(int id)
    {
        try
        {
            var quizTry = await _quizTryRepository.GetByIdAsync(id);
            if (quizTry == null)
                return ServiceResult<QuizTryDto>.CreateError("QuizTry not found");

            return ServiceResult<QuizTryDto>.CreateSuccess(await MapToDto(quizTry));
        }
        catch (Exception ex)
        {
            return ServiceResult<QuizTryDto>.CreateError($"Failed to get quiz try: {ex.Message}");
        }
    }

    public async Task<ServiceResult<IEnumerable<QuizTryDto>>> GetQuizTriesByUserAsync(int userId)
    {
        try
        {
            var quizTries = await _quizTryRepository.GetByUserIdAsync(userId);
            var dtos = await Task.WhenAll(quizTries.Select(MapToDto));
            return ServiceResult<IEnumerable<QuizTryDto>>.CreateSuccess(dtos);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<QuizTryDto>>.CreateError($"Failed to get quiz tries: {ex.Message}");
        }
    }

    public async Task<ServiceResult<IEnumerable<QuizTryDto>>> GetQuizTriesByQuizAsync(int quizId)
    {
        try
        {
            var quizTries = await _quizTryRepository.GetByQuizIdAsync(quizId);
            var dtos = await Task.WhenAll(quizTries.Select(MapToDto));
            return ServiceResult<IEnumerable<QuizTryDto>>.CreateSuccess(dtos);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<QuizTryDto>>.CreateError($"Failed to get quiz tries: {ex.Message}");
        }
    }

    public async Task<ServiceResult<IEnumerable<QuizTryDto>>> GetCompletedQuizTriesByUserAsync(int userId)
    {
        try
        {
            var quizTries = await _quizTryRepository.GetCompletedByUserIdAsync(userId);
            var dtos = await Task.WhenAll(quizTries.Select(MapToDto));
            return ServiceResult<IEnumerable<QuizTryDto>>.CreateSuccess(dtos);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<QuizTryDto>>.CreateError($"Failed to get completed quiz tries: {ex.Message}");
        }
    }

    private async Task<QuizTryDto> MapToDto(QuizTry quizTry)
    {
        var quiz = await _quizRepository.GetByIdAsync(quizTry.QuizId);
        var user = await _userRepository.GetByIdAsync(quizTry.UserId);
        var answers = await _quizAnswerRepository.GetByQuizTryIdAsync(quizTry.Id);

        return new QuizTryDto
        {
            Id = quizTry.Id,
            QuizId = quizTry.QuizId,
            UserId = quizTry.UserId,
            QuizPrompt = quiz?.Prompt ?? "Unknown Quiz",
            UserName = user?.Name ?? "Unknown User",
            StartedAt = quizTry.StartedAt,
            FinishedAt = quizTry.FinishedAt,
            CreatedAt = quizTry.CreatedAt,
            Answers = answers.Select(qa => new QuizAnswerDto
            {
                Id = qa.Id,
                QuestionId = qa.QuestionId,
                AnswerId = qa.AnswerId,
                QuestionText = qa.Question?.Text ?? "Unknown Question",
                AnswerText = qa.Answer?.Text ?? "Unknown Answer",
                IsCorrect = qa.Answer?.IsCorrect ?? false,
                CreatedAt = qa.CreatedAt
            }).ToList()
        };
    }
} 