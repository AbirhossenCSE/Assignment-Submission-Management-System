using AssignmentManagementSystem.API.Configurations;
using AssignmentManagementSystem.API.Middlewares;
using AssignmentManagementSystem.API.Repositories.Implementations;
using AssignmentManagementSystem.API.Repositories.Interfaces;
using AssignmentManagementSystem.API.Services.Implementations;
using AssignmentManagementSystem.API.Services.Interfaces;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// 1. Serilog Setup (Console + Rolling File)
var logPath = Path.Combine(builder.Environment.ContentRootPath, "Logs", "app-.log");
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: logPath,
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();

builder.Host.UseSerilog();

// 2. Configuration Settings Registration
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

// 3. MongoDB Client & Database DI Registration
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var mongoSettings = builder.Configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>();
    ArgumentNullException.ThrowIfNull(mongoSettings, nameof(mongoSettings));
    if (string.IsNullOrWhiteSpace(mongoSettings.ConnectionString))
    {
        throw new InvalidOperationException("MongoDB ConnectionString is not configured.");
    }
    return new MongoClient(mongoSettings.ConnectionString);
});

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var mongoClient = sp.GetRequiredService<IMongoClient>();
    var mongoSettings = builder.Configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>();
    ArgumentNullException.ThrowIfNull(mongoSettings, nameof(mongoSettings));
    if (string.IsNullOrWhiteSpace(mongoSettings.DatabaseName))
    {
        throw new InvalidOperationException("MongoDB DatabaseName is not configured.");
    }
    return mongoClient.GetDatabase(mongoSettings.DatabaseName);
});

// 4. Repositories & Services Registration
builder.Services.AddScoped(typeof(IMongoRepository<>), typeof(MongoRepository<>));
builder.Services.AddScoped<IHealthService, HealthService>();

// 5. Add Controllers
builder.Services.AddControllers();

// 6. CORS Configuration for Next.js Client
const string corsPolicyName = "AllowNextJsClient";
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicyName, policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 7. Swagger / OpenAPI Configuration with JWT Bearer
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Assignment & Submission Management System API",
        Version = "v1",
        Description = "Role-based API for Admin, Teacher, and Student operations."
    });

    // JWT Security Definition for future auth integration
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by a space and your JWT token."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

var app = builder.Build();

// 8. Middleware Pipeline
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment() || true)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Assignment Management System API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors(corsPolicyName);

app.UseAuthorization();

app.MapControllers();

try
{
    Log.Information("Starting Assignment & Submission Management System Web API...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}
