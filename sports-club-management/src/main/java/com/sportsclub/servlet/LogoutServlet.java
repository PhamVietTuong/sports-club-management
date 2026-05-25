package com.sportsclub.servlet;

import jakarta.servlet.ServletException;
import jakarta.servlet.annotation.WebServlet;
import jakarta.servlet.http.*;
import java.io.IOException;

/**
 * SECURITY — Logout controller.
 * session.invalidate() clears all session data, preventing session hijacking
 * after the user logs out.
 */
@WebServlet("/logout")
public class LogoutServlet extends HttpServlet {

    @Override
    protected void doGet(HttpServletRequest req, HttpServletResponse resp)
            throws ServletException, IOException {
        doPost(req, resp);
    }

    @Override
    protected void doPost(HttpServletRequest req, HttpServletResponse resp)
            throws ServletException, IOException {
        HttpSession session = req.getSession(false);
        if (session != null) {
            // Completely destroy the session — no partial cleanup
            session.invalidate();
        }
        // Redirect to login after logout
        resp.sendRedirect(req.getContextPath() + "/login");
    }
}
