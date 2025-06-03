using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using QuizGen.API.Controllers;
using QuizGen.BLL.Models.Quiz;
using QuizGen.BLL.Models.Base;
using QuizGen.BLL.Services.Interfaces;
using System.Security.Claims;

namespace QuizGen.Tests
{
    public class QuizControllerTests
    {
        private readonly Mock<IQuizService> _mockQuizService;
        private readonly QuizController _quizController;

        public QuizControllerTests()
        {
            _mockQuizService = new Mock<IQuizService>();
            _quizController = new QuizController(_mockQuizService.Object);
        }

        private void SetupAuthenticatedUser(int userId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "test");
            var principal = new ClaimsPrincipal(identity);
            _quizController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        [Fact]
        public async Task CreateQuiz_ValidRequest_ReturnsCreatedResult()
        {
            // Arrange
            var userId = 1;
            SetupAuthenticatedUser(userId);

            var request = new CreateQuizRequest
            {
                Topic = "Test Topic",
                Difficulty = "Easy",
                NumQuestions = 5,
                AllowedTypes = new[] { "MultipleChoice", "TrueFalse" }
            };

            var expectedQuiz = new QuizDto
            {
                Id = 1,
                Name = "Test Quiz",
                Prompt = request.Topic,
                Difficulty = request.Difficulty,
                AuthorId = userId,
                AuthorName = "Test User",
                NumQuestions = request.NumQuestions,
                AllowedTypes = request.AllowedTypes,
                CreatedAt = DateTime.UtcNow
            };

            var expectedResponse = new ServiceResult<QuizDto>
            {
                Success = true,
                Data = expectedQuiz
            };

            _mockQuizService.Setup(s => s.CreateQuizAsync(
                userId,
                request.Topic,
                request.Difficulty,
                request.NumQuestions,
                request.AllowedTypes
            )).Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _quizController.CreateQuiz(request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(QuizController.GetQuiz), createdResult.ActionName);
            Assert.Equal(expectedQuiz.Id, createdResult.RouteValues["id"]);
            var response = Assert.IsType<QuizDto>(createdResult.Value);
            Assert.Equal(expectedQuiz.Name, response.Name);
            Assert.Equal(expectedQuiz.Prompt, response.Prompt);
            Assert.Equal(expectedQuiz.Difficulty, response.Difficulty);
            Assert.Equal(userId, response.AuthorId);
        }

        [Fact]
        public async Task CreateQuiz_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var userId = 1;
            SetupAuthenticatedUser(userId);

            var request = new CreateQuizRequest
            {
                Topic = "Test Topic",
                Difficulty = "Invalid",
                NumQuestions = 0,
                AllowedTypes = Array.Empty<string>()
            };

            var expectedResponse = new ServiceResult<QuizDto>
            {
                Success = false,
                Message = "Invalid difficulty level"
            };

            _mockQuizService.Setup(s => s.CreateQuizAsync(
                userId,
                request.Topic,
                request.Difficulty,
                request.NumQuestions,
                request.AllowedTypes
            )).Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _quizController.CreateQuiz(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid difficulty level", badRequestResult.Value);
        }

        [Fact]
        public async Task GetMyQuizzes_AuthenticatedUser_ReturnsOkResult()
        {
            // Arrange
            var userId = 1;
            SetupAuthenticatedUser(userId);

            var expectedQuizzes = new List<QuizDto>
            {
                new QuizDto { Id = 1, Name = "Quiz 1", AuthorId = userId, AuthorName = "Test User" },
                new QuizDto { Id = 2, Name = "Quiz 2", AuthorId = userId, AuthorName = "Test User" }
            };

            var expectedResponse = new ServiceResult<IEnumerable<QuizDto>>
            {
                Success = true,
                Data = expectedQuizzes
            };

            _mockQuizService.Setup(s => s.GetQuizzesByAuthorAsync(userId))
                .Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _quizController.GetMyQuizzes();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<List<QuizDto>>(okResult.Value);
            Assert.Equal(2, response.Count);
            Assert.Equal("Quiz 1", response[0].Name);
            Assert.Equal("Quiz 2", response[1].Name);
        }

