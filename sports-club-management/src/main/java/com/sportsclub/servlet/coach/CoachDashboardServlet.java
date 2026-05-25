package com.sportsclub.servlet.coach;

import com.sportsclub.dao.ClassDAO;
import com.sportsclub.dao.CoachDAO;
import com.sportsclub.dao.ScheduleDAO;
import com.sportsclub.model.Coach;
import com.sportsclub.model.Schedule;
import com.sportsclub.model.User;
import com.sportsclub.pattern.iterator.ClubIterator;
import com.sportsclub.pattern.iterator.ScheduleCollection;
import com.sportsclub.util.CsrfUtils;
import jakarta.servlet.ServletException;
import jakarta.servlet.annotation.WebServlet;
import jakarta.servlet.http.*;
import java.io.IOException;
import java.util.ArrayList;
import java.util.List;

@WebServlet("/coach/dashboard")
public class CoachDashboardServlet extends HttpServlet {

    private final CoachDAO    coachDAO    = new CoachDAO();
    private final ClassDAO    classDAO    = new ClassDAO();
    private final ScheduleDAO scheduleDAO = new ScheduleDAO();

    @Override
    protected void doGet(HttpServletRequest req, HttpServletResponse resp)
            throws ServletException, IOException {
        try {
            User loggedIn = (User) req.getSession().getAttribute("loggedInUser");
            Coach coach = coachDAO.findByUserId(loggedIn.getId());

            req.setAttribute("coach", coach);
            req.setAttribute("myClasses", classDAO.findByCoachId(coach.getId()));

            // ITERATOR PATTERN — iterate coach's schedules
            List<Schedule> allSchedules = scheduleDAO.findByCoachId(coach.getId());
            ScheduleCollection sc = new ScheduleCollection();
            allSchedules.forEach(sc::add);

            ClubIterator<Schedule> si = sc.createIterator(); // ITERATOR in action
            List<Schedule> scheduleList = new ArrayList<>();
            while (si.hasNext()) scheduleList.add(si.next());

            req.setAttribute("schedules", scheduleList);
            req.setAttribute("csrfToken", CsrfUtils.generateToken(req.getSession()));
            req.getRequestDispatcher("/WEB-INF/views/coach/dashboard.jsp").forward(req, resp);
        } catch (Exception e) {
            req.setAttribute("error", "Could not load dashboard.");
            req.getRequestDispatcher("/WEB-INF/views/coach/dashboard.jsp").forward(req, resp);
        }
    }
}
