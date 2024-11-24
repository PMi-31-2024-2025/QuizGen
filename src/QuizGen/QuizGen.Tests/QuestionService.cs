using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuizGen.BLL.Models.Base;
using QuizGen.BLL.Models.Question;
using QuizGen.BLL.Services;
using QuizGen.DAL.Interfaces;
using QuizGen.DAL.Models;

public class QuestionServiceTests
{
    private readonly Mock<IQuestionRepository> _mockQuestionRepository;
    private readonly Mock<IQuizRepository> _mockQuizRepository;
    private readonly QuestionService _questionService;

    public QuestionServiceTests()
    {
        _mockQuestionRepository = new Mock<IQuestionRepository>();
        _mockQuizRepository = new Mock<IQuizRepository>();
        _questionService = new QuestionService(_mockQuestionRepository.Object, _mockQuizRepository.Object);
    }

    [Fact]
    public async Task CreateQuestionAsync_ValidQuizId_ReturnsSuccess()
    {
        var quizId = 1;
        var text = "Sample Question";
        var type = "Multiple Choice";
        var explanation = "This is an explanation.";
        var question = new Question
        {
            Id = 1,
            QuizId = quizId,
            Text = text,
            Type = type,
            Explanation = explanation,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockQuizRepository.Setup(repo => repo.GetByIdAsync(quizId))
            .ReturnsAsync(new Quiz { Id = quizId, AuthorId=1, Name="Quiz name", Prompt="Cool answer", Difficulty="Hard",NumQuestions=10, AllowedTypes = [] });
        _mockQuestionRepository.Setup(repo => repo.AddAsync(It.IsAny<Question>()))
            .ReturnsAsync(question);

        var result = await _questionService.CreateQuestionAsync(quizId, text, type, explanation);

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(question.Id, result.Data.Id);
        _mockQuizRepository.Verify(repo => repo.GetByIdAsync(quizId), Times.Once);
        _mockQuestionRepository.Verify(repo => repo.AddAsync(It.IsAny<Question>()), Times.Once);
    }

    [Fact]
    public async Task CreateQuestionAsync_InvalidQuizId_ReturnsError()
    {
        var quizId = 999;
        var text = "Sample Question";
        var type = "Multiple Choice";
        var explanation = "This is an explanation.";

        _mockQuizRepository.Setup(repo => repo.GetByIdAsync(quizId))
            .ReturnsAsync((Quiz)null);

        var result = await _questionService.CreateQuestionAsync(quizId, text, type, explanation);

        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("Quiz not found", result.Message);
        _mockQuizRepository.Verify(repo => repo.GetByIdAsync(quizId), Times.Once);
        _mockQuestionRepository.Verify(repo => repo.AddAsync(It.IsAny<Question>()), Times.Never);
    }

    [Fact]
    public async Task GetQuestionByIdAsync_ValidId_ReturnsSuccess()
    {
        var questionId = 1;
        var question = new Question
        {
            Id = questionId,
            QuizId = 1,
            Text = "Sample Question",
            Type = "Multiple Choice",
            Explanation = "Sample Explanation",
            CreatedAt = DateTime.UtcNow
        };

        _mockQuestionRepository.Setup(repo => repo.GetByIdAsync(questionId))
            .ReturnsAsync(question);


        var result = await _questionService.GetQuestionByIdAsync(questionId);


        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(questionId, result.Data.Id);
        _mockQuestionRepository.Verify(repo => repo.GetByIdAsync(questionId), Times.Once);
    }

    [Fact]
    public async Task GetQuestionByIdAsync_InvalidId_ReturnsError()
    {

        var questionId = 999;

        _mockQuestionRepository.Setup(repo => repo.GetByIdAsync(questionId))
            .ReturnsAsync((Question)null);


        var result = await _questionService.GetQuestionByIdAsync(questionId);

        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("Question not found", result.Message);
        _mockQuestionRepository.Verify(repo => repo.GetByIdAsync(questionId), Times.Once);
    }

    [Fact]
    public async Task GetQuestionsByQuizAsync_ValidQuizId_ReturnsQuestions()
    {

        var quizId = 1;
        var questions = new List<Question>
        {
            new Question { Id = 1, QuizId = quizId, Text = "Question 1", Type="Single select" },
            new Question { Id = 2, QuizId = quizId, Text = "Question 2",Type="Single select" }
        };

        _mockQuestionRepository.Setup(repo => repo.GetByQuizIdAsync(quizId))
            .ReturnsAsync(questions);


        var result = await _questionService.GetQuestionsByQuizAsync(quizId);


        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(2, result.Data.Count());
        _mockQuestionRepository.Verify(repo => repo.GetByQuizIdAsync(quizId), Times.Once);
    }

    [Fact]
    public async Task GetQuestionsByQuizAsync_InvalidQuizId_ReturnsEmptyList()
    {

        var quizId = 999;

        _mockQuestionRepository.Setup(repo => repo.GetByQuizIdAsync(quizId))
            .ReturnsAsync(new List<Question>());


        var result = await _questionService.GetQuestionsByQuizAsync(quizId);


        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Empty(result.Data);
        _mockQuestionRepository.Verify(repo => repo.GetByQuizIdAsync(quizId), Times.Once);
    }

    [Fact]
    public async Task GetQuestionsByTypeAsync_ValidType_ReturnsQuestions()
    {

        var type = "Multiple Choice";
        var questions = new List<Question>
        {
            new Question { Id = 1, Type = type, Text = "Question 1",QuizId=5 },
            new Question { Id = 2, Type = type, Text = "Question 2",QuizId=5 }
        };

        _mockQuestionRepository.Setup(repo => repo.GetByTypeAsync(type))
            .ReturnsAsync(questions);

        var result = await _questionService.GetQuestionsByTypeAsync(type);


        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(2, result.Data.Count());
        _mockQuestionRepository.Verify(repo => repo.GetByTypeAsync(type), Times.Once);
    }

    [Fact]
    public async Task GetQuestionsByTypeAsync_InvalidType_ReturnsEmptyList()
    {

        var type = "Invalid Type";

        _mockQuestionRepository.Setup(repo => repo.GetByTypeAsync(type))
            .ReturnsAsync(new List<Question>());


        var result = await _questionService.GetQuestionsByTypeAsync(type);

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Empty(result.Data);
        _mockQuestionRepository.Verify(repo => repo.GetByTypeAsync(type), Times.Once);
    }
}
