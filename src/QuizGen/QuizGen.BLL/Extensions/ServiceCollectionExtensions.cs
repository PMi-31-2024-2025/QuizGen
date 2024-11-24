namespace QuizGen.BLL.Extensions;

using Microsoft.Extensions.DependencyInjection;
using QuizGen.BLL.Configuration;
using QuizGen.BLL.Services;
using QuizGen.BLL.Services.Interfaces;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services, AppConfig config)
    {
        _ = services.AddSingleton(config);
        _ = services.AddScoped<IAuthService, AuthService>();
        _ = services.AddScoped<IQuizService, QuizService>();
        _ = services.AddScoped<IQuizExportService, QuizExportService>();
        _ = services.AddScoped<IQuizTryService, QuizTryService>();

        _ = services.AddSingleton<IAuthStateService, AuthStateService>();
        _ = services.AddSingleton<IOpenAiService, OpenAiService>();

        return services;
    }
}