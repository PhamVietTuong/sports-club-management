package com.sportsclub.filter;

import com.sportsclub.util.CsrfUtils;
import jakarta.servlet.*;
import jakarta.servlet.annotation.WebFilter;
import jakarta.servlet.http.HttpServletRequest;
import jakarta.servlet.http.HttpServletResponse;
import jakarta.servlet.http.HttpSession;
import java.io.IOException;

/**
 * SECURITY — CSRF prevention.
 * On every POST request, the filter verifies that the submitted _csrf token
 * matches the token stored in the user's session.
 * A mismatch means the request did not originate from our own forms.
 */
@WebFilter(urlPatterns = {"/admin/*", "/coach/*", "/member/*", "/login", "/register"})
public class CsrfFilter implements Filter {

    @Override
    public void doFilter(ServletRequest req, ServletResponse res, FilterChain chain)
            throws IOException, ServletException {

        HttpServletRequest  request  = (HttpServletRequest)  req;
        HttpServletResponse response = (HttpServletResponse) res;

        if ("POST".equalsIgnoreCase(request.getMethod())) {
            HttpSession session = request.getSession(false);
            String submittedToken = request.getParameter("_csrf");

            // Validate the submitted CSRF token against the session token
            if (!CsrfUtils.isValidToken(session, submittedToken)) {
                response.sendError(HttpServletResponse.SC_FORBIDDEN,
                    "CSRF token validation failed. Please refresh the page and try again.");
                return;
            }
        }

        chain.doFilter(req, res);
    }
}
