package com.sportsclub.servlet.member;

import com.sportsclub.dao.ClassDAO;
import com.sportsclub.dao.EnrollmentDAO;
import com.sportsclub.dao.MemberDAO;
import com.sportsclub.model.Member;
import com.sportsclub.model.TrainingClass;
import com.sportsclub.model.User;
import com.sportsclub.pattern.iterator.ClassCollection;
import com.sportsclub.pattern.iterator.ClubIterator;
import com.sportsclub.util.CsrfUtils;
import jakarta.servlet.ServletException;
import jakarta.servlet.annotation.WebServlet;
import jakarta.servlet.http.*;
import java.io.IOException;
import java.util.ArrayList;
import java.util.List;

@WebServlet("/member/classes")
public class EnrollmentServlet extends HttpServlet {

    private final ClassDAO      classDAO      = new ClassDAO();
    private final MemberDAO     memberDAO     = new MemberDAO();
    private final EnrollmentDAO enrollmentDAO = new EnrollmentDAO();

    @Override
    protected void doGet(HttpServletRequest req, HttpServletResponse resp)
            throws ServletException, IOException {
        try {
            List<TrainingClass> all = classDAO.findActive();

            // ITERATOR PATTERN — traverse active classes
            ClassCollection collection = new ClassCollection();
            all.forEach(collection::add);

            ClubIterator<TrainingClass> it = collection.createIterator(); // ITERATOR in action
            List<TrainingClass> displayList = new ArrayList<>();
            while (it.hasNext()) displayList.add(it.next());

            User   loggedIn = (User) req.getSession().getAttribute("loggedInUser");
            Member member   = memberDAO.findByUserId(loggedIn.getId());

            req.setAttribute("classes", displayList);
            req.setAttribute("member", member);
            req.setAttribute("csrfToken", CsrfUtils.generateToken(req.getSession()));
            req.getRequestDispatcher("/WEB-INF/views/member/classes.jsp").forward(req, resp);
        } catch (Exception e) {
            req.setAttribute("error", "Could not load classes.");
            req.getRequestDispatcher("/WEB-INF/views/member/classes.jsp").forward(req, resp);
        }
    }

    @Override
    protected void doPost(HttpServletRequest req, HttpServletResponse resp)
            throws ServletException, IOException {
        String action = req.getParameter("action");
        try {
            User   loggedIn = (User) req.getSession().getAttribute("loggedInUser");
            Member member   = memberDAO.findByUserId(loggedIn.getId());
            int    classId  = Integer.parseInt(req.getParameter("classId"));

            if ("enroll".equals(action)) {
                TrainingClass tc = classDAO.findById(classId);
                if (tc == null || !tc.isActive()) {
                    req.getSession().setAttribute("flash", "Class not available.");
                } else if (tc.getAvailableSlots() <= 0) {
                    req.getSession().setAttribute("flash", "Class is full.");
                } else if (enrollmentDAO.isEnrolled(member.getId(), classId)) {
                    req.getSession().setAttribute("flash", "Already enrolled.");
                } else {
                    enrollmentDAO.insert(member.getId(), classId);
                    classDAO.incrementEnrolled(classId);
                    req.getSession().setAttribute("flash", "Enrolled successfully!");
                }
            } else if ("cancel".equals(action)) {
                enrollmentDAO.cancel(member.getId(), classId);
                classDAO.decrementEnrolled(classId);
                req.getSession().setAttribute("flash", "Enrollment cancelled.");
            }
            resp.sendRedirect(req.getContextPath() + "/member/classes");
        } catch (Exception e) {
            resp.sendRedirect(req.getContextPath() + "/member/classes?error=1");
        }
    }
}