        [Fact]
        public async Task GetQuiz_ValidId_ReturnsOkResult()
        {
            // Arrange
            var userId = 1;
            var quizId = 1;
            SetupAuthenticatedUser(userId);

            var expectedQuiz = new QuizDto
            {
                Id = quizId,
                Name = "Test Quiz",
                AuthorId = userId,
                AuthorName = "Test User"
            };

            var expectedResponse = new ServiceResult<QuizDto>
            {
                Success = true,
                Data = expectedQuiz
            };

            _mockQuizService.Setup(s => s.GetQuizByIdAsync(quizId))
                .Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _quizController.GetQuiz(quizId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<QuizDto>(okResult.Value);
            Assert.Equal(quizId, response.Id);
            Assert.Equal("Test Quiz", response.Name);
        }

        [Fact]
        public async Task GetQuiz_NotAuthorized_ReturnsForbid()
        {
            // Arrange
            var userId = 1;
            var quizId = 1;
            SetupAuthenticatedUser(userId);

            var expectedQuiz = new QuizDto
            {
                Id = quizId,
                Name = "Test Quiz",
                AuthorId = 2, // Different author
                AuthorName = "Other User"
            };

            var expectedResponse = new ServiceResult<QuizDto>
            {
                Success = true,
                Data = expectedQuiz
            };

            _mockQuizService.Setup(s => s.GetQuizByIdAsync(quizId))
                .Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _quizController.GetQuiz(quizId);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task GetQuizzesByDifficulty_ValidDifficulty_ReturnsOkResult()
        {
            // Arrange
            var userId = 1;
            var difficulty = "Easy";
            SetupAuthenticatedUser(userId);

            var allQuizzes = new List<QuizDto>
            {
                new QuizDto { Id = 1, Name = "Quiz 1", Difficulty = difficulty, AuthorId = userId, AuthorName = "Test User" },
                new QuizDto { Id = 2, Name = "Quiz 2", Difficulty = difficulty, AuthorId = 2, AuthorName = "Other User" },
                new QuizDto { Id = 3, Name = "Quiz 3", Difficulty = difficulty, AuthorId = userId, AuthorName = "Test User" }
            };

            var expectedResponse = new ServiceResult<IEnumerable<QuizDto>>
            {
                Success = true,
                Data = allQuizzes
            };

            _mockQuizService.Setup(s => s.GetQuizzesByDifficultyAsync(difficulty))
                .Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _quizController.GetQuizzesByDifficulty(difficulty);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<List<QuizDto>>(okResult.Value);
            Assert.Equal(2, response.Count); // Only user's quizzes
            Assert.All(response, q => Assert.Equal(userId, q.AuthorId));
        }

        [Fact]
        public async Task GetQuizzesByDifficulty_InvalidResponse_ReturnsBadRequest()
        {
            var userId = 1;
            var difficulty = "another difficulty";
            SetupAuthenticatedUser(userId);

            var expectedResponse = new ServiceResult<IEnumerable<QuizDto>>
            {
                Success = false,
                Message = "Invalid difficulty level provided"
            };

            _mockQuizService.Setup(s => s.GetQuizzesByDifficultyAsync(difficulty))
                .Returns(Task.FromResult(expectedResponse));

            
            var result = await _quizController.GetQuizzesByDifficulty(difficulty);


            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid difficulty level provided", badRequestResult.Value);
        }

        [Fact]
        public async Task DeleteQuiz_ValidId_ReturnsNoContent()
        {
            // Arrange
            var userId = 1;
            var quizId = 1;
            SetupAuthenticatedUser(userId);

            var getQuizResponse = new ServiceResult<QuizDto>
            {
                Success = true,
                Data = new QuizDto { Id = quizId, AuthorId = userId, AuthorName = "Test User" }
            };

            var deleteResponse = new ServiceResult<bool>
            {
                Success = true
            };

            _mockQuizService.Setup(s => s.GetQuizByIdAsync(quizId))
                .Returns(Task.FromResult(getQuizResponse));

            _mockQuizService.Setup(s => s.DeleteQuizAsync(quizId))
                .Returns(Task.FromResult(deleteResponse));

            // Act
            var result = await _quizController.DeleteQuiz(quizId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteQuiz_NotAuthorized_ReturnsForbid()
        {
            // Arrange
            var userId = 1;
            var quizId = 1;
            SetupAuthenticatedUser(userId);

            var getQuizResponse = new ServiceResult<QuizDto>
            {
                Success = true,
                Data = new QuizDto { Id = quizId, AuthorId = 2, AuthorName = "Other User" } // Different author
            };

            _mockQuizService.Setup(s => s.GetQuizByIdAsync(quizId))
                .Returns(Task.FromResult(getQuizResponse));

            // Act
            var result = await _quizController.DeleteQuiz(quizId);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task DeleteQuiz_QuizNotFound_ReturnsNotFound()
        {
       
            var userId = 1;
            var quizId = 1;
            SetupAuthenticatedUser(userId);

            var getQuizResponse = new ServiceResult<QuizDto>
            {
                Success = false,
                Message = "Quiz not found"
            };

            _mockQuizService.Setup(s => s.GetQuizByIdAsync(quizId))
                .Returns(Task.FromResult(getQuizResponse));

        
            var result = await _quizController.DeleteQuiz(quizId);

          
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Quiz not found", notFoundResult.Value);
        }
    }
} 