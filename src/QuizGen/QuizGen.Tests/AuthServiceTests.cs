using Microsoft.Extensions.Configuration;
using Moq;
using QuizGen.BLL.Models.Auth;
using QuizGen.BLL.Services;
using QuizGen.BLL.Services.Interfaces;
using QuizGen.DAL.Interfaces;
using QuizGen.DAL.Models;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IAuthStateService> _mockAuthStateService;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockAuthStateService = new Mock<IAuthStateService>();
        _mockConfiguration = new Mock<IConfiguration>();
        
        // Set up configuration mock for JWT token generation
        _mockConfiguration.Setup(c => c["Jwt:Key"]).Returns("TestSecretKey12345678901234567890");
        _mockConfiguration.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
        _mockConfiguration.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");
        _mockConfiguration.Setup(c => c["Jwt:ExpirationMinutes"]).Returns("60");
        
        _authService = new AuthService(
            _mockUserRepository.Object, 
            _mockAuthStateService.Object,
            _mockConfiguration.Object);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsSuccess()
    {
        var request = new LoginRequest { Username = "testUser", Password = "password123" };
        var user = new User
        {
            Id = 1,
            Name = "test1",
            Username = "testUser",
            PasswordHash = Convert.ToBase64String(System.Security.Cryptography.SHA256.Create()
                .ComputeHash(System.Text.Encoding.UTF8.GetBytes("password123")))
        };

        _ = _mockUserRepository.Setup(repo => repo.GetByUsernameAsync(request.Username))
            .ReturnsAsync(user);

        var result = await _authService.LoginAsync(request);

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(user.Username, result.Data.Username);
     
    }

    [Fact]
    public async Task LoginAsync_InvalidUsername_ReturnsError()
    {
        var request = new LoginRequest { Username = "invalidUser", Password = "password123" };
        _ = _mockUserRepository.Setup(repo => repo.GetByUsernameAsync(request.Username))
            .ReturnsAsync((User)null);

        var result = await _authService.LoginAsync(request);

        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("Invalid username or password", result.Message);
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ReturnsError()
    {
        var request = new LoginRequest { Username = "testUser", Password = "wrongPassword" };
        var user = new User
        {
            Id = 1,
            Name = "test1",
            Username = "testUser",
            PasswordHash = Convert.ToBase64String(System.Security.Cryptography.SHA256.Create()
                .ComputeHash(System.Text.Encoding.UTF8.GetBytes("password123")))
        };

        _ = _mockUserRepository.Setup(repo => repo.GetByUsernameAsync(request.Username))
            .ReturnsAsync(user);

        var result = await _authService.LoginAsync(request);

        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("Invalid username or password", result.Message);
    }
}
