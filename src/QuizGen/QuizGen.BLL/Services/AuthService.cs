namespace QuizGen.BLL.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using QuizGen.BLL.Models.Auth;
using QuizGen.BLL.Models.Base;
using QuizGen.BLL.Services.Interfaces;
using QuizGen.DAL.Interfaces;
using QuizGen.DAL.Models;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public AuthService(IUserRepository userRepository, IAuthStateService authStateService, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    public async Task<ServiceResult<AuthResult>> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username);

        if (user == null)
            return ServiceResult<AuthResult>.CreateError("Invalid username or password");

        var passwordHash = HashPassword(request.Password);
        if (user.PasswordHash != passwordHash)
            return ServiceResult<AuthResult>.CreateError("Invalid username or password");

        var authResult = MapToAuthResult(user);
        
        // Generate JWT token
        authResult.AccessToken = GenerateJwtToken(user);

        return ServiceResult<AuthResult>.CreateSuccess(authResult);
    }

    public async Task<ServiceResult<AuthResult>> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var existingUser = await _userRepository.GetByUsernameAsync(request.Username);
            if (existingUser != null)
            {
                return ServiceResult<AuthResult>.CreateError("Username already exists");
            }

            var user = new User
            {
                Username = request.Username,
                PasswordHash = HashPassword(request.Password),
                Name = request.Name,
                GptModel = "gpt-4o-mini"
            };

            _ = await _userRepository.AddAsync(user);
            var authResult = MapToAuthResult(user);
            
            // Generate JWT token for new user
            authResult.AccessToken = GenerateJwtToken(user);
            
            return ServiceResult<AuthResult>.CreateSuccess(authResult);
        }
        catch (Exception ex)
        {
            return ServiceResult<AuthResult>.CreateError($"Registration failed: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return ServiceResult<bool>.CreateError("User not found");

        var currentPasswordHash = HashPassword(currentPassword);
        if (user.PasswordHash != currentPasswordHash)
            return ServiceResult<bool>.CreateError("Current password is incorrect");

        user.PasswordHash = HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        return ServiceResult<bool>.CreateSuccess(true);
    }

    public async Task<ServiceResult<bool>> UpdateProfileAsync(int userId, string name, string openAiApiKey, string gptModel)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return ServiceResult<bool>.CreateError("User not found");

        user.Name = name;
        user.OpenAiApiKey = openAiApiKey;
        user.GptModel = gptModel;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        return ServiceResult<bool>.CreateSuccess(true);
    }

    public async Task<ServiceResult<AuthResult>> AutoLoginAsync(StoredCredentials credentials)
    {
        var user = await _userRepository.GetByIdAsync(credentials.UserId);
        if (user == null || user.Username != credentials.Username ||
            user.PasswordHash != credentials.HashedPassword)
        {
            return ServiceResult<AuthResult>.CreateError("Stored credentials are invalid");
        }

        var authResult = MapToAuthResult(user);
        
        // Generate JWT token
        authResult.AccessToken = GenerateJwtToken(user);
        
        return ServiceResult<AuthResult>.CreateSuccess(authResult);
    }
    
    public async Task<ServiceResult<AuthResult>> GetCurrentUserAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return ServiceResult<AuthResult>.CreateError("User not found");
            
        var authResult = MapToAuthResult(user);
        return ServiceResult<AuthResult>.CreateSuccess(authResult);
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private AuthResult MapToAuthResult(User user)
    {
        return new AuthResult
        {
            UserId = user.Id,
            Username = user.Username,
            Name = user.Name,
            OpenAiApiKey = user.OpenAiApiKey,
            GptModel = user.GptModel
        };
    }
    
    private string GenerateJwtToken(User user)
    {
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim("Name", user.Name)
        };
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpirationMinutes"])),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };
        
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        
        return tokenHandler.WriteToken(token);
    }
}