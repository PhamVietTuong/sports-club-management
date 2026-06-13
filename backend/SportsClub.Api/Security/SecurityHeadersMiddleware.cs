namespace SportsClub.Api.Security;

/// <summary>
/// SECURITY HEADERS — adds hardening headers to every HTTP response, a port of
/// the Java <c>SecurityHeadersFilter</c>. Prevents MIME-sniffing and
/// clickjacking and enforces HTTPS. (CSP is intentionally minimal for a JSON
/// API; the React app sets its own CSP when served.)
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;
        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "DENY";
        headers["X-XSS-Protection"] = "1; mode=block";
        headers["Referrer-Policy"] = "no-referrer";
        headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
        await _next(context);
    }
}
