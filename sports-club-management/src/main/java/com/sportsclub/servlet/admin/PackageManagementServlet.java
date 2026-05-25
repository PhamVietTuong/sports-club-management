package com.sportsclub.servlet.admin;

import com.sportsclub.dao.PackageDAO;
import com.sportsclub.model.TrainingPackage;
import com.sportsclub.util.CsrfUtils;
import jakarta.servlet.ServletException;
import jakarta.servlet.annotation.WebServlet;
import jakarta.servlet.http.*;
import java.io.IOException;

@WebServlet("/admin/packages")
public class PackageManagementServlet extends HttpServlet {

    private final PackageDAO packageDAO = new PackageDAO();

    @Override
    protected void doGet(HttpServletRequest req, HttpServletResponse resp)
            throws ServletException, IOException {
        try {
            req.setAttribute("packages", packageDAO.findAll());
            req.setAttribute("csrfToken", CsrfUtils.generateToken(req.getSession()));
            req.getRequestDispatcher("/WEB-INF/views/admin/packages.jsp").forward(req, resp);
        } catch (Exception e) {
            req.setAttribute("error", "Could not load packages.");
            req.getRequestDispatcher("/WEB-INF/views/admin/packages.jsp").forward(req, resp);
        }
    }

    @Override
    protected void doPost(HttpServletRequest req, HttpServletResponse resp)
            throws ServletException, IOException {
        String action = req.getParameter("action");
        try {
            if ("add".equals(action)) {
                TrainingPackage pkg = new TrainingPackage();
                pkg.setName(req.getParameter("name"));
                pkg.setDurationMonths(Integer.parseInt(req.getParameter("durationMonths")));
                pkg.setPrice(Double.parseDouble(req.getParameter("price")));
                pkg.setMaxClasses(Integer.parseInt(req.getParameter("maxClasses")));
                pkg.setDescription(req.getParameter("description"));
                pkg.setActive(true);
                packageDAO.save(pkg);
            } else if ("update".equals(action)) {
                int id = Integer.parseInt(req.getParameter("id"));
                TrainingPackage pkg = packageDAO.findById(id);
                if (pkg != null) {
                    pkg.setName(req.getParameter("name"));
                    pkg.setDurationMonths(Integer.parseInt(req.getParameter("durationMonths")));
                    pkg.setPrice(Double.parseDouble(req.getParameter("price")));
                    pkg.setMaxClasses(Integer.parseInt(req.getParameter("maxClasses")));
                    pkg.setDescription(req.getParameter("description"));
                    pkg.setActive("on".equals(req.getParameter("isActive")));
                    packageDAO.update(pkg);
                }
            } else if ("clone".equals(action)) {
                // PROTOTYPE PATTERN — clone package template, then adjust
                int templateId = Integer.parseInt(req.getParameter("templateId"));
                TrainingPackage template = packageDAO.findById(templateId);
                if (template != null) {
                    TrainingPackage newPkg = template.clone(); // PROTOTYPE in action
                    newPkg.setId(0);
                    newPkg.setName("Copy of " + template.getName());
                    newPkg.setPrice(template.getPrice() * 1.2); // 20% premium
                    packageDAO.save(newPkg);
                }
            }
            resp.sendRedirect(req.getContextPath() + "/admin/packages");
        } catch (Exception e) {
            resp.sendRedirect(req.getContextPath() + "/admin/packages?error=1");
        }
    }
}
