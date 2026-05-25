package com.sportsclub.servlet;

import com.sportsclub.dao.MemberDAO;
import com.sportsclub.dao.UserDAO;
import com.sportsclub.model.User;
import com.sportsclub.util.BCryptUtil;
import com.sportsclub.util.CsrfUtils;
import jakarta.servlet.ServletException;
import jakarta.servlet.annotation.WebServlet;
import jakarta.servlet.http.*;
import java.io.IOException;
import java.time.LocalDate;

/**
 * SECURITY — Registration controller.
 * Validates all inputs, hashes password with BCrypt cost 12.
 */
@WebServlet("/register")
public class RegisterServlet extends HttpServlet {

    private final UserDAO   userDAO   = new UserDAO();
    private final MemberDAO memberDAO = new MemberDAO();

    @Override
    protected void doGet(HttpServletRequest req, HttpServletResponse resp)
            throws ServletException, IOException {
        HttpSession session = req.getSession(true);
        req.setAttribute("csrfToken", CsrfUtils.generateToken(session));
        req.getRequestDispatcher("/WEB-INF/views/common/register.jsp").forward(req, resp);
    }

    @Override
    protected void doPost(HttpServletRequest req, HttpServletResponse resp)
            throws ServletException, IOException {

        String username  = req.getParameter("username");
        String email     = req.getParameter("email");
        String password  = req.getParameter("password");
        String confirm   = req.getParameter("confirmPassword");
        String fullName  = req.getParameter("fullName");
        String phone     = req.getParameter("phone");
        String gender    = req.getParameter("gender");

        try {
            // Input validation
            if (isBlank(username) || isBlank(email) || isBlank(password) || isBlank(fullName)) {
                setError(req, resp, "All required fields must be filled.");
                return;
            }
            if (!password.equals(confirm)) {
                setError(req, resp, "Passwords do not match.");
                return;
            }
            if (password.length() < 8) {
                setError(req, resp, "Password must be at least 8 characters.");
                return;
            }
            if (userDAO.findByUsername(username) != null) {
                setError(req, resp, "Username already taken.");
                return;
            }
            if (userDAO.findByEmail(email) != null) {
                setError(req, resp, "Email already registered.");
                return;
            }

            // BCrypt hash — cost factor 12
            String hash   = BCryptUtil.hashPassword(password);
            int    userId = userDAO.insert(username, hash, email, phone, "MEMBER");

            if (userId > 0) {
                memberDAO.insert(userId, fullName, gender, null, null, 0, null);
                resp.sendRedirect(req.getContextPath() + "/login?registered=true");
            } else {
                setError(req, resp, "Registration failed. Please try again.");
            }
        } catch (Exception e) {
            setError(req, resp, "A system error occurred. Please try again.");
        }
    }

    private void setError(HttpServletRequest req, HttpServletResponse resp, String msg)
            throws ServletException, IOException {
        req.setAttribute("error", msg);
        HttpSession session = req.getSession(true);
        req.setAttribute("csrfToken", CsrfUtils.generateToken(session));
        req.getRequestDispatcher("/WEB-INF/views/common/register.jsp").forward(req, resp);
    }

    private boolean isBlank(String s) {
        return s == null || s.trim().isEmpty();
    }
}
