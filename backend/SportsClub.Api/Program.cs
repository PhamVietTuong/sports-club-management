using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SportsClub.Api.Data;
using SportsClub.Api.Models.Dtos;
using SportsClub.Api.Patterns.Singleton;
using SportsClub.Api.Repositories;
using SportsClub.Api.Security;
using SportsClub.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ── JWT settings ─────────────────────────────────────────────────────────────
var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services.Configure<JwtSettings>(jwtSection);
var jwtSettings = jwtSection.Get<JwtSettings>() ?? new JwtSettings();

// ── EF Core (SQL Server) ─────────────────────────────────────────────────────
// SINGLETON PATTERN — the connection string comes from DatabaseConfig.Instance
// unless an explicit ConnectionStrings:Default override is configured.
var connectionString = builder.Configuration.GetConnectionString("Default")
                       ?? DatabaseConfig.Instance.ConnectionString;
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(connectionString));

// ── DAO repositories + services ──────────────────────────────────────────────
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<MemberRepository>();
builder.Services.AddScoped<CoachRepository>();
builder.Services.AddScoped<ClassRepository>();
builder.Services.AddScoped<PackageRepository>();
builder.Services.AddScoped<ScheduleRepository>();
builder.Services.AddScoped<EnrollmentRepository>();
builder.Services.AddScoped<AccountService>();
builder.Services.AddSingleton<JwtTokenService>();

// ── Authentication / Authorization (JWT bearer) ──────────────────────────────
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ClockSkew = TimeSpan.FromMinutes(1),
        };
    });
builder.Services.AddAuthorization();

// ── CORS for the React dev server ────────────────────────────────────────────
const string CorsPolicy = "spa";
builder.Services.AddCors(options => options.AddPolicy(CorsPolicy, policy =>
{
    var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
                  ?? new[] { "http://localhost:5173" };
    policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
}));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHttpsRedirection();
    // GLOBAL EXCEPTION HANDLER — any unhandled exception returns a controlled,
    // generic JSON message instead of leaking a stack trace / internals.
    // (In Development the framework's developer exception page is kept for
    // debugging.)
    app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(
            new MessageResponse("Đã xảy ra lỗi hệ thống. Vui lòng thử lại."));
    }));
}

// SECURITY HEADERS on every response (port of SecurityHeadersFilter).
app.UseMiddleware<SecurityHeadersMiddleware>();

app.UseCors(CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
