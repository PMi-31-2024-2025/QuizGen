using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuestPDF.Infrastructure;
using QuizGen.BLL.Configuration;
using QuizGen.BLL.Extensions;
using QuizGen.BLL.Services;
using QuizGen.BLL.Services.Interfaces;
using QuizGen.DAL.Extensions;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure QuestPDF
QuestPDF.Settings.License = LicenseType.Community;

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddRazorPages(options =>
{
    // More specific routes first
    options.Conventions.AddPageRoute("/QuizTry", "quiz/{quizId}/try/{attemptId}");
    options.Conventions.AddPageRoute("/QuizTry", "quiz/{quizId}/try");
    options.Conventions.AddPageRoute("/QuizResult", "quiz/result/{attemptId}");
    options.Conventions.AddPageRoute("/QuizDetails", "quiz/{id}");
    options.Conventions.AddPageRoute("/QuizList", "quizzes");
});

// Add HttpClient services
builder.Services.AddHttpClient();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "QuizGen API", Version = "v1" });
    
    // Define Bearer authentication scheme
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Configure app settings
var appConfig = new AppConfig
{
    DatabaseConnectionString = builder.Configuration.GetConnectionString("DefaultConnection"),
    LocalSettingsPath = "appsettings.json"
};

// Add BLL and DAL services
builder.Services.AddBusinessLogicLayer(appConfig);
builder.Services.AddDataAccessLayer(appConfig.DatabaseConnectionString);

// Register services
builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<IQuizTryService, QuizTryService>();
builder.Services.AddScoped<IQuizAnswerService, QuizAnswerService>();
builder.Services.AddScoped<IOpenAiService, OpenAiService>();
builder.Services.AddScoped<IQuizExportService, QuizExportService>();

// Add the HTTP context accessor
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowAll");

// Add Authentication middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

app.Run();
