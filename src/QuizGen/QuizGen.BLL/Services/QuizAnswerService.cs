using QuizGen.BLL.Models.Base;
using QuizGen.BLL.Models.QuizAnswer;
using QuizGen.BLL.Services.Interfaces;
using QuizGen.DAL.Interfaces;
using QuizGen.DAL.Models;

namespace QuizGen.BLL.Services;

public class QuizAnswerService : IQuizAnswerService
{
    private readonly IQuizAnswerRepository _quizAnswerRepository;
    private readonly IQuizTryRepository _quizTryRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly IAnswerRepository _answerRepository;

    public QuizAnswerService(
        IQuizAnswerRepository quizAnswerRepository,
        IQuizTryRepository quizTryRepository,
        IQuestionRepository questionRepository,
        IAnswerRepository answerRepository)
    {
        _quizAnswerRepository = quizAnswerRepository;
        _quizTryRepository = quizTryRepository;
        _questionRepository = questionRepository;
        _answerRepository = answerRepository;
    }

    public async Task<ServiceResult<QuizAnswerDto>> CreateQuizAnswerAsync(int quizTryId, int questionId, int answerId)
    {
        try
        {
            var quizTry = await _quizTryRepository.GetByIdAsync(quizTryId);
            if (quizTry == null)
                return ServiceResult<QuizAnswerDto>.CreateError("QuizTry not found");

            var question = await _questionRepository.GetByIdAsync(questionId);
            if (question == null)
                return ServiceResult<QuizAnswerDto>.CreateError("Question not found");

            var answer = await _answerRepository.GetByIdAsync(answerId);
            if (answer == null)
                return ServiceResult<QuizAnswerDto>.CreateError("Answer not found");

            var quizAnswer = new QuizAnswer
            {
                QuizTryId = quizTryId,
                QuestionId = questionId,
                AnswerId = answerId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdQuizAnswer = await _quizAnswerRepository.AddAsync(quizAnswer);
            return ServiceResult<QuizAnswerDto>.CreateSuccess(MapToDto(createdQuizAnswer, question, answer));
        }
        catch (Exception ex)
        {
            return ServiceResult<QuizAnswerDto>.CreateError($"Failed to create quiz answer: {ex.Message}");
        }
    }

    public async Task<ServiceResult<QuizAnswerDto>> GetQuizAnswerByIdAsync(int id)
    {
        try
        {
            var quizAnswer = await _quizAnswerRepository.GetByIdAsync(id);
            if (quizAnswer == null)
                return ServiceResult<QuizAnswerDto>.CreateError("QuizAnswer not found");

            var question = await _questionRepository.GetByIdAsync(quizAnswer.QuestionId);
            var answer = await _answerRepository.GetByIdAsync(quizAnswer.AnswerId);

            return ServiceResult<QuizAnswerDto>.CreateSuccess(MapToDto(quizAnswer, question, answer));
        }
        catch (Exception ex)
        {
            return ServiceResult<QuizAnswerDto>.CreateError($"Failed to get quiz answer: {ex.Message}");
        }
    }

    public async Task<ServiceResult<IEnumerable<QuizAnswerDto>>> GetQuizAnswersByQuizTryAsync(int quizTryId)
    {
        try
        {
            var quizAnswers = await _quizAnswerRepository.GetByQuizTryIdAsync(quizTryId);
            var dtos = new List<QuizAnswerDto>();

            foreach (var qa in quizAnswers)
            {
                dtos.Add(MapToDto(qa, qa.Question, qa.Answer));
            }

            return ServiceResult<IEnumerable<QuizAnswerDto>>.CreateSuccess(dtos);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<QuizAnswerDto>>.CreateError($"Failed to get quiz answers: {ex.Message}");
        }
    }

    public async Task<ServiceResult<IEnumerable<QuizAnswerDto>>> GetQuizAnswersByQuestionAsync(int questionId)
    {
        try
        {
            var quizAnswers = await _quizAnswerRepository.GetByQuestionIdAsync(questionId);
            var dtos = new List<QuizAnswerDto>();

            foreach (var qa in quizAnswers)
            {
                dtos.Add(MapToDto(qa, qa.Question, qa.Answer));
            }

            return ServiceResult<IEnumerable<QuizAnswerDto>>.CreateSuccess(dtos);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<QuizAnswerDto>>.CreateError($"Failed to get quiz answers: {ex.Message}");
        }
    }

    private QuizAnswerDto MapToDto(QuizAnswer quizAnswer, Question question, Answer answer)
    {
        return new QuizAnswerDto
        {
            Id = quizAnswer.Id,
            QuizTryId = quizAnswer.QuizTryId,
            QuestionId = quizAnswer.QuestionId,
            AnswerId = quizAnswer.AnswerId,
            QuestionText = question.Text,
            AnswerText = answer.Text,
            IsCorrect = answer.IsCorrect,
            CreatedAt = quizAnswer.CreatedAt
        };
    }
}