using QuizGen.BLL.Models.Base;
using QuizGen.BLL.Models.Quiz;
using QuizGen.BLL.Services.Interfaces;
using QuizGen.DAL.Interfaces;
using QuizGen.DAL.Models;

namespace QuizGen.BLL.Services;

public class QuizService : IQuizService
{
    private readonly IOpenAiService _openAiService;
    private readonly IQuizRepository _quizRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly IAnswerRepository _answerRepository;
    private readonly IUserRepository _userRepository;

    public QuizService(
        IOpenAiService openAiService,
        IQuizRepository quizRepository,
        IQuestionRepository questionRepository,
        IAnswerRepository answerRepository,
        IUserRepository userRepository)
    {
        _openAiService = openAiService;
        _quizRepository = quizRepository;
        _questionRepository = questionRepository;
        _answerRepository = answerRepository;
        _userRepository = userRepository;
    }

    public async Task<ServiceResult<QuizDto>> CreateQuizAsync(int authorId, string topic, string difficulty, int numQuestions, string[] allowedTypes)
    {
        var user = await _userRepository.GetByIdAsync(authorId);
        if (user == null || string.IsNullOrEmpty(user.OpenAiApiKey))
        {
            return ServiceResult<QuizDto>.CreateError("User not found or OpenAI API key not set");
        }

        var request = new QuizGenerationRequest
        {
            Topic = topic,
            Difficulty = difficulty,
            QuestionCount = numQuestions,
            AllowedTypes = allowedTypes
        };

        var generationResult = await _openAiService.GenerateQuizAsync(
            request,
            user.OpenAiApiKey,
            "gpt-4o-mini"
        );

        if (!generationResult.Success)
        {
            return ServiceResult<QuizDto>.CreateError(generationResult.Message);
        }

        var generatedQuiz = generationResult.Data;
        if (_openAiService.IsGenerationFailed(generatedQuiz))
        {
            return ServiceResult<QuizDto>.CreateError("Failed to generate a valid quiz");
        }

        try
        {
            // Create the quiz entity
            var quiz = new Quiz
            {
                AuthorId = authorId,
                Name = generatedQuiz.Name,
                Prompt = topic,
                Difficulty = difficulty,
                NumQuestions = numQuestions,
                AllowedTypes = allowedTypes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Save quiz to get its ID
            var savedQuiz = await _quizRepository.AddAsync(quiz);

            // Create and save questions
            foreach (var generatedQuestion in generatedQuiz.Questions)
            {
                var question = new Question
                {
                    QuizId = savedQuiz.Id,
                    Text = generatedQuestion.Text,
                    Type = generatedQuestion.Type,
                    Explanation = generatedQuestion.Explanation,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var savedQuestion = await _questionRepository.AddAsync(question);

                // Create and save answers for the question
                foreach (var generatedOption in generatedQuestion.Options)
                {
                    var answer = new Answer
                    {
                        QuestionId = savedQuestion.Id,
                        Text = generatedOption.Text,
                        IsCorrect = generatedOption.IsCorrect,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _ = await _answerRepository.AddAsync(answer);
                }
            }

            // Get the author's name for the DTO
            var author = await _userRepository.GetByIdAsync(authorId);
            if (author == null)
                return ServiceResult<QuizDto>.CreateError("Quiz author not found");

            return ServiceResult<QuizDto>.CreateSuccess(MapToDto(savedQuiz, author.Name));
        }
        catch (Exception ex)
        {
            return ServiceResult<QuizDto>.CreateError($"Failed to save generated quiz: {ex.Message} {ex.InnerException?.Message}");
        }
    }

    public async Task<ServiceResult<QuizDto>> GetQuizByIdAsync(int id)
    {
        try
        {
            var quiz = await _quizRepository.GetByIdAsync(id);
            if (quiz == null)
                return ServiceResult<QuizDto>.CreateError("Quiz not found");

            var author = await _userRepository.GetByIdAsync(quiz.AuthorId);
            if (author == null)
                return ServiceResult<QuizDto>.CreateError("Quiz author not found");

            return ServiceResult<QuizDto>.CreateSuccess(MapToDto(quiz, author.Name));
        }
        catch (Exception ex)
        {
            return ServiceResult<QuizDto>.CreateError($"Failed to get quiz: {ex.Message}");
        }
    }

    public async Task<ServiceResult<IEnumerable<QuizDto>>> GetQuizzesByAuthorAsync(int authorId)
    {
        try
        {
            var author = await _userRepository.GetByIdAsync(authorId);
            if (author == null)
                return ServiceResult<IEnumerable<QuizDto>>.CreateError("Author not found");

            var quizzes = await _quizRepository.GetByAuthorIdAsync(authorId);
            var quizDtos = quizzes.Select(q => MapToDto(q, author.Name));

            return ServiceResult<IEnumerable<QuizDto>>.CreateSuccess(quizDtos);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<QuizDto>>.CreateError($"Failed to get quizzes: {ex.Message}");
        }
    }

    public async Task<ServiceResult<IEnumerable<QuizDto>>> GetQuizzesByDifficultyAsync(string difficulty)
    {
        try
        {
            var quizzes = await _quizRepository.GetByDifficultyAsync(difficulty);
            var quizDtos = new List<QuizDto>();

            foreach (var quiz in quizzes)
            {
                var author = await _userRepository.GetByIdAsync(quiz.AuthorId);
                if (author != null)
                {
                    quizDtos.Add(MapToDto(quiz, author.Name));
                }
            }

            return ServiceResult<IEnumerable<QuizDto>>.CreateSuccess(quizDtos);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<QuizDto>>.CreateError($"Failed to get quizzes: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> DeleteQuizAsync(int id)
    {
        try
        {
            var quiz = await _quizRepository.GetByIdAsync(id);
            if (quiz == null)
                return ServiceResult<bool>.CreateError("Quiz not found");

            await _quizRepository.DeleteAsync(quiz.Id);
            return ServiceResult<bool>.CreateSuccess(true);
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.CreateError($"Failed to delete quiz: {ex.Message}");
        }
    }

    private QuizDto MapToDto(Quiz quiz, string authorName)
    {
        return new QuizDto
        {
            Id = quiz.Id,
            AuthorId = quiz.AuthorId,
            AuthorName = authorName,
            Name = quiz.Name,
            Prompt = quiz.Prompt,
            Difficulty = quiz.Difficulty,
            NumQuestions = quiz.NumQuestions,
            AllowedTypes = quiz.AllowedTypes,
            CreatedAt = quiz.CreatedAt
        };
    }
}