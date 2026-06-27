using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SportsClub.Api.Data;
using SportsClub.Api.Models.Dtos;
using SportsClub.Api.Patterns.Singleton;
using SportsClub.Api.Hubs;
using SportsClub.Api.Repositories;
using SportsClub.Api.Security;
using SportsClub.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ── JWT settings ─────────────────────────────────────────────────────────────
var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services.Configure<JwtSettings>(jwtSection);
var jwtSettings = jwtSection.Get<JwtSettings>() ?? new JwtSettings();

// API/JWT HARDENING — fail fast rather than ship a weak signing key. The value
// committed to appsettings.json is a clearly-marked dev placeholder; in any
// non-Development environment the key MUST be overridden (e.g. Jwt__Key env var)
// with a strong secret, otherwise tokens could be forged.
if (!builder.Environment.IsDevelopment())
{
    if (string.IsNullOrWhiteSpace(jwtSettings.Key)
        || jwtSettings.Key.Contains("CHANGE_ME")
        || Encoding.UTF8.GetByteCount(jwtSettings.Key) < 32)
    {
        throw new InvalidOperationException(
            "Jwt:Key must be overridden with a strong secret (>= 32 bytes) outside Development. " +
            "Set the Jwt__Key environment variable.");
    }
}

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
builder.Services.AddScoped<EquipmentRepository>();
builder.Services.AddScoped<PaymentRepository>();
builder.Services.AddScoped<AttendanceRepository>();
builder.Services.AddScoped<LessonPlanRepository>();
builder.Services.AddScoped<ProgressNoteRepository>();
builder.Services.AddScoped<CoachRatingRepository>();
builder.Services.AddScoped<HealthMetricRepository>();
builder.Services.AddScoped<PtSessionRepository>();
builder.Services.AddScoped<MessageRepository>();
builder.Services.AddScoped<MembershipRequestRepository>();
builder.Services.AddScoped<PackageClassRepository>();
builder.Services.AddScoped<ClassChangeRequestRepository>();
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<ChatDirectory>();
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

        // SignalR WebSocket connections cannot send an Authorization header, so the
        // JS client passes the JWT as ?access_token=… . Read it only for the hub path.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                    context.Token = accessToken;
                return Task.CompletedTask;
            },
        };
    });
builder.Services.AddAuthorization();

// ── CORS for the React dev server ────────────────────────────────────────────
const string CorsPolicy = "spa";
builder.Services.AddCors(options => options.AddPolicy(CorsPolicy, policy =>
{
    var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
                  ?? new[] { "http://localhost:5173" };
    // AllowCredentials is required for the SignalR connection (and is safe here
    // because the origin list is explicit, never AllowAnyOrigin).
    policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
}));

// RATE LIMITING — a second, IP-based layer in front of the per-username
// brute-force lockout (and a general API-abuse guard). The "login" policy caps
// login attempts from a single IP, so an attacker cannot spray many usernames
// from one host to sidestep the per-account counter. Over-limit → HTTP 429.
const string LoginRateLimit = "login";
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy(LoginRateLimit, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
            }));
});

builder.Services.AddControllers();
builder.Services.AddSignalR();
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
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");

app.Run();
