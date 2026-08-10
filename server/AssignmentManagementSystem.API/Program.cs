using System.Text;
using AssignmentManagementSystem.API.Common;
using AssignmentManagementSystem.API.Configurations;
using AssignmentManagementSystem.API.Helpers.Implementations;
using AssignmentManagementSystem.API.Helpers.Interfaces;
using AssignmentManagementSystem.API.Middlewares;
using AssignmentManagementSystem.API.Repositories.Implementations;
using AssignmentManagementSystem.API.Repositories.Interfaces;
using AssignmentManagementSystem.API.Seed;
using AssignmentManagementSystem.API.Services.Implementations;
using AssignmentManagementSystem.API.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Bson;
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

// 3. HttpContextAccessor Registration
builder.Services.AddHttpContextAccessor();

// 4. MongoDB Client & Database DI Registration
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var mongoSettings = builder.Configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>();
    ArgumentNullException.ThrowIfNull(mongoSettings, nameof(mongoSettings));
    if (string.IsNullOrWhiteSpace(mongoSettings.ConnectionString))
    {
        throw new InvalidOperationException("MongoDB ConnectionString is not configured.");
    }

    var clientSettings = MongoClientSettings.FromConnectionString(mongoSettings.ConnectionString);
    clientSettings.ConnectTimeout = TimeSpan.FromSeconds(30);
    clientSettings.ServerSelectionTimeout = TimeSpan.FromSeconds(30);

    return new MongoClient(clientSettings);
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

// 5. JWT Authentication & Authorization Configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
ArgumentNullException.ThrowIfNull(jwtSettings, nameof(jwtSettings));
if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey))
{
    throw new InvalidOperationException("JwtSettings:SecretKey is not configured.");
}

var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// 6. Repositories, Helpers & Services Registration
builder.Services.AddScoped(typeof(IMongoRepository<>), typeof(MongoRepository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IClassRepository, ClassRepository>();
builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
builder.Services.AddScoped<IAssignmentRepository, AssignmentRepository>();
builder.Services.AddScoped<ISubmissionRepository, SubmissionRepository>();

builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IClassService, ClassService>();
builder.Services.AddScoped<ISubjectService, SubjectService>();
builder.Services.AddScoped<IAssignmentService, AssignmentService>();
builder.Services.AddScoped<ISubmissionService, SubmissionService>();
builder.Services.AddScoped<IHealthService, HealthService>();

// 7. Add Controllers & Custom ApiBehaviorOptions for Standardized Validation Errors
builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = actionContext =>
    {
        var errors = actionContext.ModelState
            .Where(e => e.Value?.Errors.Count > 0)
            .SelectMany(e => e.Value!.Errors.Select(err => string.IsNullOrWhiteSpace(err.ErrorMessage) ? err.Exception?.Message ?? "Validation error" : err.ErrorMessage))
            .ToList();

        var response = ApiResponse<object>.FailureResponse(
            message: "One or more validation errors occurred.",
            errors: errors,
            statusCode: StatusCodes.Status400BadRequest
        );

        return new BadRequestObjectResult(response);
    };
});

// 8. CORS Configuration for Next.js Client
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

// 9. Swagger / OpenAPI Configuration with JWT Bearer
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Assignment & Submission Management System API",
        Version = "v1",
        Description = "Role-based API for Admin, Teacher, and Student operations."
    });

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

// 10. Database Connection Diagnostic & Seeding Execution with Cold-Start Retry Loop
using (var scope = app.Services.CreateScope())
{
    var database = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
    var dbName = database.DatabaseNamespace.DatabaseName;

    const int maxRetries = 3;
    bool isConnected = false;

    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            Log.Information("Testing MongoDB connection to database '{DatabaseName}' (Attempt {Attempt}/{MaxRetries})...", dbName, attempt, maxRetries);
            using var pingCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var pingResult = await database.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1), cancellationToken: pingCts.Token);
            Log.Information("Successfully connected to MongoDB database '{DatabaseName}'. Ping response: {Ping}", dbName, pingResult.ToJson());
            isConnected = true;
            break;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "MongoDB connection attempt {Attempt}/{MaxRetries} failed for database '{DatabaseName}': {Message}", attempt, maxRetries, dbName, ex.Message);
            if (attempt < maxRetries)
            {
                Log.Information("Retrying MongoDB connection check in 2 seconds...");
                await Task.Delay(TimeSpan.FromSeconds(2));
            }
            else
            {
                Log.Error(ex, "All {MaxRetries} MongoDB connection attempts failed for database '{DatabaseName}'.", maxRetries, dbName);
                throw;
            }
        }
    }

    if (isConnected)
    {
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await DataSeeder.SeedAsync(database, passwordHasher);
    }
}

// 11. Middleware Pipeline Order: Global Exception Handling -> Serilog -> Swagger -> CORS -> Auth -> Controllers
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Assignment Management System API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors(corsPolicyName);

app.UseAuthentication();
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
