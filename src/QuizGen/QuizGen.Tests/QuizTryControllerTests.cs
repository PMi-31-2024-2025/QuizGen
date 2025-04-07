using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using QuizGen.API.Controllers;
using QuizGen.BLL.Models.Quiz;
using QuizGen.BLL.Models.QuizAnswer;
using QuizGen.BLL.Models.QuizTry;
using QuizGen.BLL.Models.Base;
using QuizGen.BLL.Services.Interfaces;
using System.Security.Claims;

namespace QuizGen.Tests
{
    public class QuizTryControllerTests
    {
        private readonly Mock<IQuizTryService> _mockQuizTryService;
        private readonly Mock<IQuizAnswerService> _mockQuizAnswerService;
        private readonly Mock<IQuizService> _mockQuizService;
        private readonly QuizTryController _quizTryController;

        public QuizTryControllerTests()
        {
            _mockQuizTryService = new Mock<IQuizTryService>();
            _mockQuizAnswerService = new Mock<IQuizAnswerService>();
            _mockQuizService = new Mock<IQuizService>();
            _quizTryController = new QuizTryController(
                _mockQuizTryService.Object,
                _mockQuizAnswerService.Object,
                _mockQuizService.Object);
        }

        private void SetupAuthenticatedUser(int userId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "test");
            var principal = new ClaimsPrincipal(identity);
            _quizTryController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        [Fact]
        public async Task StartQuizTry_ValidQuiz_ReturnsCreatedResult()
        {
            // Arrange
            var userId = 1;
            var quizId = 1;
            SetupAuthenticatedUser(userId);

            var quiz = new QuizDto
            {
                Id = quizId,
                AuthorId = userId,
                Name = "Test Quiz",
                AuthorName = "Test User"
            };

            var quizResult = new ServiceResult<QuizDto>
            {
                Success = true,
                Data = quiz
            };

            var quizTry = new QuizTryDto
            {
                Id = 1,
                QuizId = quizId,
                UserId = userId,
                QuizPrompt = "Test Quiz",
                UserName = "Test User",
                StartedAt = DateTime.UtcNow
            };

            var quizTryResult = new ServiceResult<QuizTryDto>
            {
                Success = true,
                Data = quizTry
            };

            _mockQuizService.Setup(s => s.GetQuizByIdAsync(quizId))
                .Returns(Task.FromResult(quizResult));

            _mockQuizTryService.Setup(s => s.StartQuizTryAsync(quizId, userId))
                .Returns(Task.FromResult(quizTryResult));

            // Act
            var result = await _quizTryController.StartQuizTry(quizId);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(QuizTryController.GetQuizTryDetails), createdResult.ActionName);
            Assert.Equal(quizId, createdResult.RouteValues["quizId"]);
            Assert.Equal(quizTry.Id, createdResult.RouteValues["attemptId"]);
            var response = Assert.IsType<QuizTryDto>(createdResult.Value);
            Assert.Equal(quizId, response.QuizId);
            Assert.Equal(userId, response.UserId);
        }

