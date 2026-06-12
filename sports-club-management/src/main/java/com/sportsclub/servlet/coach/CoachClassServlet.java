package com.sportsclub.servlet.coach;

import com.sportsclub.dao.ClassDAO;
import com.sportsclub.dao.CoachDAO;
import com.sportsclub.dao.EnrollmentDAO;
import com.sportsclub.model.Coach;
import com.sportsclub.model.User;
import com.sportsclub.util.CsrfUtils;
import jakarta.servlet.ServletException;
import jakarta.servlet.annotation.WebServlet;
import jakarta.servlet.http.*;
import java.io.IOException;

@WebServlet("/coach/classes")
public class CoachClassServlet extends HttpServlet {

    private final CoachDAO      coachDAO      = new CoachDAO();
    private final ClassDAO      classDAO      = new ClassDAO();
    private final EnrollmentDAO enrollmentDAO = new EnrollmentDAO();

    @Override
    protected void doGet(HttpServletRequest req, HttpServletResponse resp)
            throws ServletException, IOException {
        try {
            User  loggedIn = (User) req.getSession().getAttribute("loggedInUser");
            Coach coach    = coachDAO.findByUserId(loggedIn.getId());

            String classIdParam = req.getParameter("classId");
            if (classIdParam != null) {
                int classId = Integer.parseInt(classIdParam);
                com.sportsclub.model.TrainingClass selected = classDAO.findById(classId);

                // BROKEN ACCESS CONTROL / IDOR PREVENTION —
                // only expose the class (and its enrolled members) if it
                // actually belongs to the logged-in coach. Otherwise a coach
                // could read other coaches' classes by guessing classId.
                if (selected == null || selected.getCoachId() != coach.getId()) {
                    req.setAttribute("error", "Bạn không có quyền xem lớp học này.");
                } else {
                    req.setAttribute("selectedClass", selected);
                    req.setAttribute("enrolledMembers", enrollmentDAO.findByClassId(classId));
                }
            }

            req.setAttribute("myClasses", classDAO.findByCoachId(coach.getId()));
            req.setAttribute("csrfToken", CsrfUtils.generateToken(req.getSession()));
            req.getRequestDispatcher("/WEB-INF/views/coach/classes.jsp").forward(req, resp);
        } catch (Exception e) {
            req.setAttribute("error", "Không thể tải thông tin lớp học.");
            req.getRequestDispatcher("/WEB-INF/views/coach/classes.jsp").forward(req, resp);
        }
    }
}
