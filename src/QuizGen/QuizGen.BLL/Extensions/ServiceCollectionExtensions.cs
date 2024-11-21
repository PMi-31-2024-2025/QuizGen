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
        services.AddSingleton<IAuthStateService, AuthStateService>();
        return services;
    }
}