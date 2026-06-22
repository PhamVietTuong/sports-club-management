namespace SportsClub.Api.Security;

/// <summary>
/// SECURITY HEADERS — adds hardening headers to every HTTP response, a port of
/// the Java <c>SecurityHeadersFilter</c>. Prevents MIME-sniffing, clickjacking
/// and cross-site script/resource loading, and enforces HTTPS.
///
/// Because this is a pure JSON API (responses are never rendered as an HTML
/// document), the Content-Security-Policy can be maximally strict —
/// <c>default-src 'none'</c> blocks any script/style/image/frame the response
/// could be coerced into loading, and <c>frame-ancestors 'none'</c> is the
/// modern, CSP-level replacement for X-Frame-Options against clickjacking.
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

        // CONTENT-SECURITY-POLICY (XSS / HTML-injection defense-in-depth). A JSON
        // API never needs to load any resource, so lock everything down.
        headers["Content-Security-Policy"] =
            "default-src 'none'; frame-ancestors 'none'; base-uri 'none'; form-action 'none'";

        // PERMISSIONS-POLICY — disable powerful browser features outright.
        headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";

        // HSTS only makes sense (and is only honoured) over HTTPS; sending it on
        // plain HTTP is meaningless, so scope it to secure requests.
        if (context.Request.IsHttps)
            headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";

        await _next(context);
    }
}
