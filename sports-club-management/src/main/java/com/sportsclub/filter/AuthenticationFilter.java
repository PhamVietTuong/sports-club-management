package com.sportsclub.filter;

import com.sportsclub.model.User;
import jakarta.servlet.*;
import jakarta.servlet.annotation.WebFilter;
import jakarta.servlet.http.HttpServletRequest;
import jakarta.servlet.http.HttpServletResponse;
import jakarta.servlet.http.HttpSession;
import java.io.IOException;
import java.util.Arrays;
import java.util.HashSet;
import java.util.Set;

/**
 * SECURITY — Authentication gate: every request to protected paths
 * requires a valid session with a logged-in user.
 * Unauthenticated requests are redirected to /login.
 */
@WebFilter(urlPatterns = {"/admin/*", "/coach/*", "/member/*"})
public class AuthenticationFilter implements Filter {

    // Paths that are always reachable without authentication
    private static final Set<String> PUBLIC_PATHS = new HashSet<>(Arrays.asList(
        "/login", "/register", "/error"
    ));

    @Override
    public void doFilter(ServletRequest req, ServletResponse res, FilterChain chain)
            throws IOException, ServletException {

        HttpServletRequest  request  = (HttpServletRequest)  req;
        HttpServletResponse response = (HttpServletResponse) res;

        String path = request.getServletPath();

        // Allow public paths through without checking session
        for (String pub : PUBLIC_PATHS) {
            if (path.startsWith(pub)) {
                chain.doFilter(req, res);
                return;
            }
        }

        // Check session for authenticated user
        HttpSession session = request.getSession(false);
        User loggedInUser = (session != null) ? (User) session.getAttribute("loggedInUser") : null;

        if (loggedInUser == null) {
            // Not authenticated — redirect to login page
            response.sendRedirect(request.getContextPath() + "/login");
        } else {
            chain.doFilter(req, res);
        }
    }
}
