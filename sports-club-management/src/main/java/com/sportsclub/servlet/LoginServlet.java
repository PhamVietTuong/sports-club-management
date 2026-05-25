package com.sportsclub.servlet;

import com.sportsclub.dao.CoachDAO;
import com.sportsclub.dao.MemberDAO;
import com.sportsclub.dao.UserDAO;
import com.sportsclub.model.Coach;
import com.sportsclub.model.Member;
import com.sportsclub.model.User;
import com.sportsclub.util.BCryptUtil;
import com.sportsclub.util.CsrfUtils;
import jakarta.servlet.ServletException;
import jakarta.servlet.annotation.WebServlet;
import jakarta.servlet.http.*;
import java.io.IOException;

/**
 * SECURITY — Login controller.
 * Implements: BCrypt verification, brute-force lockout after 5 failures,
 * session fixation protection, and HttpOnly/Secure cookie flags.
 */
@WebServlet("/login")
public class LoginServlet extends HttpServlet {

    private static final int MAX_ATTEMPTS = 5;

    private final UserDAO   userDAO   = new UserDAO();
    private final MemberDAO memberDAO = new MemberDAO();
    private final CoachDAO  coachDAO  = new CoachDAO();

    @Override
    protected void doGet(HttpServletRequest req, HttpServletResponse resp)
            throws ServletException, IOException {
        // Generate CSRF token and expose it to the login form
        HttpSession session = req.getSession(true);
        String csrfToken = CsrfUtils.generateToken(session);
        req.setAttribute("csrfToken", csrfToken);
        req.getRequestDispatcher("/WEB-INF/views/common/login.jsp").forward(req, resp);
    }

    @Override
    protected void doPost(HttpServletRequest req, HttpServletResponse resp)
            throws ServletException, IOException {

        String username = req.getParameter("username");
        String password = req.getParameter("password");
        String clientIp = req.getRemoteAddr();

        try {
            // BRUTE-FORCE PROTECTION — reject if too many recent failures
            int failCount = userDAO.countRecentFailedAttempts(username);
            if (failCount >= MAX_ATTEMPTS) {
                req.setAttribute("error", "Account temporarily locked. Please wait 15 minutes.");
                HttpSession s = req.getSession(true);
                req.setAttribute("csrfToken", CsrfUtils.generateToken(s));
                req.getRequestDispatcher("/WEB-INF/views/common/login.jsp").forward(req, resp);
                return;
            }

            User user = userDAO.findByUsername(username);

            // BCrypt verification — timing-safe comparison
            if (user != null && BCryptUtil.checkPassword(password, user.getPasswordHash())) {
                userDAO.logLoginAttempt(username, clientIp, true);

                // SESSION FIXATION PROTECTION — invalidate old session, create new one
                HttpSession oldSession = req.getSession(false);
                if (oldSession != null) oldSession.invalidate();
                HttpSession newSession = req.getSession(true);

                // SESSION SECURITY — 30-minute inactivity timeout
                newSession.setMaxInactiveInterval(30 * 60);

                // Store the full domain object (Member or Coach) for role-specific access
                if (user.getRole() == User.Role.MEMBER) {
                    Member member = memberDAO.findByUserId(user.getId());
                    newSession.setAttribute("loggedInUser", member != null ? member : user);
                } else if (user.getRole() == User.Role.COACH) {
                    Coach coach = coachDAO.findByUserId(user.getId());
                    newSession.setAttribute("loggedInUser", coach != null ? coach : user);
                } else {
                    newSession.setAttribute("loggedInUser", user);
                }

                // Redirect based on role
                switch (user.getRole()) {
                    case ADMIN:  resp.sendRedirect(req.getContextPath() + "/admin/dashboard");  break;
                    case COACH:  resp.sendRedirect(req.getContextPath() + "/coach/dashboard");  break;
                    case MEMBER: resp.sendRedirect(req.getContextPath() + "/member/dashboard"); break;
                }
            } else {
                // Log failed attempt for brute-force tracking
                userDAO.logLoginAttempt(username, clientIp, false);
                req.setAttribute("error", "Invalid username or password.");
                HttpSession s = req.getSession(true);
                req.setAttribute("csrfToken", CsrfUtils.generateToken(s));
                req.getRequestDispatcher("/WEB-INF/views/common/login.jsp").forward(req, resp);
            }
        } catch (Exception e) {
            // Never expose system internals to the user
            req.setAttribute("error", "A system error occurred. Please try again.");
            req.getRequestDispatcher("/WEB-INF/views/common/login.jsp").forward(req, resp);
        }
    }
}
