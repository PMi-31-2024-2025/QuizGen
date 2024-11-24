namespace QuizGen.DAL.Extensions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuizGen.DAL.Context;
using QuizGen.DAL.Interfaces;
using QuizGen.DAL.Repositories;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataAccessLayer(this IServiceCollection services, string connectionString)
    {
        _ = services.AddDbContext<AppDbContext>(options =>
        {
            _ = options.UseNpgsql(connectionString);
            _ = options.EnableSensitiveDataLogging();
            _ = options.EnableDetailedErrors();
        });

        // Register repositories
        _ = services.AddScoped<IUserRepository, UserRepository>();
        _ = services.AddScoped<IQuizRepository, QuizRepository>();
        _ = services.AddScoped<IQuestionRepository, QuestionRepository>();
        _ = services.AddScoped<IAnswerRepository, AnswerRepository>();
        _ = services.AddScoped<IQuizTryRepository, QuizTryRepository>();
        _ = services.AddScoped<IQuizAnswerRepository, QuizAnswerRepository>();

        // Initialize database
        using var scope = services.BuildServiceProvider().CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            //dbContext.Database.EnsureDeleted(); // Remove this in production!
            _ = dbContext.Database.EnsureCreated();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database initialization error: {ex.Message}");
            throw;
        }

        return services;
    }
}