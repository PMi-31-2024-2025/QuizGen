using QuizGen.BLL.Models.Base;
using QuizGen.BLL.Models.QuizAnswer;
using QuizGen.BLL.Models.QuizTry;
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
    private readonly IQuestionRepository _questionRepository;
    private readonly IAnswerRepository _answerRepository;

    public QuizTryService(
        IQuizTryRepository quizTryRepository,
        IQuizRepository quizRepository,
        IUserRepository userRepository,
        IQuizAnswerRepository quizAnswerRepository,
        IQuestionRepository questionRepository,
        IAnswerRepository answerRepository)
    {
        _quizTryRepository = quizTryRepository;
        _quizRepository = quizRepository;
        _userRepository = userRepository;
        _quizAnswerRepository = quizAnswerRepository;
        _questionRepository = questionRepository;
        _answerRepository = answerRepository;
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

    public async Task<ServiceResult<QuizTryResultDto>> GetQuizTryByIdAsync(int quizTryId)
    {
        try
        {
            return await CalculateAndSaveScoreAsync(quizTryId);
        }
        catch (Exception ex)
        {
            return ServiceResult<QuizTryResultDto>.CreateError($"Failed to get quiz try: {ex.Message}");
        }
    }

    public async Task<ServiceResult<IEnumerable<QuizTryDto>>> GetQuizTriesByUserAsync(int userId)
    {
        try
        {
            var quizTries = await _quizTryRepository.GetByUserIdAsync(userId);
            
            // Load all related data in bulk to avoid multiple queries per entity
            var quizIds = quizTries.Select(qt => qt.QuizId).Distinct().ToList();
            var quizzes = await _quizRepository.GetByIdsAsync(quizIds);
            var quizDict = quizzes.ToDictionary(q => q.Id);
            
            var userIds = quizTries.Select(qt => qt.UserId).Distinct().ToList();
            var users = await _userRepository.GetByIdsAsync(userIds);
            var userDict = users.ToDictionary(u => u.Id);
            
            var quizTryIds = quizTries.Select(qt => qt.Id).ToList();
            var allAnswers = await _quizAnswerRepository.GetByQuizTryIdsAsync(quizTryIds);
            var answersDict = allAnswers.GroupBy(a => a.QuizTryId)
                                       .ToDictionary(g => g.Key, g => g.ToList());
            
            // Use the bulk loaded data to create DTOs
            var dtos = quizTries.Select(qt => MapToDtoWithContext(qt, quizDict, userDict, answersDict)).ToList();
            
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
            
            // Load all related data in bulk to avoid multiple queries per entity
            var quiz = await _quizRepository.GetByIdAsync(quizId);
            var quizDict = new Dictionary<int, Quiz> { { quizId, quiz } };
            
            var userIds = quizTries.Select(qt => qt.UserId).Distinct().ToList();
            var users = await _userRepository.GetByIdsAsync(userIds);
            var userDict = users.ToDictionary(u => u.Id);
            
            var quizTryIds = quizTries.Select(qt => qt.Id).ToList();
            var allAnswers = await _quizAnswerRepository.GetByQuizTryIdsAsync(quizTryIds);
            var answersDict = allAnswers.GroupBy(a => a.QuizTryId)
                                       .ToDictionary(g => g.Key, g => g.ToList());
            
            // Use the bulk loaded data to create DTOs
            var dtos = quizTries.Select(qt => MapToDtoWithContext(qt, quizDict, userDict, answersDict)).ToList();
            
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

    public async Task<ServiceResult<QuizTryDetailsDto>> GetQuizTryDetailsAsync(int quizTryId)
    {
        try
        {
            var quizTry = await _quizTryRepository.GetQuizTryWithDetailsAsync(quizTryId);
            if (quizTry == null)
                return ServiceResult<QuizTryDetailsDto>.CreateError("Quiz try not found");

            var questions = await _questionRepository.GetByQuizIdAsync(quizTry.QuizId);
            var answers = await _answerRepository.GetByQuizIdAsync(quizTry.QuizId);

            // Get the current question index based on the number of answers
            var answeredQuestionIds = quizTry.QuizAnswers
                .Select(qa => qa.QuestionId)
                .Distinct()
                .ToList();

            // Find the first unanswered question
            var currentQuestionIndex = 0;
            for (int i = 0; i < questions.Count(); i++)
            {
                if (!answeredQuestionIds.Contains(questions.ElementAt(i).Id))
                {
                    currentQuestionIndex = i;
                    break;
                }
                if (i == questions.Count() - 1)
                {
                    currentQuestionIndex = i;
                }
            }

            var dto = new QuizTryDetailsDto
            {
                Id = quizTry.Id,
                QuizId = quizTry.QuizId,
                UserId = quizTry.UserId,
                QuizName = quizTry.Quiz.Name,
                QuizPrompt = quizTry.Quiz.Prompt,
                Difficulty = quizTry.Quiz.Difficulty,
                CurrentQuestionIndex = currentQuestionIndex,
                TotalQuestions = quizTry.Quiz.NumQuestions,
                StartedAt = quizTry.StartedAt,
                Questions = questions.Select(q => new QuizTryQuestionDto
                {
                    Id = q.Id,
                    Text = q.Text,
                    Type = q.Type,
                    Explanation = q.Explanation,
                    AnswerOptions = answers
                        .Where(a => a.QuestionId == q.Id)
                        .Select(a => new QuizTryAnswerOptionDto
                        {
                            Id = a.Id,
                            Text = a.Text,
                            IsSelected = quizTry.QuizAnswers.Any(qa => qa.AnswerId == a.Id)
                        }).ToList()
                }).ToList()
            };

            return ServiceResult<QuizTryDetailsDto>.CreateSuccess(dto);
        }
        catch (Exception ex)
        {
            return ServiceResult<QuizTryDetailsDto>.CreateError($"Failed to get quiz try details: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> DeleteQuizTryAsync(int quizTryId)
    {
        try
        {
            var quizTry = await _quizTryRepository.GetByIdAsync(quizTryId);
            if (quizTry == null)
                return ServiceResult<bool>.CreateError("Quiz try not found");

            var result = await _quizTryRepository.DeleteWithAnswersAsync(quizTryId);
            return result
                ? ServiceResult<bool>.CreateSuccess(true)
                : ServiceResult<bool>.CreateError("Failed to delete quiz try");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.CreateError($"Failed to delete quiz try: {ex.Message}");
        }
    }

    public async Task<ServiceResult<QuizTryResultDto>> CalculateAndSaveScoreAsync(int quizTryId)
    {
        try
        {
            var quizTry = await _quizTryRepository.GetQuizTryWithDetailsAsync(quizTryId);
            if (quizTry == null)
                return ServiceResult<QuizTryResultDto>.CreateError("Quiz try not found");

            var questions = await _questionRepository.GetByQuizIdAsync(quizTry.QuizId);
            var answers = await _answerRepository.GetByQuizIdAsync(quizTry.QuizId);

            double totalScore = 0;
            var questionResults = new List<QuestionResultDto>();

            foreach (var question in questions)
            {
                var questionAnswers = answers.Where(a => a.QuestionId == question.Id).ToList();
                var userAnswers = quizTry.QuizAnswers.Where(qa => qa.QuestionId == question.Id).ToList();

                double questionScore = 0;
                var correctAnswers = questionAnswers.Where(a => a.IsCorrect).ToList();
                var incorrectAnswers = questionAnswers.Where(a => !a.IsCorrect).ToList();

                if (question.Type == "multi-select")
                {
                    // For multiple select questions
                    int totalCorrectAnswers = correctAnswers.Count;
                    int totalIncorrectAnswers = incorrectAnswers.Count;

                    // Count user's correct and incorrect selections
                    int correctSelections = userAnswers.Count(ua => correctAnswers.Any(ca => ca.Id == ua.AnswerId));
                    int incorrectSelections = userAnswers.Count(ua => incorrectAnswers.Any(ia => ia.Id == ua.AnswerId));
                    int missedCorrectAnswers = totalCorrectAnswers - correctSelections;

                    // Calculate score components
                    double correctScore = totalCorrectAnswers > 0 ? (double)correctSelections / totalCorrectAnswers : 0;
                    double incorrectPenalty = totalIncorrectAnswers > 0 ? (double)incorrectSelections / totalIncorrectAnswers : 0;
                    double missingPenalty = totalCorrectAnswers > 0 ? (double)missedCorrectAnswers / totalCorrectAnswers : 0;

                    // Final score calculation
                    questionScore = Math.Max(0, correctScore - incorrectPenalty - missingPenalty);
                }
                else // single-select or true-false
                {
                    if (userAnswers.Count == 1 && correctAnswers.Count == 1)
                    {
                        questionScore = userAnswers[0].AnswerId == correctAnswers[0].Id ? 1.0 : 0.0;
                    }
                }

                // Convert to percentage of total quiz score
                questionScore = Math.Max(0, Math.Min(1, questionScore));
                double questionPercentage = 1.0 / questions.Count() * 100;

                totalScore += questionScore;

                questionResults.Add(new QuestionResultDto
                {
                    Id = question.Id,
                    Text = question.Text,
                    IsCorrect = questionScore >= 0.99,
                    Explanation = question.Explanation,
                    Score = questionScore * questionPercentage, // Contribution to total score
                    CorrectPercentage = questionScore * 100, // Percentage of correct answers
                    Type = question.Type,
                    Answers = questionAnswers.Select(a => new AnswerResultDto
                    {
                        Id = a.Id,
                        Text = a.Text,
                        IsCorrect = a.IsCorrect,
                        WasSelected = userAnswers.Any(ua => ua.AnswerId == a.Id)
                    }).ToList()
                });
            }

            var finalScore = totalScore / questions.Count() * 100;

            quizTry.Score = finalScore;
            quizTry.FinishedAt = DateTime.UtcNow;
            quizTry.UpdatedAt = DateTime.UtcNow;
            await _quizTryRepository.UpdateAsync(quizTry);

            var result = new QuizTryResultDto
            {
                Id = quizTry.Id,
                QuizName = quizTry.Quiz.Name,
                Score = finalScore,
                TotalQuestions = questions.Count(),
                CorrectAnswers = questionResults.Count(q => q.IsCorrect),
                Questions = questionResults,
                StartedAt = quizTry.StartedAt,
                FinishedAt = quizTry.FinishedAt.Value
            };

            return ServiceResult<QuizTryResultDto>.CreateSuccess(result);
        }
        catch (Exception ex)
        {
            return ServiceResult<QuizTryResultDto>.CreateError($"Failed to calculate quiz score: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> SaveAnswersAsync(int quizTryId, int questionId, List<int> selectedAnswerIds)
    {
        try
        {
            var quizTry = await _quizTryRepository.GetByIdAsync(quizTryId);
            if (quizTry == null)
                return ServiceResult<bool>.CreateError("Quiz try not found");

            // Remove existing answers for this question
            await _quizTryRepository.RemoveAnswersForQuestionAsync(quizTryId, questionId);

            // Add new answers
            foreach (var answerId in selectedAnswerIds)
            {
                var quizAnswer = new QuizAnswer
                {
                    QuizTryId = quizTryId,
                    QuestionId = questionId,
                    AnswerId = answerId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _quizTryRepository.AddAnswerAsync(quizAnswer);
            }

            return ServiceResult<bool>.CreateSuccess(true);
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.CreateError($"Failed to save answers: {ex.Message}");
        }
    }

    public async Task<ServiceResult<QuizTryStatisticsDto>> GetUserStatisticsAsync(int userId)
    {
        try
        {
            var quizTries = await _quizTryRepository.GetCompletedByUserIdAsync(userId);

            if (!quizTries.Any())
                return ServiceResult<QuizTryStatisticsDto>.CreateSuccess(new QuizTryStatisticsDto());

            var completedTries = quizTries.Where(qt => qt.FinishedAt.HasValue).ToList();
            var statistics = new QuizTryStatisticsDto
            {
                TotalTries = completedTries.Count,
                AverageScore = completedTries.Average(qt => qt.Score ?? 0),
                AverageTime = TimeSpan.FromTicks((long)completedTries
                    .Where(qt => qt.FinishedAt.HasValue)
                    .Average(qt => (qt.FinishedAt.Value - qt.StartedAt).Ticks)),
                PassRate = (double)completedTries.Count(qt => (qt.Score ?? 0) >= 50) / completedTries.Count * 100
            };

            return ServiceResult<QuizTryStatisticsDto>.CreateSuccess(statistics);
        }
        catch (Exception ex)
        {
            return ServiceResult<QuizTryStatisticsDto>.CreateError($"Failed to get user statistics: {ex.Message}");
        }
    }

    public async Task<ServiceResult<IEnumerable<QuizTryListItemDto>>> GetUserQuizTriesAsync(int userId)
    {
        try
        {
            var quizTries = await _quizTryRepository.GetByUserIdAsync(userId);
            var dtos = await Task.WhenAll(quizTries.Select(MapToListItemDto));
            return ServiceResult<IEnumerable<QuizTryListItemDto>>.CreateSuccess(dtos);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<QuizTryListItemDto>>.CreateError($"Failed to get user quiz tries: {ex.Message}");
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

    // New method that uses pre-loaded data instead of making individual DB calls
    private QuizTryDto MapToDtoWithContext(
        QuizTry quizTry, 
        Dictionary<int, Quiz> quizDict, 
        Dictionary<int, User> userDict,
        Dictionary<int, List<QuizAnswer>> answersDict)
    {
        var quiz = quizDict.TryGetValue(quizTry.QuizId, out var q) ? q : null;
        var user = userDict.TryGetValue(quizTry.UserId, out var u) ? u : null;
        var answers = answersDict.TryGetValue(quizTry.Id, out var answerList) ? answerList : new List<QuizAnswer>();

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

    private async Task<QuizTryListItemDto> MapToListItemDto(QuizTry quizTry)
    {
        var quiz = await _quizRepository.GetByIdAsync(quizTry.QuizId);
        if (quiz == null)
            throw new Exception($"Quiz with ID {quizTry.QuizId} not found");

        return new QuizTryListItemDto
        {
            Id = quizTry.Id,
            QuizName = quiz.Name,
            Difficulty = quiz.Difficulty,
            QuestionTypes = quiz.AllowedTypes,
            TotalQuestions = quiz.NumQuestions,
            Score = quizTry.Score ?? 0,
            Duration = quizTry.FinishedAt.HasValue ? quizTry.FinishedAt.Value - quizTry.StartedAt : TimeSpan.Zero,
            StartedAt = quizTry.StartedAt,
            FinishedAt = quizTry.FinishedAt ?? DateTime.MinValue
        };
    }
}