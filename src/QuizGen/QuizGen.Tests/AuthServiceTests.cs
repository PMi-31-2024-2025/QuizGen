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
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockAuthStateService = new Mock<IAuthStateService>();
        _authService = new AuthService(_mockUserRepository.Object, _mockAuthStateService.Object);
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
        _mockAuthStateService.Verify(s => s.SetCredentials(It.IsAny<StoredCredentials>()), Times.Once);
        _mockAuthStateService.Verify(s => s.SaveStateAsync(), Times.Once);
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
