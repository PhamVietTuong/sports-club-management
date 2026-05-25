package com.sportsclub.filter;

import com.sportsclub.model.User;
import jakarta.servlet.*;
import jakarta.servlet.annotation.WebFilter;
import jakarta.servlet.http.HttpServletRequest;
import jakarta.servlet.http.HttpServletResponse;
import jakarta.servlet.http.HttpSession;
import java.io.IOException;

/**
 * SECURITY — Role-Based Access Control (RBAC).
 * URL pattern → required role:
 *   /admin/*  → ADMIN only
 *   /coach/*  → COACH only
 *   /member/* → MEMBER only
 * Insufficient role → forward to /WEB-INF/views/common/403.jsp
 */
@WebFilter(urlPatterns = {"/admin/*", "/coach/*", "/member/*"})
public class AuthorizationFilter implements Filter {

    @Override
    public void doFilter(ServletRequest req, ServletResponse res, FilterChain chain)
            throws IOException, ServletException {

        HttpServletRequest  request  = (HttpServletRequest)  req;
        HttpServletResponse response = (HttpServletResponse) res;

        HttpSession session = request.getSession(false);
        if (session == null) {
            response.sendRedirect(request.getContextPath() + "/login");
            return;
        }

        User user = (User) session.getAttribute("loggedInUser");
        if (user == null) {
            response.sendRedirect(request.getContextPath() + "/login");
            return;
        }

        String path = request.getServletPath();
        boolean authorized = false;

        if (path.startsWith("/admin/")) {
            authorized = user.getRole() == User.Role.ADMIN;
        } else if (path.startsWith("/coach/")) {
            authorized = user.getRole() == User.Role.COACH;
        } else if (path.startsWith("/member/")) {
            authorized = user.getRole() == User.Role.MEMBER;
        } else {
            authorized = true;
        }

        if (authorized) {
            chain.doFilter(req, res);
        } else {
            // Insufficient privileges — forward to 403 error page
            request.getRequestDispatcher("/WEB-INF/views/common/403.jsp").forward(request, response);
        }
    }
}
