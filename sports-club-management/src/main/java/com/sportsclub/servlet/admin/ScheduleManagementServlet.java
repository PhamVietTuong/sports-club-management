package com.sportsclub.servlet.admin;

import com.sportsclub.dao.ClassDAO;
import com.sportsclub.dao.ScheduleDAO;
import com.sportsclub.model.Schedule;
import com.sportsclub.pattern.iterator.ClubIterator;
import com.sportsclub.pattern.iterator.ScheduleCollection;
import com.sportsclub.util.CsrfUtils;
import jakarta.servlet.ServletException;
import jakarta.servlet.annotation.WebServlet;
import jakarta.servlet.http.*;
import java.io.IOException;
import java.time.LocalTime;
import java.util.ArrayList;
import java.util.List;

@WebServlet("/admin/schedules")
public class ScheduleManagementServlet extends HttpServlet {

    private final ScheduleDAO scheduleDAO = new ScheduleDAO();
    private final ClassDAO    classDAO    = new ClassDAO();

    @Override
    protected void doGet(HttpServletRequest req, HttpServletResponse resp)
            throws ServletException, IOException {
        try {
            List<Schedule> all = scheduleDAO.findAll();

            // ITERATOR PATTERN — iterate schedules without exposing internal list
            ScheduleCollection sc = new ScheduleCollection();
            all.forEach(sc::add);

            ClubIterator<Schedule> si = sc.createIterator(); // ITERATOR in action
            List<Schedule> displayList = new ArrayList<>();
            while (si.hasNext()) {
                displayList.add(si.next());
            }

            req.setAttribute("schedules", displayList);
            req.setAttribute("classes", classDAO.findActive());
            req.setAttribute("csrfToken", CsrfUtils.generateToken(req.getSession()));
            req.getRequestDispatcher("/WEB-INF/views/admin/schedules.jsp").forward(req, resp);
        } catch (Exception e) {
            req.setAttribute("error", "Could not load schedules.");
            req.getRequestDispatcher("/WEB-INF/views/admin/schedules.jsp").forward(req, resp);
        }
    }

    @Override
    protected void doPost(HttpServletRequest req, HttpServletResponse resp)
            throws ServletException, IOException {
        String action = req.getParameter("action");
        try {
            if ("add".equals(action)) {
                Schedule s = new Schedule();
                s.setClassId(Integer.parseInt(req.getParameter("classId")));
                s.setDayOfWeek(req.getParameter("dayOfWeek"));
                s.setStartTime(LocalTime.parse(req.getParameter("startTime")));
                s.setEndTime(LocalTime.parse(req.getParameter("endTime")));
                s.setRoom(req.getParameter("room"));
                s.setRepeatWeekly(true);
                scheduleDAO.save(s);
            } else if ("clone".equals(action)) {
                // PROTOTYPE PATTERN — clone this week's schedule for the next week
                int sourceId = Integer.parseInt(req.getParameter("sourceId"));
                Schedule thisWeek = scheduleDAO.findById(sourceId);
                if (thisWeek != null) {
                    Schedule nextWeek = thisWeek.clone(); // PROTOTYPE in action
                    nextWeek.setId(0);
                    // Append "(Copy)" to distinguish the cloned schedule
                    nextWeek.setRoom(thisWeek.getRoom() + " (Copy)");
                    scheduleDAO.save(nextWeek);
                }
            } else if ("delete".equals(action)) {
                int id = Integer.parseInt(req.getParameter("id"));
                scheduleDAO.delete(id);
            }
            resp.sendRedirect(req.getContextPath() + "/admin/schedules");
        } catch (Exception e) {
            resp.sendRedirect(req.getContextPath() + "/admin/schedules?error=1");
        }
    }
}
