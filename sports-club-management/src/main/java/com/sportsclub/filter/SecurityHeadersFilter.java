package com.sportsclub.filter;

import jakarta.servlet.*;
import jakarta.servlet.annotation.WebFilter;
import jakarta.servlet.http.HttpServletResponse;
import java.io.IOException;

/**
 * SECURITY HEADERS — adds hardening headers to every HTTP response.
 * These prevent clickjacking (X-Frame-Options), MIME-sniffing,
 * reflected XSS, and enforce HTTPS.
 */
@WebFilter("/*")
public class SecurityHeadersFilter implements Filter {

    @Override
    public void doFilter(ServletRequest req, ServletResponse res, FilterChain chain)
            throws IOException, ServletException {

        HttpServletResponse response = (HttpServletResponse) res;

        // Prevent MIME-type sniffing attacks
        response.setHeader("X-Content-Type-Options", "nosniff");

        // Prevent clickjacking — disallow embedding in iframes
        response.setHeader("X-Frame-Options", "DENY");

        // Enable browser XSS filter (legacy browsers)
        response.setHeader("X-XSS-Protection", "1; mode=block");

        // Content Security Policy — restrict resource origins
        response.setHeader("Content-Security-Policy",
            "default-src 'self'; " +
            "script-src 'self' https://cdn.jsdelivr.net; " +
            "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://fonts.googleapis.com; " +
            "font-src 'self' https://fonts.gstatic.com https://cdn.jsdelivr.net; " +
            "img-src 'self' data:;");

        // Enforce HTTPS for 1 year
        response.setHeader("Strict-Transport-Security", "max-age=31536000; includeSubDomains");

        chain.doFilter(req, res);
    }
}
