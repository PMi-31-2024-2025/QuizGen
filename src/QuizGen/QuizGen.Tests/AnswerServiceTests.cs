using Moq;
using QuizGen.BLL.Services;
using QuizGen.DAL.Interfaces;
using QuizGen.DAL.Models;

public class AnswerServiceTests
{
    private readonly Mock<IAnswerRepository> _mockAnswerRepository;
    private readonly Mock<IQuestionRepository> _mockQuestionRepository;
    private readonly AnswerService _answerService;

    public AnswerServiceTests()
    {
        _mockAnswerRepository = new Mock<IAnswerRepository>();
        _mockQuestionRepository = new Mock<IQuestionRepository>();
        _answerService = new AnswerService(_mockAnswerRepository.Object, _mockQuestionRepository.Object);
    }

    [Fact]
    public async Task CreateAnswerAsync_ValidQuestionId_ReturnsSuccess()
    {
        var questionId = 1;
        var text = "Sample Answer";
        var isCorrect = true;
        var answer = new Answer
        {
            Id = 1,
            QuestionId = questionId,
            Text = text,
            IsCorrect = isCorrect,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _ = _mockQuestionRepository.Setup(repo => repo.GetByIdAsync(questionId))
            .ReturnsAsync(new Question { Id = questionId, QuizId = 1, Text = "Sample text", Type = "ads" });
        _ = _mockAnswerRepository.Setup(repo => repo.AddAsync(It.IsAny<Answer>()))
            .ReturnsAsync(answer);

        var result = await _answerService.CreateAnswerAsync(questionId, text, isCorrect);

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(answer.Id, result.Data.Id);
        _mockQuestionRepository.Verify(repo => repo.GetByIdAsync(questionId), Times.Once);
        _mockAnswerRepository.Verify(repo => repo.AddAsync(It.IsAny<Answer>()), Times.Once);
    }

    [Fact]
    public async Task CreateAnswerAsync_InvalidQuestionId_ReturnsError()
    {
        var questionId = 999;
        var text = "Sample Answer";
        var isCorrect = true;

        _ = _mockQuestionRepository.Setup(repo => repo.GetByIdAsync(questionId))
            .ReturnsAsync((Question)null);

        var result = await _answerService.CreateAnswerAsync(questionId, text, isCorrect);

        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("Question not found", result.Message);
        _mockQuestionRepository.Verify(repo => repo.GetByIdAsync(questionId), Times.Once);
        _mockAnswerRepository.Verify(repo => repo.AddAsync(It.IsAny<Answer>()), Times.Never);
    }

    [Fact]
    public async Task GetAnswerByIdAsync_ValidId_ReturnsSuccess()
    {
        var answerId = 1;
        var answer = new Answer
        {
            Id = answerId,
            QuestionId = 2,
            Text = "Sample Answer",
            IsCorrect = true,
            CreatedAt = DateTime.UtcNow
        };

        _ = _mockAnswerRepository.Setup(repo => repo.GetByIdAsync(answerId))
            .ReturnsAsync(answer);

        var result = await _answerService.GetAnswerByIdAsync(answerId);

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(answerId, result.Data.Id);
        _mockAnswerRepository.Verify(repo => repo.GetByIdAsync(answerId), Times.Once);
    }

    [Fact]
    public async Task GetAnswerByIdAsync_InvalidId_ReturnsError()
    {
        var answerId = 999;

        _ = _mockAnswerRepository.Setup(repo => repo.GetByIdAsync(answerId))
            .ReturnsAsync((Answer)null);

        var result = await _answerService.GetAnswerByIdAsync(answerId);

        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("Answer not found", result.Message);
        _mockAnswerRepository.Verify(repo => repo.GetByIdAsync(answerId), Times.Once);
    }

    [Fact]
    public async Task GetAnswersByQuestionAsync_ValidQuestionId_ReturnsAnswers()
    {
        var questionId = 2;
        var answers = new List<Answer>
        {
            new Answer { Id = 1, QuestionId = questionId, Text = "Answer 1", IsCorrect = true },
            new Answer { Id = 2, QuestionId = questionId, Text = "Answer 2", IsCorrect = false }
        };

        _ = _mockAnswerRepository.Setup(repo => repo.GetByQuestionIdAsync(questionId))
            .ReturnsAsync(answers);

        var result = await _answerService.GetAnswersByQuestionAsync(questionId);

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(2, result.Data.Count());
        _mockAnswerRepository.Verify(repo => repo.GetByQuestionIdAsync(questionId), Times.Once);
    }

    [Fact]
    public async Task GetCorrectAnswersByQuestionAsync_ValidQuestionId_ReturnsCorrectAnswers()
    {
        var questionId = 2;
        var answers = new List<Answer>
        {
            new Answer { Id = 1, QuestionId = questionId, Text = "Correct Answer", IsCorrect = true }
        };

        _ = _mockAnswerRepository.Setup(repo => repo.GetCorrectAnswersByQuestionIdAsync(questionId))
            .ReturnsAsync(answers);

        var result = await _answerService.GetCorrectAnswersByQuestionAsync(questionId);

        Assert.NotNull(result);
        Assert.True(result.Success);
        _ = Assert.Single(result.Data);
        Assert.True(result.Data.First().IsCorrect);
        _mockAnswerRepository.Verify(repo => repo.GetCorrectAnswersByQuestionIdAsync(questionId), Times.Once);
    }
}
