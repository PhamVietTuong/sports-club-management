package com.sportsclub.servlet.admin;

import com.sportsclub.dao.CoachDAO;
import com.sportsclub.dao.UserDAO;
import com.sportsclub.model.Coach;
import com.sportsclub.util.BCryptUtil;
import com.sportsclub.util.CsrfUtils;
import jakarta.servlet.ServletException;
import jakarta.servlet.annotation.WebServlet;
import jakarta.servlet.http.*;
import java.io.IOException;

@WebServlet("/admin/coaches")
public class CoachManagementServlet extends HttpServlet {

    private final CoachDAO coachDAO = new CoachDAO();
    private final UserDAO  userDAO  = new UserDAO();

    @Override
    protected void doGet(HttpServletRequest req, HttpServletResponse resp)
            throws ServletException, IOException {
        try {
            req.setAttribute("coaches", coachDAO.findAll());
            req.setAttribute("csrfToken", CsrfUtils.generateToken(req.getSession()));
            req.getRequestDispatcher("/WEB-INF/views/admin/coaches.jsp").forward(req, resp);
        } catch (Exception e) {
            req.setAttribute("error", "Could not load coaches.");
            req.getRequestDispatcher("/WEB-INF/views/admin/coaches.jsp").forward(req, resp);
        }
    }

    @Override
    protected void doPost(HttpServletRequest req, HttpServletResponse resp)
            throws ServletException, IOException {
        String action = req.getParameter("action");
        try {
            if ("add".equals(action)) {
                String username       = req.getParameter("username");
                String email          = req.getParameter("email");
                String password       = req.getParameter("password");
                String fullName       = req.getParameter("fullName");
                String phone          = req.getParameter("phone");
                String specialization = req.getParameter("specialization");
                String bio            = req.getParameter("bio");
                int    experience     = Integer.parseInt(req.getParameter("experience"));
                double salary         = Double.parseDouble(req.getParameter("salary"));

                if (isBlank(fullName)) {
                    resp.sendRedirect(req.getContextPath() + "/admin/coaches?error=1");
                    return;
                }
                fullName = fullName.trim();

                String hash   = BCryptUtil.hashPassword(password);
                int    userId = userDAO.insert(username, hash, email, phone, "COACH");
                if (userId > 0) {
                    coachDAO.insert(userId, fullName, specialization, bio, experience, salary);
                }
            } else if ("update".equals(action)) {
                int    id             = Integer.parseInt(req.getParameter("id"));
                String fullName       = req.getParameter("fullName");
                if (isBlank(fullName)) {
                    resp.sendRedirect(req.getContextPath() + "/admin/coaches?error=1");
                    return;
                }
                Coach  coach          = coachDAO.findById(id);
                if (coach != null) {
                    coach.setFullName(fullName.trim());
                    coach.setSpecialization(req.getParameter("specialization"));
                    coach.setBio(req.getParameter("bio"));
                    coach.setExperience(Integer.parseInt(req.getParameter("experience")));
                    coach.setSalary(Double.parseDouble(req.getParameter("salary")));
                    coachDAO.update(coach);
                }
            }
            resp.sendRedirect(req.getContextPath() + "/admin/coaches");
        } catch (Exception e) {
            resp.sendRedirect(req.getContextPath() + "/admin/coaches?error=1");
        }
    }

    private boolean isBlank(String s) {
        return s == null || s.trim().isEmpty();
    }
}
