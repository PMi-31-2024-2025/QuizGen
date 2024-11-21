using QuizGen.BLL.Models.Base;
using QuizGen.BLL.Models.Quiz;
using QuizGen.BLL.Services.Interfaces;
using QuizGen.DAL.Interfaces;
using QuizGen.DAL.Models;

namespace QuizGen.BLL.Services;

public class QuizService : IQuizService
{
    private readonly IQuizRepository _quizRepository;
    private readonly IUserRepository _userRepository;

    public QuizService(IQuizRepository quizRepository, IUserRepository userRepository)
    {
        _quizRepository = quizRepository;
        _userRepository = userRepository;
    }

    public async Task<ServiceResult<QuizDto>> CreateQuizAsync(int authorId, string prompt, string difficulty, int numQuestions, string[] allowedTypes)
    {
        try
        {
            var author = await _userRepository.GetByIdAsync(authorId);
            if (author == null)
                return ServiceResult<QuizDto>.CreateError("Author not found");

            var quiz = new Quiz
            {
                AuthorId = authorId,
                Prompt = prompt,
                Difficulty = difficulty,
                NumQuestions = numQuestions,
                AllowedTypes = allowedTypes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdQuiz = await _quizRepository.AddAsync(quiz);
            return ServiceResult<QuizDto>.CreateSuccess(MapToDto(createdQuiz, author.Name));
        }
        catch (Exception ex)
        {
            return ServiceResult<QuizDto>.CreateError($"Failed to create quiz: {ex.Message}");
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

    private QuizDto MapToDto(Quiz quiz, string authorName)
    {
        return new QuizDto
        {
            Id = quiz.Id,
            AuthorId = quiz.AuthorId,
            AuthorName = authorName,
            Prompt = quiz.Prompt,
            Difficulty = quiz.Difficulty,
            NumQuestions = quiz.NumQuestions,
            AllowedTypes = quiz.AllowedTypes,
            CreatedAt = quiz.CreatedAt
        };
    }
} 