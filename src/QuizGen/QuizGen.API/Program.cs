using QuizGen.BLL.Configuration;
using QuizGen.BLL.Extensions;
using QuizGen.BLL.Services;
using QuizGen.BLL.Services.Interfaces;
using QuizGen.DAL.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();

app.MapControllers();

app.Run();
