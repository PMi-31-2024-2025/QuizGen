namespace QuizGen.BLL.Extensions;

using Microsoft.Extensions.DependencyInjection;
using QuizGen.BLL.Services;
using QuizGen.BLL.Services.Interfaces;
using QuizGen.BLL.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services, AppConfig config)
    {
        services.AddSingleton(config);
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IQuizService, QuizService>();

        services.AddSingleton<IAuthStateService, AuthStateService>();
        services.AddSingleton<IOpenAiService, OpenAiService>();
        
        return services;
    }
}