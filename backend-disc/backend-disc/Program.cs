using backend_disc.Dtos.Departments;
using backend_disc.Dtos.DiscProfiles;
using backend_disc.Dtos.Positions;
using backend_disc.Models;
using backend_disc.Repositories;
using backend_disc.Services;
using class_library_disc.Data;
using class_library_disc.Models.Sql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

// Load environment variables from .env file only in local development (not Docker)
var envPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", ".env");
if (File.Exists(envPath))
{
    Console.WriteLine($"Loading .env file from: {envPath}");
    DotNetEnv.Env.Load(envPath);
}
else
{
    Console.WriteLine("No .env file found, using environment variables from Docker/system");
}

var builder = WebApplication.CreateBuilder(args);

// Configure Sentry only if DSN is provided
var sentryDsn = builder.Configuration["Sentry:Dsn"] 
    ?? Environment.GetEnvironmentVariable("SENTRY_DSN");

if (!string.IsNullOrWhiteSpace(sentryDsn))
{
    builder.WebHost.UseSentry(o =>
    {
        o.Dsn = sentryDsn;
        o.Debug = false;
        o.TracesSampleRate = 1.0;
        o.Environment = builder.Environment.EnvironmentName;
        o.AttachStacktrace = false; 
        o.StackTraceMode = Sentry.StackTraceMode.Original; 
    });
}

builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
    config.SetMinimumLevel(LogLevel.Debug);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowFrontend",
                              policy =>
                              {
                                  policy
            .WithOrigins(
                "http://localhost:3000",
                "https://disc-application-fs-frontend.onrender.com"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();

                              });
    options.AddPolicy(name: "OnlyGET",
                              policy =>
                              {
                                  policy.AllowAnyOrigin()
                                  .WithMethods("GET")
                                  .AllowAnyHeader();
                              });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "JWT Authentication",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };
    options.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {securityScheme, Array.Empty<string>()}
    });
});

builder.Services.AddDbContext<DiscProfileDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddAutoMapper(
    cfg => { },
    typeof(AutoMapperProfile)
);

builder.Services.AddScoped<IGenericService<DepartmentDto, CreateDepartmentDto, UpdateDepartmentDto>,
    GenericService<Department, DepartmentDto, CreateDepartmentDto, UpdateDepartmentDto>>();
builder.Services.AddScoped<IGenericService<DiscProfileDto, CreateDiscProfileDto, UpdateDiscProfileDto>,
    GenericService<DiscProfile, DiscProfileDto, CreateDiscProfileDto, UpdateDiscProfileDto>>();
builder.Services.AddScoped<IGenericService<PositionDto, CreatePositionDto, UpdatePositionDto>,
    GenericService<Position, PositionDto, CreatePositionDto, UpdatePositionDto>>();

builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<IWeatherService, WeatherService>();

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEmployeesRepository, EmployeesRepository>();

builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IAuthService, AuthService>();

var secretKey = builder.Configuration["API_SECRET_KEY"]
    ?? throw new InvalidOperationException("API_SECRET_KEY is not configured");

var keyBytes = Encoding.UTF8.GetBytes(secretKey);
if (keyBytes.Length < 32)
{
    throw new InvalidOperationException(
        $"API_SECRET_KEY must be at least 32 characters. Current: {keyBytes.Length} bytes");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(jwtOptions =>
{
    jwtOptions.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        TryAllIssuerSigningKeys = true //without this token sekret key is not valid
    };
    

});

var app = builder.Build();

if (!string.IsNullOrWhiteSpace(sentryDsn))
{
    app.UseSentryTracing();
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Only create tables in development
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<DiscProfileDbContext>();
        await db.Database.EnsureCreatedAsync();
    }
}

await app.RunAsync();
