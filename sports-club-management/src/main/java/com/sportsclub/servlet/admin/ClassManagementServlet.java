package com.sportsclub.servlet.admin;

import com.sportsclub.dao.ClassDAO;
import com.sportsclub.dao.CoachDAO;
import com.sportsclub.model.TrainingClass;
import com.sportsclub.pattern.iterator.ClassCollection;
import com.sportsclub.pattern.iterator.ClubIterator;
import com.sportsclub.util.CsrfUtils;
import jakarta.servlet.ServletException;
import jakarta.servlet.annotation.WebServlet;
import jakarta.servlet.http.*;
import java.io.IOException;
import java.util.ArrayList;
import java.util.List;

@WebServlet("/admin/classes")
public class ClassManagementServlet extends HttpServlet {

    private final ClassDAO classDAO = new ClassDAO();
    private final CoachDAO coachDAO = new CoachDAO();

    @Override
    protected void doGet(HttpServletRequest req, HttpServletResponse resp)
            throws ServletException, IOException {
        try {
            List<TrainingClass> all = classDAO.findAll();

            // ITERATOR PATTERN — traverse classes without exposing the internal List
            ClassCollection collection = new ClassCollection();
            all.forEach(collection::add);

            ClubIterator<TrainingClass> it = collection.createIterator(); // ITERATOR in action
            List<TrainingClass> displayList = new ArrayList<>();
            while (it.hasNext()) {
                displayList.add(it.next());
            }

            req.setAttribute("classes", displayList);
            req.setAttribute("coaches", coachDAO.findAll());
            req.setAttribute("csrfToken", CsrfUtils.generateToken(req.getSession()));
            req.getRequestDispatcher("/WEB-INF/views/admin/classes.jsp").forward(req, resp);
        } catch (Exception e) {
            req.setAttribute("error", "Không thể tải danh sách lớp học.");
            req.getRequestDispatcher("/WEB-INF/views/admin/classes.jsp").forward(req, resp);
        }
    }

    @Override
    protected void doPost(HttpServletRequest req, HttpServletResponse resp)
            throws ServletException, IOException {
        String action = req.getParameter("action");
        try {
            if ("add".equals(action)) {
                TrainingClass tc = new TrainingClass();
                tc.setName(req.getParameter("name"));
                tc.setCoachId(Integer.parseInt(req.getParameter("coachId")));
                tc.setCapacity(Integer.parseInt(req.getParameter("capacity")));
                tc.setLevel(req.getParameter("level"));
                tc.setDescription(req.getParameter("description"));
                tc.setActive(true);
                classDAO.insert(tc);
            } else if ("update".equals(action)) {
                int id = Integer.parseInt(req.getParameter("id"));
                TrainingClass tc = classDAO.findById(id);
                if (tc != null) {
                    tc.setName(req.getParameter("name"));
                    tc.setCoachId(Integer.parseInt(req.getParameter("coachId")));
                    tc.setCapacity(Integer.parseInt(req.getParameter("capacity")));
                    tc.setLevel(req.getParameter("level"));
                    tc.setDescription(req.getParameter("description"));
                    tc.setActive("on".equals(req.getParameter("isActive")));
                    classDAO.update(tc);
                }
            } else if ("clone".equals(action)) {
                // PROTOTYPE PATTERN — duplicate a class template
                int templateId = Integer.parseInt(req.getParameter("templateId"));
                TrainingClass template = classDAO.findById(templateId);
                if (template != null) {
                    TrainingClass copy = template.clone(); // PROTOTYPE in action
                    copy.setId(0);
                    copy.setName("Copy of " + template.getName());
                    copy.setCurrentEnrolled(0);
                    classDAO.insert(copy);
                }
            }
            resp.sendRedirect(req.getContextPath() + "/admin/classes");
        } catch (Exception e) {
            resp.sendRedirect(req.getContextPath() + "/admin/classes?error=1");
        }
    }
}
