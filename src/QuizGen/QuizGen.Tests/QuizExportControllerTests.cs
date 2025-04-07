using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using QuizGen.API.Controllers;
using QuizGen.BLL.Models.Quiz;
using QuizGen.BLL.Models.QuizTry;
using QuizGen.BLL.Models.QuizAnswer;
using QuizGen.BLL.Models.Question;
using QuizGen.BLL.Models.Base;
using QuizGen.BLL.Services.Interfaces;
using System.Security.Claims;

namespace QuizGen.Tests
{
    public class QuizExportControllerTests
    {
        private readonly Mock<IQuizExportService> _mockQuizExportService;
        private readonly Mock<IQuizTryService> _mockQuizTryService;
        private readonly Mock<IQuizService> _mockQuizService;
        private readonly QuizExportController _quizExportController;

        public QuizExportControllerTests()
        {
            _mockQuizExportService = new Mock<IQuizExportService>();
            _mockQuizTryService = new Mock<IQuizTryService>();
            _mockQuizService = new Mock<IQuizService>();
            _quizExportController = new QuizExportController(
                _mockQuizExportService.Object,
                _mockQuizTryService.Object,
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
            _quizExportController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        [Fact]
        public async Task ExportQuizAsPdf_AuthorizedUser_ReturnsFileResult()
        {
            // Arrange
            var userId = 1;
            var quizId = 1;
            var includeAnswers = true;
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

            var pdfBytes = new byte[] { 1, 2, 3, 4, 5 };
            var exportResult = new ServiceResult<byte[]>
            {
                Success = true,
                Data = pdfBytes
            };

            _mockQuizService.Setup(s => s.GetQuizByIdAsync(quizId))
                .Returns(Task.FromResult(quizResult));

            _mockQuizExportService.Setup(s => s.ExportAsPdfAsync(quizId, includeAnswers))
                .Returns(Task.FromResult(exportResult));

            // Act
            var result = await _quizExportController.ExportQuizAsPdf(quizId, includeAnswers);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/pdf", fileResult.ContentType);
            Assert.Equal($"quiz-{quizId}.pdf", fileResult.FileDownloadName);
            Assert.Equal(pdfBytes, fileResult.FileContents);
        }

        [Fact]
        public async Task ExportQuizAsPdf_NotAuthorized_ReturnsForbid()
        {
            // Arrange
            var userId = 1;
            var quizId = 1;
            var includeAnswers = true;
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

            var userTriesResult = new ServiceResult<IEnumerable<QuizTryDto>>
            {
                Success = true,
                Data = new List<QuizTryDto>() // No attempts
            };

            _mockQuizService.Setup(s => s.GetQuizByIdAsync(quizId))
                .Returns(Task.FromResult(quizResult));

            _mockQuizTryService.Setup(s => s.GetQuizTriesByQuizAsync(quizId))
                .Returns(Task.FromResult(userTriesResult));

            // Act
            var result = await _quizExportController.ExportQuizAsPdf(quizId, includeAnswers);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task ExportQuizAsText_AuthorizedUser_ReturnsFileResult()
        {
            // Arrange
            var userId = 1;
            var quizId = 1;
            var includeAnswers = true;
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

            var textBytes = System.Text.Encoding.UTF8.GetBytes("Test Quiz Content");
            var exportResult = new ServiceResult<byte[]>
            {
                Success = true,
                Data = textBytes
            };

            _mockQuizService.Setup(s => s.GetQuizByIdAsync(quizId))
                .Returns(Task.FromResult(quizResult));

            _mockQuizExportService.Setup(s => s.ExportAsTextAsync(quizId, includeAnswers))
                .Returns(Task.FromResult(exportResult));

            // Act
            var result = await _quizExportController.ExportQuizAsText(quizId, includeAnswers);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("text/plain", fileResult.ContentType);
            Assert.Equal($"quiz-{quizId}.txt", fileResult.FileDownloadName);
            Assert.Equal(textBytes, fileResult.FileContents);
        }

        [Fact]
        public async Task ExportQuizTryAsPdf_AuthorizedUser_ReturnsFileResult()
        {
            // Arrange
            var userId = 1;
            var quizId = 1;
            var attemptId = 1;
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

            var pdfBytes = new byte[] { 1, 2, 3, 4, 5 };
            var exportResult = new ServiceResult<byte[]>
            {
                Success = true,
                Data = pdfBytes
            };

            _mockQuizService.Setup(s => s.GetQuizByIdAsync(quizId))
                .Returns(Task.FromResult(quizResult));

            _mockQuizTryService.Setup(s => s.GetQuizTryDetailsAsync(attemptId))
                .Returns(Task.FromResult(attemptResult));

            _mockQuizExportService.Setup(s => s.ExportTryAsPdfAsync(attemptId))
                .Returns(Task.FromResult(exportResult));

            // Act
            var result = await _quizExportController.ExportQuizTryAsPdf(quizId, attemptId);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/pdf", fileResult.ContentType);
            Assert.Equal($"quiz-{quizId}-attempt-{attemptId}.pdf", fileResult.FileDownloadName);
            Assert.Equal(pdfBytes, fileResult.FileContents);
        }

        [Fact]
        public async Task ExportQuizTryAsText_AuthorizedUser_ReturnsFileResult()
        {
            // Arrange
            var userId = 1;
            var quizId = 1;
            var attemptId = 1;
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

            var textBytes = System.Text.Encoding.UTF8.GetBytes("Test Quiz Attempt Content");
            var exportResult = new ServiceResult<byte[]>
            {
                Success = true,
                Data = textBytes
            };

            _mockQuizService.Setup(s => s.GetQuizByIdAsync(quizId))
                .Returns(Task.FromResult(quizResult));

            _mockQuizTryService.Setup(s => s.GetQuizTryDetailsAsync(attemptId))
                .Returns(Task.FromResult(attemptResult));

            _mockQuizExportService.Setup(s => s.ExportTryAsTextAsync(attemptId))
                .Returns(Task.FromResult(exportResult));

            // Act
            var result = await _quizExportController.ExportQuizTryAsText(quizId, attemptId);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("text/plain", fileResult.ContentType);
            Assert.Equal($"quiz-{quizId}-attempt-{attemptId}.txt", fileResult.FileDownloadName);
            Assert.Equal(textBytes, fileResult.FileContents);
        }

        [Fact]
        public async Task ExportQuizTryAsPdf_NotAuthorized_ReturnsForbid()
        {
            // Arrange
            var userId = 1;
            var quizId = 1;
            var attemptId = 1;
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

            var quizTryDetails = new QuizTryDetailsDto
            {
                Id = attemptId,
                QuizId = quizId,
                UserId = 3, // Different user
                QuizName = "Test Quiz"
            };

            var attemptResult = new ServiceResult<QuizTryDetailsDto>
            {
                Success = true,
                Data = quizTryDetails
            };

            _mockQuizService.Setup(s => s.GetQuizByIdAsync(quizId))
                .Returns(Task.FromResult(quizResult));

            _mockQuizTryService.Setup(s => s.GetQuizTryDetailsAsync(attemptId))
                .Returns(Task.FromResult(attemptResult));

            // Act
            var result = await _quizExportController.ExportQuizTryAsPdf(quizId, attemptId);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task ExportQuizAsPdf_QuizNotFound_ReturnsNotFound()
        {
            // Arrange
            var userId = 1;
            var quizId = 999;
            var includeAnswers = true;
            SetupAuthenticatedUser(userId);

            var quizResult = new ServiceResult<QuizDto>
            {
                Success = false,
                Message = "Quiz not found"
            };

            _mockQuizService.Setup(s => s.GetQuizByIdAsync(quizId))
                .Returns(Task.FromResult(quizResult));

            // Act
            var result = await _quizExportController.ExportQuizAsPdf(quizId, includeAnswers);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task ExportQuizAsPdf_ServiceThrowsException_ReturnsBadRequest()
        {
            // Arrange
            var userId = 1;
            var quizId = 1;
            var includeAnswers = true;
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

            _mockQuizService.Setup(s => s.GetQuizByIdAsync(quizId))
                .Returns(Task.FromResult(quizResult));

            _mockQuizExportService.Setup(s => s.ExportAsPdfAsync(quizId, includeAnswers))
                .Throws(new Exception("Export failed"));

            // Act
            var result = await _quizExportController.ExportQuizAsPdf(quizId, includeAnswers);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Failed to export quiz: Export failed", badRequestResult.Value);
        }

        [Fact]
        public async Task ExportQuizAsPdf_EmptyQuiz_ReturnsFileResult()
        {
            // Arrange
            var userId = 1;
            var quizId = 1;
            var includeAnswers = true;
            SetupAuthenticatedUser(userId);

            var quiz = new QuizDto
            {
                Id = quizId,
                AuthorId = userId,
                Name = "Empty Quiz",
                AuthorName = "Test User"
            };

            var quizResult = new ServiceResult<QuizDto>
            {
                Success = true,
                Data = quiz
            };

            var pdfBytes = new byte[] { 1, 2, 3, 4, 5 };
            var exportResult = new ServiceResult<byte[]>
            {
                Success = true,
                Data = pdfBytes
            };

            _mockQuizService.Setup(s => s.GetQuizByIdAsync(quizId))
                .Returns(Task.FromResult(quizResult));

            _mockQuizExportService.Setup(s => s.ExportAsPdfAsync(quizId, includeAnswers))
                .Returns(Task.FromResult(exportResult));

            // Act
            var result = await _quizExportController.ExportQuizAsPdf(quizId, includeAnswers);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/pdf", fileResult.ContentType);
            Assert.Equal($"quiz-{quizId}.pdf", fileResult.FileDownloadName);
            Assert.Equal(pdfBytes, fileResult.FileContents);
        }

        [Fact]
        public async Task ExportQuizAsPdf_SpecialCharactersInName_ReturnsFileResult()
        {
            // Arrange
            var userId = 1;
            var quizId = 1;
            var includeAnswers = true;
            SetupAuthenticatedUser(userId);

            var quiz = new QuizDto
            {
                Id = quizId,
                AuthorId = userId,
                Name = "Test Quiz with special chars: !@#$%^&*()",
                AuthorName = "Test User"
            };

            var quizResult = new ServiceResult<QuizDto>
            {
                Success = true,
                Data = quiz
            };

            var pdfBytes = new byte[] { 1, 2, 3, 4, 5 };
            var exportResult = new ServiceResult<byte[]>
            {
                Success = true,
                Data = pdfBytes
            };

            _mockQuizService.Setup(s => s.GetQuizByIdAsync(quizId))
                .Returns(Task.FromResult(quizResult));

            _mockQuizExportService.Setup(s => s.ExportAsPdfAsync(quizId, includeAnswers))
                .Returns(Task.FromResult(exportResult));

            // Act
            var result = await _quizExportController.ExportQuizAsPdf(quizId, includeAnswers);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/pdf", fileResult.ContentType);
            Assert.Equal($"quiz-{quizId}.pdf", fileResult.FileDownloadName);
            Assert.Equal(pdfBytes, fileResult.FileContents);
        }

        [Fact]
        public async Task ExportQuizTryAsPdf_AttemptNotFound_ReturnsNotFound()
        {
            // Arrange
            var userId = 1;
            var quizId = 1;
            var attemptId = 999;
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

            var attemptResult = new ServiceResult<QuizTryDetailsDto>
            {
                Success = false,
                Message = "Attempt not found"
            };

            _mockQuizService.Setup(s => s.GetQuizByIdAsync(quizId))
                .Returns(Task.FromResult(quizResult));

            _mockQuizTryService.Setup(s => s.GetQuizTryDetailsAsync(attemptId))
                .Returns(Task.FromResult(attemptResult));

            // Act
            var result = await _quizExportController.ExportQuizTryAsPdf(quizId, attemptId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task ExportQuizTryAsPdf_IncompleteAttempt_ReturnsFileResult()
        {
            // Arrange
            var userId = 1;
            var quizId = 1;
            var attemptId = 1;
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

            var attemptResult = new ServiceResult<QuizTryDetailsDto>
            {
                Success = true,
                Data = quizTryDetails
            };

            var pdfBytes = new byte[] { 1, 2, 3, 4, 5 };
            var exportResult = new ServiceResult<byte[]>
            {
                Success = true,
                Data = pdfBytes
            };

            _mockQuizService.Setup(s => s.GetQuizByIdAsync(quizId))
                .Returns(Task.FromResult(quizResult));

            _mockQuizTryService.Setup(s => s.GetQuizTryDetailsAsync(attemptId))
                .Returns(Task.FromResult(attemptResult));

            _mockQuizExportService.Setup(s => s.ExportTryAsPdfAsync(attemptId))
                .Returns(Task.FromResult(exportResult));

            // Act
            var result = await _quizExportController.ExportQuizTryAsPdf(quizId, attemptId);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/pdf", fileResult.ContentType);
            Assert.Equal($"quiz-{quizId}-attempt-{attemptId}.pdf", fileResult.FileDownloadName);
            Assert.Equal(pdfBytes, fileResult.FileContents);
        }

        [Fact]
        public async Task ExportQuizTryAsPdf_EmptyAnswers_ReturnsFileResult()
        {
            // Arrange
            var userId = 1;
            var quizId = 1;
            var attemptId = 1;
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

            var attemptResult = new ServiceResult<QuizTryDetailsDto>
            {
                Success = true,
                Data = quizTryDetails
            };

            var pdfBytes = new byte[] { 1, 2, 3, 4, 5 };
            var exportResult = new ServiceResult<byte[]>
            {
                Success = true,
                Data = pdfBytes
            };

            _mockQuizService.Setup(s => s.GetQuizByIdAsync(quizId))
                .Returns(Task.FromResult(quizResult));

            _mockQuizTryService.Setup(s => s.GetQuizTryDetailsAsync(attemptId))
                .Returns(Task.FromResult(attemptResult));

            _mockQuizExportService.Setup(s => s.ExportTryAsPdfAsync(attemptId))
                .Returns(Task.FromResult(exportResult));

            // Act
            var result = await _quizExportController.ExportQuizTryAsPdf(quizId, attemptId);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/pdf", fileResult.ContentType);
            Assert.Equal($"quiz-{quizId}-attempt-{attemptId}.pdf", fileResult.FileDownloadName);
            Assert.Equal(pdfBytes, fileResult.FileContents);
        }

        [Fact]
        public async Task ExportQuizAsText_IncludeAnswersFalse_ReturnsFileResult()
        {
            // Arrange
            var userId = 1;
            var quizId = 1;
            var includeAnswers = false;
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

            var textBytes = System.Text.Encoding.UTF8.GetBytes("Test Quiz Content");
            var exportResult = new ServiceResult<byte[]>
            {
                Success = true,
                Data = textBytes
            };

            _mockQuizService.Setup(s => s.GetQuizByIdAsync(quizId))
                .Returns(Task.FromResult(quizResult));

            _mockQuizExportService.Setup(s => s.ExportAsTextAsync(quizId, includeAnswers))
                .Returns(Task.FromResult(exportResult));

            // Act
            var result = await _quizExportController.ExportQuizAsText(quizId, includeAnswers);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("text/plain", fileResult.ContentType);
            Assert.Equal($"quiz-{quizId}.txt", fileResult.FileDownloadName);
            Assert.Equal(textBytes, fileResult.FileContents);
        }

        [Fact]
        public async Task ExportQuizTryAsText_InvalidAnswerData_ReturnsFileResult()
        {
            // Arrange
            var userId = 1;
            var quizId = 1;
            var attemptId = 1;
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

            var quizTryDetails = new QuizTryDetailsDto
            {
                Id = attemptId,
                QuizId = quizId,
                UserId = userId,
                QuizName = "Test Quiz",
                CurrentQuestionIndex = 0,
                TotalQuestions = 10,
                Questions = new List<QuizTryQuestionDto>
                {
                    new QuizTryQuestionDto
                    {
                        Id = 1,
                        Text = "Test Question",
                        Type = "multiple-choice",
                        AnswerOptions = new List<QuizTryAnswerOptionDto>
                        {
                            new QuizTryAnswerOptionDto
                            {
                                Id = 1,
                                Text = null,
                                IsSelected = false
                            }
                        }
                    }
                }
            };

            var attemptResult = new ServiceResult<QuizTryDetailsDto>
            {
                Success = true,
                Data = quizTryDetails
            };

            var textBytes = System.Text.Encoding.UTF8.GetBytes("Test Quiz Attempt Content");
            var exportResult = new ServiceResult<byte[]>
            {
                Success = true,
                Data = textBytes
            };

            _mockQuizService.Setup(s => s.GetQuizByIdAsync(quizId))
                .Returns(Task.FromResult(quizResult));

            _mockQuizTryService.Setup(s => s.GetQuizTryDetailsAsync(attemptId))
                .Returns(Task.FromResult(attemptResult));

            _mockQuizExportService.Setup(s => s.ExportTryAsTextAsync(attemptId))
                .Returns(Task.FromResult(exportResult));

            // Act
            var result = await _quizExportController.ExportQuizTryAsText(quizId, attemptId);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("text/plain", fileResult.ContentType);
            Assert.Equal($"quiz-{quizId}-attempt-{attemptId}.txt", fileResult.FileDownloadName);
            Assert.Equal(textBytes, fileResult.FileContents);
        }
    }
} 