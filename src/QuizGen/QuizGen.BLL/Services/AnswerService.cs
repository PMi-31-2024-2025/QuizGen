using QuizGen.BLL.Models.Answer;
using QuizGen.BLL.Models.Base;
using QuizGen.BLL.Services.Interfaces;
using QuizGen.DAL.Interfaces;
using QuizGen.DAL.Models;

namespace QuizGen.BLL.Services;

public class AnswerService : IAnswerService
{
    private readonly IAnswerRepository _answerRepository;
    private readonly IQuestionRepository _questionRepository;

    public AnswerService(IAnswerRepository answerRepository, IQuestionRepository questionRepository)
    {
        _answerRepository = answerRepository;
        _questionRepository = questionRepository;
    }

    public async Task<ServiceResult<AnswerDto>> CreateAnswerAsync(int questionId, string text, bool isCorrect)
    {
        try
        {
            var question = await _questionRepository.GetByIdAsync(questionId);
            if (question == null)
                return ServiceResult<AnswerDto>.CreateError("Question not found");

            var answer = new Answer
            {
                QuestionId = questionId,
                Text = text,
                IsCorrect = isCorrect,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdAnswer = await _answerRepository.AddAsync(answer);
            return ServiceResult<AnswerDto>.CreateSuccess(MapToDto(createdAnswer));
        }
        catch (Exception ex)
        {
            return ServiceResult<AnswerDto>.CreateError($"Failed to create answer: {ex.Message}");
        }
    }

    public async Task<ServiceResult<AnswerDto>> GetAnswerByIdAsync(int id)
    {
        try
        {
            var answer = await _answerRepository.GetByIdAsync(id);
            if (answer == null)
                return ServiceResult<AnswerDto>.CreateError("Answer not found");

            return ServiceResult<AnswerDto>.CreateSuccess(MapToDto(answer));
        }
        catch (Exception ex)
        {
            return ServiceResult<AnswerDto>.CreateError($"Failed to get answer: {ex.Message}");
        }
    }

    public async Task<ServiceResult<IEnumerable<AnswerDto>>> GetAnswersByQuestionAsync(int questionId)
    {
        try
        {
            var answers = await _answerRepository.GetByQuestionIdAsync(questionId);
            return ServiceResult<IEnumerable<AnswerDto>>.CreateSuccess(
                answers.Select(MapToDto));
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<AnswerDto>>.CreateError($"Failed to get answers: {ex.Message}");
        }
    }

    public async Task<ServiceResult<IEnumerable<AnswerDto>>> GetCorrectAnswersByQuestionAsync(int questionId)
    {
        try
        {
            var answers = await _answerRepository.GetCorrectAnswersByQuestionIdAsync(questionId);
            return ServiceResult<IEnumerable<AnswerDto>>.CreateSuccess(
                answers.Select(MapToDto));
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<AnswerDto>>.CreateError($"Failed to get correct answers: {ex.Message}");
        }
    }

    private AnswerDto MapToDto(Answer answer)
    {
        return new AnswerDto
        {
            Id = answer.Id,
            QuestionId = answer.QuestionId,
            Text = answer.Text,
            IsCorrect = answer.IsCorrect,
            CreatedAt = answer.CreatedAt
        };
    }
}