        [Fact]
        public async Task StartQuizTry_NotAuthorized_ReturnsForbid()
        {
            // Arrange
            var userId = 1;
            var quizId = 1;
            SetupAuthenticatedUser(userId);

            var quiz = new QuizDto
            {
                Id = quizId,
                AuthorId = 2, // Different author
                Name = "Test Quiz",
                AuthorName = "Other User"
            };

            var quizResult = new ServiceResult<QuizDto>
            {
                Success = true,
                Data = quiz
            };

            _mockQuizService.Setup(s => s.GetQuizByIdAsync(quizId))
                .Returns(Task.FromResult(quizResult));

            // Act
            var result = await _quizTryController.StartQuizTry(quizId);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task GetMyQuizTries_AuthenticatedUser_ReturnsOkResult()
        {
            // Arrange
            var userId = 1;
            SetupAuthenticatedUser(userId);

            var quizTries = new List<QuizTryDto>
            {
                new QuizTryDto { Id = 1, QuizId = 1, UserId = userId, QuizPrompt = "Quiz 1" },
                new QuizTryDto { Id = 2, QuizId = 2, UserId = userId, QuizPrompt = "Quiz 2" }
            };

            var expectedResponse = new ServiceResult<IEnumerable<QuizTryDto>>
            {
                Success = true,
                Data = quizTries
            };

            _mockQuizTryService.Setup(s => s.GetQuizTriesByUserAsync(userId))
                .Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _quizTryController.GetMyQuizTries();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<List<QuizTryDto>>(okResult.Value);
            Assert.Equal(2, response.Count);
            Assert.Equal("Quiz 1", response[0].QuizPrompt);
            Assert.Equal("Quiz 2", response[1].QuizPrompt);
        }

        [Fact]
        public async Task GetQuizAttempts_ValidQuiz_ReturnsOkResult()
        {
            // Arrange
            var userId = 1;
            var quizId = 1;
            SetupAuthenticatedUser(userId);

            var quiz = new QuizDto
            {
                Id = quizId,
                AuthorId = userId,
                Name = "Test Quiz",
                AuthorName = "Test User"
            };

            var quizResult = new ServiceResult<QuizDto>
            {
                Success = true,
                Data = quiz
            };

            var quizTries = new List<QuizTryDto>
            {
                new QuizTryDto { Id = 1, QuizId = quizId, UserId = userId, QuizPrompt = "Quiz 1" },
                new QuizTryDto { Id = 2, QuizId = quizId, UserId = userId, QuizPrompt = "Quiz 2" }
            };

            var expectedResponse = new ServiceResult<IEnumerable<QuizTryDto>>
            {
                Success = true,
                Data = quizTries
            };

            _mockQuizService.Setup(s => s.GetQuizByIdAsync(quizId))
                .Returns(Task.FromResult(quizResult));

            _mockQuizTryService.Setup(s => s.GetQuizTriesByQuizAsync(quizId))
                .Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _quizTryController.GetQuizAttempts(quizId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<List<QuizTryDto>>(okResult.Value);
            Assert.Equal(2, response.Count);
            Assert.All(response, t => Assert.Equal(userId, t.UserId));
        }

        [Fact]
        public async Task GetQuizTryDetails_ValidAttempt_ReturnsOkResult()
        {
            // Arrange
            var userId = 1;
            var quizId = 1;
            var attemptId = 1;
            SetupAuthenticatedUser(userId);

            var quizTryDetails = new QuizTryDetailsDto
            {
                Id = attemptId,
                QuizId = quizId,
                UserId = userId,
                QuizName = "Test Quiz",
                QuizPrompt = "Test Prompt",
                Difficulty = "Easy",
                CurrentQuestionIndex = 0,
                TotalQuestions = 5,
                StartedAt = DateTime.UtcNow
            };

            var expectedResponse = new ServiceResult<QuizTryDetailsDto>
            {
                Success = true,
                Data = quizTryDetails
            };

            _mockQuizTryService.Setup(s => s.GetQuizTryDetailsAsync(attemptId))
                .Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _quizTryController.GetQuizTryDetails(quizId, attemptId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<QuizTryDetailsDto>(okResult.Value);
            Assert.Equal(attemptId, response.Id);
            Assert.Equal(quizId, response.QuizId);
            Assert.Equal(userId, response.UserId);
        }

        [Fact]
        public async Task SubmitAnswer_ValidAnswer_ReturnsCreatedResult()
        {
            // Arrange
            var userId = 1;
            var quizId = 1;
            var attemptId = 1;
            var questionId = 1;
            var answerId = 1;
            SetupAuthenticatedUser(userId);

            var quizTryDetails = new QuizTryDetailsDto
            {
                Id = attemptId,
                QuizId = quizId,
                UserId = userId,
                QuizName = "Test Quiz"
            };

            var quizTryResult = new ServiceResult<QuizTryDetailsDto>
            {
                Success = true,
                Data = quizTryDetails
            };

            var quizAnswer = new QuizAnswerDto
            {
                Id = 1,
                QuizTryId = attemptId,
                QuestionId = questionId,
                AnswerId = answerId,
                QuestionText = "Test Question",
                AnswerText = "Test Answer",
                IsCorrect = true
            };

            var quizAnswerResult = new ServiceResult<QuizAnswerDto>
            {
                Success = true,
                Data = quizAnswer
            };

            var request = new SubmitAnswerRequest
            {
                QuestionId = questionId,
                AnswerId = answerId
            };

            _mockQuizTryService.Setup(s => s.GetQuizTryDetailsAsync(attemptId))
                .Returns(Task.FromResult(quizTryResult));

            _mockQuizAnswerService.Setup(s => s.CreateQuizAnswerAsync(
                attemptId,
                questionId,
                answerId
            )).Returns(Task.FromResult(quizAnswerResult));

            // Act
            var result = await _quizTryController.SubmitAnswer(quizId, attemptId, request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(QuizTryController.GetQuizTryAnswers), createdResult.ActionName);
            Assert.Equal(quizId, createdResult.RouteValues["quizId"]);
            Assert.Equal(attemptId, createdResult.RouteValues["attemptId"]);
            var response = Assert.IsType<QuizAnswerDto>(createdResult.Value);
            Assert.Equal(questionId, response.QuestionId);
            Assert.Equal(answerId, response.AnswerId);
        }

        [Fact]
        public async Task GetQuizTryAnswers_ValidAttempt_ReturnsOkResult()
        {
            // Arrange
            var userId = 1;
            var quizId = 1;
            var attemptId = 1;
            SetupAuthenticatedUser(userId);

            var quizTryDetails = new QuizTryDetailsDto
            {
                Id = attemptId,
                QuizId = quizId,
                UserId = userId,
                QuizName = "Test Quiz"
            };

            var quizTryResult = new ServiceResult<QuizTryDetailsDto>
            {
                Success = true,
                Data = quizTryDetails
            };

            var quizAnswers = new List<QuizAnswerDto>
            {
                new QuizAnswerDto
                {
                    Id = 1,
                    QuizTryId = attemptId,
                    QuestionId = 1,
                    AnswerId = 1,
                    QuestionText = "Question 1",
                    AnswerText = "Answer 1",
                    IsCorrect = true
                },
                new QuizAnswerDto
                {
                    Id = 2,
                    QuizTryId = attemptId,
                    QuestionId = 2,
                    AnswerId = 2,
                    QuestionText = "Question 2",
                    AnswerText = "Answer 2",
                    IsCorrect = false
                }
            };

            var expectedResponse = new ServiceResult<IEnumerable<QuizAnswerDto>>
            {
                Success = true,
                Data = quizAnswers
            };

            _mockQuizTryService.Setup(s => s.GetQuizTryDetailsAsync(attemptId))
                .Returns(Task.FromResult(quizTryResult));

            _mockQuizAnswerService.Setup(s => s.GetQuizAnswersByQuizTryAsync(attemptId))
                .Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _quizTryController.GetQuizTryAnswers(quizId, attemptId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<List<QuizAnswerDto>>(okResult.Value);
            Assert.Equal(2, response.Count);
            Assert.Equal("Question 1", response[0].QuestionText);
            Assert.Equal("Question 2", response[1].QuestionText);
        }
    }
} 