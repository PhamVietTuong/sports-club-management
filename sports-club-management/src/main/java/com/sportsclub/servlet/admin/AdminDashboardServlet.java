package com.sportsclub.servlet.admin;

import com.sportsclub.dao.*;
import com.sportsclub.util.CsrfUtils;
import jakarta.servlet.ServletException;
import jakarta.servlet.annotation.WebServlet;
import jakarta.servlet.http.*;
import java.io.IOException;

@WebServlet("/admin/dashboard")
public class AdminDashboardServlet extends HttpServlet {

    private final MemberDAO memberDAO = new MemberDAO();
    private final CoachDAO  coachDAO  = new CoachDAO();
    private final ClassDAO  classDAO  = new ClassDAO();

    @Override
    protected void doGet(HttpServletRequest req, HttpServletResponse resp)
            throws ServletException, IOException {
        try {
            req.setAttribute("totalMembers", memberDAO.countAll());
            req.setAttribute("totalCoaches", coachDAO.countAll());
            req.setAttribute("totalClasses", classDAO.countAll());
            req.setAttribute("csrfToken", CsrfUtils.generateToken(req.getSession()));
            req.getRequestDispatcher("/WEB-INF/views/admin/dashboard.jsp").forward(req, resp);
        } catch (Exception e) {
            req.setAttribute("error", "Could not load dashboard data.");
            req.getRequestDispatcher("/WEB-INF/views/admin/dashboard.jsp").forward(req, resp);
        }
    }
}
