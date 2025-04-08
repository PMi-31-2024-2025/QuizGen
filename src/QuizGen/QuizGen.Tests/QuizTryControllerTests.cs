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
                AuthorId = 2, // Different from current user
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
                StartedAt = DateTime.UtcNow
            };

            var tryResult = new ServiceResult<QuizTryDto>
            {
                Success = true,
                Data = quizTry
            };

            _mockQuizService.Setup(s => s.GetQuizByIdAsync(quizId))
                .Returns(Task.FromResult(quizResult));

            _mockQuizTryService.Setup(s => s.StartQuizTryAsync(quizId, userId))
                .Returns(Task.FromResult(tryResult));

            // Act
            var result = await _quizTryController.StartQuizTry(quizId);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(QuizTryController.GetQuizTryDetails), createdResult.ActionName);
            Assert.Equal(quizId, createdResult.RouteValues["quizId"]);
            Assert.Equal(quizTry.Id, createdResult.RouteValues["attemptId"]);
            Assert.Equal(quizTry, createdResult.Value);
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
                AuthorId = userId, // Same as current user
                Name = "Test Quiz",
                AuthorName = "Test User"
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
                new QuizTryDto
                {
                    Id = 1,
                    QuizId = 1,
                    UserId = userId,
                    StartedAt = DateTime.UtcNow
                }
            };

            var result = new ServiceResult<IEnumerable<QuizTryDto>>
            {
                Success = true,
                Data = quizTries
            };

            _mockQuizTryService.Setup(s => s.GetQuizTriesByUserAsync(userId))
                .Returns(Task.FromResult(result));

            // Act
            var actionResult = await _quizTryController.GetMyQuizTries();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            var returnedTries = Assert.IsAssignableFrom<IEnumerable<QuizTryDto>>(okResult.Value);
            Assert.Single(returnedTries);
            Assert.Equal(quizTries[0].Id, returnedTries.First().Id);
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
                new QuizTryDto
                {
                    Id = 1,
                    QuizId = quizId,
                    UserId = userId,
                    StartedAt = DateTime.UtcNow
                }
            };

            var triesResult = new ServiceResult<IEnumerable<QuizTryDto>>
            {
                Success = true,
                Data = quizTries
            };

            _mockQuizService.Setup(s => s.GetQuizByIdAsync(quizId))
                .Returns(Task.FromResult(quizResult));

            _mockQuizTryService.Setup(s => s.GetQuizTriesByQuizAsync(quizId))
                .Returns(Task.FromResult(triesResult));

            // Act
            var result = await _quizTryController.GetQuizAttempts(quizId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedTries = Assert.IsAssignableFrom<List<QuizTryDto>>(okResult.Value);
            Assert.Single(returnedTries);
            Assert.Equal(quizTries[0].Id, returnedTries[0].Id);
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
                CurrentQuestionIndex = 0,
                TotalQuestions = 10,
                Questions = new List<QuizTryQuestionDto>()
            };

            var result = new ServiceResult<QuizTryDetailsDto>
            {
                Success = true,
                Data = quizTryDetails
            };

            _mockQuizTryService.Setup(s => s.GetQuizTryDetailsAsync(attemptId))
                .Returns(Task.FromResult(result));

            // Act
            var actionResult = await _quizTryController.GetQuizTryDetails(quizId, attemptId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            var returnedDetails = Assert.IsType<QuizTryDetailsDto>(okResult.Value);
            Assert.Equal(quizTryDetails.Id, returnedDetails.Id);
            Assert.Equal(quizTryDetails.QuizId, returnedDetails.QuizId);
            Assert.Equal(quizTryDetails.UserId, returnedDetails.UserId);
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

            var attemptResult = new ServiceResult<QuizTryDetailsDto>
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
                IsCorrect = true,
                CreatedAt = DateTime.UtcNow
            };

            var answerResult = new ServiceResult<QuizAnswerDto>
            {
                Success = true,
                Data = quizAnswer
            };

            _mockQuizTryService.Setup(s => s.GetQuizTryDetailsAsync(attemptId))
                .Returns(Task.FromResult(attemptResult));

            _mockQuizAnswerService.Setup(s => s.CreateQuizAnswerAsync(attemptId, questionId, answerId))
                .Returns(Task.FromResult(answerResult));

            var request = new SubmitAnswerRequest
            {
                QuestionId = questionId,
                AnswerId = answerId
            };

            // Act
            var result = await _quizTryController.SubmitAnswer(quizId, attemptId, request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(QuizTryController.GetQuizTryAnswers), createdResult.ActionName);
            Assert.Equal(quizId, createdResult.RouteValues["quizId"]);
            Assert.Equal(attemptId, createdResult.RouteValues["attemptId"]);
            Assert.Equal(quizAnswer, createdResult.Value);
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

            var attemptResult = new ServiceResult<QuizTryDetailsDto>
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
                    QuestionText = "Test Question",
                    AnswerText = "Test Answer",
                    IsCorrect = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            var answersResult = new ServiceResult<IEnumerable<QuizAnswerDto>>
            {
                Success = true,
                Data = quizAnswers
            };

            _mockQuizTryService.Setup(s => s.GetQuizTryDetailsAsync(attemptId))
                .Returns(Task.FromResult(attemptResult));

            _mockQuizAnswerService.Setup(s => s.GetQuizAnswersByQuizTryAsync(attemptId))
                .Returns(Task.FromResult(answersResult));

            // Act
            var result = await _quizTryController.GetQuizTryAnswers(quizId, attemptId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedAnswers = Assert.IsAssignableFrom<IEnumerable<QuizAnswerDto>>(okResult.Value);
            Assert.Single(returnedAnswers);
            Assert.Equal(quizAnswers[0].Id, returnedAnswers.First().Id);
        }

        [Fact]
        public async Task StartQuizTry_QuizNotFound_ReturnsNotFound()
        {
            // Arrange
            var userId = 1;
            var quizId = 1;
            SetupAuthenticatedUser(userId);

            var quizResult = new ServiceResult<QuizDto>
            {
                Success = false,
                Message = "Quiz not found"
            };

            _mockQuizService.Setup(s => s.GetQuizByIdAsync(quizId))
                .Returns(Task.FromResult(quizResult));

            // Act
            var result = await _quizTryController.StartQuizTry(quizId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Quiz not found", notFoundResult.Value);
        }

        [Fact]
        public async Task StartQuizTry_ServiceThrowsException_ReturnsBadRequest()
        {
            // Arrange
            var userId = 1;
            var quizId = 1;
            SetupAuthenticatedUser(userId);

            var quiz = new QuizDto
            {
                Id = quizId,
                AuthorId = 2, // Different from current user
                Name = "Test Quiz",
                AuthorName = "Test User"
            };

            var quizResult = new ServiceResult<QuizDto>
            {
                Success = true,
                Data = quiz
            };

            _mockQuizService.Setup(s => s.GetQuizByIdAsync(quizId))
                .Returns(Task.FromResult(quizResult));

            _mockQuizTryService.Setup(s => s.StartQuizTryAsync(quizId, userId))
                .Throws(new Exception("Failed to start quiz try"));

            // Act
            var result = await _quizTryController.StartQuizTry(quizId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Failed to start quiz try", badRequestResult.Value);
        }

        [Fact]
        public async Task GetQuizTryDetails_AttemptNotFound_ReturnsNotFound()
        {
            // Arrange
            var userId = 1;
            var quizId = 1;
            var attemptId = 1;
            SetupAuthenticatedUser(userId);

            var result = new ServiceResult<QuizTryDetailsDto>
            {
                Success = false,
                Message = "Quiz attempt not found"
            };

            _mockQuizTryService.Setup(s => s.GetQuizTryDetailsAsync(attemptId))
                .Returns(Task.FromResult(result));

            // Act
            var actionResult = await _quizTryController.GetQuizTryDetails(quizId, attemptId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult);
            Assert.Equal("Quiz attempt not found", notFoundResult.Value);
        }

        [Fact]
        public async Task SubmitAnswer_AttemptNotFound_ReturnsNotFound()
        {
            // Arrange
            var userId = 1;
            var quizId = 1;
            var attemptId = 1;
            var questionId = 1;
            var answerId = 1;
            SetupAuthenticatedUser(userId);

            var attemptResult = new ServiceResult<QuizTryDetailsDto>
            {
                Success = false,
                Message = "Quiz attempt not found"
            };

            _mockQuizTryService.Setup(s => s.GetQuizTryDetailsAsync(attemptId))
                .Returns(Task.FromResult(attemptResult));

            var request = new SubmitAnswerRequest
            {
                QuestionId = questionId,
                AnswerId = answerId
            };

            // Act
            var result = await _quizTryController.SubmitAnswer(quizId, attemptId, request);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Quiz attempt not found", notFoundResult.Value);
        }

        [Fact]
        public async Task SubmitAnswer_InvalidRequest_ReturnsBadRequest()
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

            var attemptResult = new ServiceResult<QuizTryDetailsDto>
            {
                Success = true,
                Data = quizTryDetails
            };

            _mockQuizTryService.Setup(s => s.GetQuizTryDetailsAsync(attemptId))
                .Returns(Task.FromResult(attemptResult));

            var request = new SubmitAnswerRequest
            {
                QuestionId = 0, // Invalid question ID
                AnswerId = 0    // Invalid answer ID
            };

            // Act
            var result = await _quizTryController.SubmitAnswer(quizId, attemptId, request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Invalid question ID", badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task GetQuizTryAnswers_AttemptNotFound_ReturnsNotFound()
        {
            // Arrange
            var userId = 1;
            var quizId = 1;
            var attemptId = 1;
            SetupAuthenticatedUser(userId);

            var attemptResult = new ServiceResult<QuizTryDetailsDto>
            {
                Success = false,
                Message = "Quiz attempt not found"
            };

            _mockQuizTryService.Setup(s => s.GetQuizTryDetailsAsync(attemptId))
                .Returns(Task.FromResult(attemptResult));

            // Act
            var result = await _quizTryController.GetQuizTryAnswers(quizId, attemptId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Quiz attempt not found", notFoundResult.Value);
        }

        [Fact]
        public async Task GetQuizTryAnswers_NoAnswers_ReturnsEmptyList()
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

            var attemptResult = new ServiceResult<QuizTryDetailsDto>
            {
                Success = true,
                Data = quizTryDetails
            };

            var answersResult = new ServiceResult<IEnumerable<QuizAnswerDto>>
            {
                Success = true,
                Data = new List<QuizAnswerDto>()
            };

            _mockQuizTryService.Setup(s => s.GetQuizTryDetailsAsync(attemptId))
                .Returns(Task.FromResult(attemptResult));

            _mockQuizAnswerService.Setup(s => s.GetQuizAnswersByQuizTryAsync(attemptId))
                .Returns(Task.FromResult(answersResult));

            // Act
            var result = await _quizTryController.GetQuizTryAnswers(quizId, attemptId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedAnswers = Assert.IsAssignableFrom<IEnumerable<QuizAnswerDto>>(okResult.Value);
            Assert.Empty(returnedAnswers);
        }

        [Fact]
        public async Task GetQuizAttempts_NotAuthorized_ReturnsForbid()
        {
            // Arrange
            var userId = 1;
            var quizId = 1;
            SetupAuthenticatedUser(userId);

            var quiz = new QuizDto
            {
                Id = quizId,
                AuthorId = 2,
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
            var result = await _quizTryController.GetQuizAttempts(quizId);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task GetQuizTryDetails_NotAuthorized_ReturnsForbid()
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
                UserId = 2,
                QuizName = "Test Quiz"
            };

            var result = new ServiceResult<QuizTryDetailsDto>
            {
                Success = true,
                Data = quizTryDetails
            };

            _mockQuizTryService.Setup(s => s.GetQuizTryDetailsAsync(attemptId))
                .Returns(Task.FromResult(result));

            // Act
            var actionResult = await _quizTryController.GetQuizTryDetails(quizId, attemptId);

            // Assert
            Assert.IsType<ForbidResult>(actionResult);
        }

        [Fact]
        public async Task SubmitAnswer_NotAuthorized_ReturnsForbid()
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
                UserId = 2,
                QuizName = "Test Quiz"
            };

            var attemptResult = new ServiceResult<QuizTryDetailsDto>
            {
                Success = true,
                Data = quizTryDetails
            };

            _mockQuizTryService.Setup(s => s.GetQuizTryDetailsAsync(attemptId))
                .Returns(Task.FromResult(attemptResult));

            var request = new SubmitAnswerRequest
            {
                QuestionId = questionId,
                AnswerId = answerId
            };

            // Act
            var result = await _quizTryController.SubmitAnswer(quizId, attemptId, request);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task GetQuizTryAnswers_NotAuthorized_ReturnsForbid()
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
                UserId = 2,
                QuizName = "Test Quiz"
            };

            var attemptResult = new ServiceResult<QuizTryDetailsDto>
            {
                Success = true,
                Data = quizTryDetails
            };

            _mockQuizTryService.Setup(s => s.GetQuizTryDetailsAsync(attemptId))
                .Returns(Task.FromResult(attemptResult));

            // Act
            var result = await _quizTryController.GetQuizTryAnswers(quizId, attemptId);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task GetQuizAttempts_QuizNotFound_ReturnsNotFound()
        {
            // Arrange
            var userId = 1;
            var quizId = 1;
            SetupAuthenticatedUser(userId);

            var quizResult = new ServiceResult<QuizDto>
            {
                Success = false,
                Message = "Quiz not found"
            };

            _mockQuizService.Setup(s => s.GetQuizByIdAsync(quizId))
                .Returns(Task.FromResult(quizResult));

            // Act
            var result = await _quizTryController.GetQuizAttempts(quizId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Quiz not found", notFoundResult.Value);
        }

        [Fact]
        public async Task SubmitAnswer_ServiceThrowsException_ReturnsBadRequest()
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

            var attemptResult = new ServiceResult<QuizTryDetailsDto>
            {
                Success = true,
                Data = quizTryDetails
            };

            _mockQuizTryService.Setup(s => s.GetQuizTryDetailsAsync(attemptId))
                .Returns(Task.FromResult(attemptResult));

            _mockQuizAnswerService.Setup(s => s.CreateQuizAnswerAsync(attemptId, questionId, answerId))
                .Throws(new Exception("Failed to submit answer"));

            var request = new SubmitAnswerRequest
            {
                QuestionId = questionId,
                AnswerId = answerId
            };

            // Act
            var result = await _quizTryController.SubmitAnswer(quizId, attemptId, request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Failed to submit answer", badRequestResult.Value);
        }

        [Fact]
        public async Task GetQuizTryAnswers_ServiceThrowsException_ReturnsBadRequest()
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

            var attemptResult = new ServiceResult<QuizTryDetailsDto>
            {
                Success = true,
                Data = quizTryDetails
            };

            _mockQuizTryService.Setup(s => s.GetQuizTryDetailsAsync(attemptId))
                .Returns(Task.FromResult(attemptResult));

            _mockQuizAnswerService.Setup(s => s.GetQuizAnswersByQuizTryAsync(attemptId))
                .Throws(new Exception("Failed to get answers"));

            // Act
            var result = await _quizTryController.GetQuizTryAnswers(quizId, attemptId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Failed to get answers", badRequestResult.Value);
        }
    }
} 