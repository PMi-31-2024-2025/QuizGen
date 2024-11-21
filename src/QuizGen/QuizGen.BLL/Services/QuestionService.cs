using QuizGen.BLL.Models.Answer;
using QuizGen.BLL.Models.Base;
using QuizGen.BLL.Models.Question;
using QuizGen.BLL.Services.Interfaces;
using QuizGen.DAL.Interfaces;
using QuizGen.DAL.Models;

namespace QuizGen.BLL.Services;

public class QuestionService : IQuestionService
{
    private readonly IQuestionRepository _questionRepository;
    private readonly IQuizRepository _quizRepository;

    public QuestionService(IQuestionRepository questionRepository, IQuizRepository quizRepository)
    {
        _questionRepository = questionRepository;
        _quizRepository = quizRepository;
    }

    public async Task<ServiceResult<QuestionDto>> CreateQuestionAsync(int quizId, string text, string type, string explanation)
    {
        try
        {
            var quiz = await _quizRepository.GetByIdAsync(quizId);
            if (quiz == null)
                return ServiceResult<QuestionDto>.CreateError("Quiz not found");

            var question = new Question
            {
                QuizId = quizId,
                Text = text,
                Type = type,
                Explanation = explanation,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdQuestion = await _questionRepository.AddAsync(question);
            return ServiceResult<QuestionDto>.CreateSuccess(MapToDto(createdQuestion));
        }
        catch (Exception ex)
        {
            return ServiceResult<QuestionDto>.CreateError($"Failed to create question: {ex.Message}");
        }
    }

    public async Task<ServiceResult<QuestionDto>> GetQuestionByIdAsync(int id)
    {
        try
        {
            var question = await _questionRepository.GetByIdAsync(id);
            if (question == null)
                return ServiceResult<QuestionDto>.CreateError("Question not found");

            return ServiceResult<QuestionDto>.CreateSuccess(MapToDto(question));
        }
        catch (Exception ex)
        {
            return ServiceResult<QuestionDto>.CreateError($"Failed to get question: {ex.Message}");
        }
    }

    public async Task<ServiceResult<IEnumerable<QuestionDto>>> GetQuestionsByQuizAsync(int quizId)
    {
        try
        {
            var questions = await _questionRepository.GetByQuizIdAsync(quizId);
            return ServiceResult<IEnumerable<QuestionDto>>.CreateSuccess(
                questions.Select(MapToDto));
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<QuestionDto>>.CreateError($"Failed to get questions: {ex.Message}");
        }
    }

    public async Task<ServiceResult<IEnumerable<QuestionDto>>> GetQuestionsByTypeAsync(string type)
    {
        try
        {
            var questions = await _questionRepository.GetByTypeAsync(type);
            return ServiceResult<IEnumerable<QuestionDto>>.CreateSuccess(
                questions.Select(MapToDto));
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<QuestionDto>>.CreateError($"Failed to get questions: {ex.Message}");
        }
    }

    private QuestionDto MapToDto(Question question)
    {
        return new QuestionDto
        {
            Id = question.Id,
            QuizId = question.QuizId,
            Text = question.Text,
            Type = question.Type,
            Explanation = question.Explanation,
            CreatedAt = question.CreatedAt,
            Answers = question.Answers?.Select(a => new AnswerDto
            {
                Id = a.Id,
                Text = a.Text,
                IsCorrect = a.IsCorrect,
                CreatedAt = a.CreatedAt
            }).ToList()
        };
    }
} 