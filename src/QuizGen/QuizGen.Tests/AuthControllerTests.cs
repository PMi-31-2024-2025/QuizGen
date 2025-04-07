using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using QuizGen.API.Controllers;
using QuizGen.BLL.Models.Auth;
using QuizGen.BLL.Models.Base;
using QuizGen.BLL.Services.Interfaces;
using QuizGen.DAL.Models;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;

namespace QuizGen.Tests
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly AuthController _authController;

        public AuthControllerTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _authController = new AuthController(_mockAuthService.Object);
        }

        private void SetupAuthenticatedUser(int userId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "test");
            var principal = new ClaimsPrincipal(identity);
            _authController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkResult()
        {
            // Arrange
            var request = new LoginRequest { Username = "testUser", Password = "password123" };
            var expectedResponse = new ServiceResult<AuthResult>
            {
                Success = true,
                Data = new AuthResult { Username = "testUser", AccessToken = "test-token" }
            };

            _mockAuthService.Setup(s => s.LoginAsync(request))
                .Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _authController.Login(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<AuthResult>(okResult.Value);
            Assert.Equal("testUser", response.Username);
            Assert.Equal("test-token", response.AccessToken);
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsBadRequest()
        {
            // Arrange
            var request = new LoginRequest { Username = "testUser", Password = "wrongpass" };
            var expectedResponse = new ServiceResult<AuthResult>
            {
                Success = false,
                Message = "Invalid username or password"
            };

            _mockAuthService.Setup(s => s.LoginAsync(request))
                .Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _authController.Login(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid username or password", badRequestResult.Value);
        }

        [Fact]
        public async Task Register_ValidData_ReturnsOkResult()
        {
            // Arrange
            var request = new RegisterRequest 
            { 
                Username = "newUser", 
                Password = "password123",
                Name = "Test User"
            };
            var expectedResponse = new ServiceResult<AuthResult>
            {
                Success = true,
                Data = new AuthResult { Username = "newUser" }
            };

            _mockAuthService.Setup(s => s.RegisterAsync(request))
                .Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _authController.Register(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<AuthResult>(okResult.Value);
            Assert.Equal("newUser", response.Username);
        }

        [Fact]
        public async Task Register_DuplicateUsername_ReturnsBadRequest()
        {
            // Arrange
            var request = new RegisterRequest 
            { 
                Username = "existingUser", 
                Password = "password123",
                Name = "Test User"
            };
            var expectedResponse = new ServiceResult<AuthResult>
            {
                Success = false,
                Message = "Username already exists"
            };

            _mockAuthService.Setup(s => s.RegisterAsync(request))
                .Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _authController.Register(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Username already exists", badRequestResult.Value);
        }

        [Fact]
        public async Task GetCurrentUser_AuthenticatedUser_ReturnsOkResult()
        {
            // Arrange
            var userId = 1;
            SetupAuthenticatedUser(userId);

            var expectedResponse = new ServiceResult<AuthResult>
            {
                Success = true,
                Data = new AuthResult 
                { 
                    UserId = userId,
                    Username = "testUser",
                    Name = "Test User"
                }
            };

            _mockAuthService.Setup(s => s.GetCurrentUserAsync(userId))
                .Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _authController.GetCurrentUser();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<AuthResult>(okResult.Value);
            Assert.Equal(userId, response.UserId);
            Assert.Equal("testUser", response.Username);
        }

        [Fact]
        public async Task GetCurrentUser_UnauthenticatedUser_ReturnsUnauthorized()
        {
            // Arrange
            _authController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _authController.GetCurrentUser();

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task ChangePassword_ValidData_ReturnsOkResult()
        {
            // Arrange
            var userId = 1;
            SetupAuthenticatedUser(userId);

            var request = new ChangePasswordRequest 
            { 
                CurrentPassword = "oldpass",
                NewPassword = "newpass"
            };

            var expectedResponse = new ServiceResult<bool>
            {
                Success = true
            };

            _mockAuthService.Setup(s => s.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword))
                .Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _authController.ChangePassword(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value;
            var message = response.GetType().GetProperty("message").GetValue(response) as string;
            Assert.Equal("Password changed successfully", message);
        }

        [Fact]
        public async Task ChangePassword_InvalidCurrentPassword_ReturnsBadRequest()
        {
            // Arrange
            var userId = 1;
            SetupAuthenticatedUser(userId);

            var request = new ChangePasswordRequest 
            { 
                CurrentPassword = "wrongpass",
                NewPassword = "newpass"
            };

            var expectedResponse = new ServiceResult<bool>
            {
                Success = false,
                Message = "Current password is incorrect"
            };

            _mockAuthService.Setup(s => s.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword))
                .Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _authController.ChangePassword(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Current password is incorrect", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateProfile_ValidData_ReturnsOkResult()
        {
            // Arrange
            var userId = 1;
            SetupAuthenticatedUser(userId);

            var request = new UpdateProfileRequest 
            { 
                Name = "Updated Name",
                OpenAiApiKey = "new-api-key",
                GptModel = "gpt-4"
            };

            var expectedResponse = new ServiceResult<bool>
            {
                Success = true
            };

            _mockAuthService.Setup(s => s.UpdateProfileAsync(userId, request.Name, request.OpenAiApiKey, request.GptModel))
                .Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _authController.UpdateProfile(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value;
            var message = response.GetType().GetProperty("message").GetValue(response) as string;
            Assert.Equal("Profile updated successfully", message);
        }

        [Fact]
        public async Task UpdateProfile_InvalidData_ReturnsBadRequest()
        {
            // Arrange
            var userId = 1;
            SetupAuthenticatedUser(userId);

            var request = new UpdateProfileRequest 
            { 
                Name = "Updated Name",
                OpenAiApiKey = "invalid-key",
                GptModel = "invalid-model"
            };

            var expectedResponse = new ServiceResult<bool>
            {
                Success = false,
                Message = "Invalid OpenAI API key format"
            };

            _mockAuthService.Setup(s => s.UpdateProfileAsync(userId, request.Name, request.OpenAiApiKey, request.GptModel))
                .Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _authController.UpdateProfile(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid OpenAI API key format", badRequestResult.Value);
        }
    }

    // Helper class for anonymous objects
    public class AnonymousObject
    {
        private readonly object _object;

        public AnonymousObject(object obj)
        {
            _object = obj;
        }

        public T GetPropertyValue<T>(string propertyName)
        {
            var property = _object.GetType().GetProperty(propertyName);
            return (T)property.GetValue(_object);
        }
    }
} 