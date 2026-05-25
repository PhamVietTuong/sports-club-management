package com.sportsclub.servlet.member;

import com.sportsclub.dao.EnrollmentDAO;
import com.sportsclub.dao.MemberDAO;
import com.sportsclub.dao.ScheduleDAO;
import com.sportsclub.model.Member;
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

@WebServlet("/member/dashboard")
public class MemberDashboardServlet extends HttpServlet {

    private final MemberDAO     memberDAO     = new MemberDAO();
    private final EnrollmentDAO enrollmentDAO = new EnrollmentDAO();
    private final ScheduleDAO   scheduleDAO   = new ScheduleDAO();

    @Override
    protected void doGet(HttpServletRequest req, HttpServletResponse resp)
            throws ServletException, IOException {
        try {
            User   loggedIn = (User) req.getSession().getAttribute("loggedInUser");
            Member member   = memberDAO.findByUserId(loggedIn.getId());

            req.setAttribute("member", member);
            req.setAttribute("enrollments", enrollmentDAO.findByMemberId(member.getId()));

            // ITERATOR PATTERN — iterate member's personal schedule
            List<Schedule> allSchedules = scheduleDAO.findByMemberId(member.getId());
            ScheduleCollection sc = new ScheduleCollection();
            allSchedules.forEach(sc::add);

            ClubIterator<Schedule> si = sc.createIterator(); // ITERATOR in action
            List<Schedule> scheduleList = new ArrayList<>();
            while (si.hasNext()) scheduleList.add(si.next());

            req.setAttribute("schedules", scheduleList);
            req.setAttribute("csrfToken", CsrfUtils.generateToken(req.getSession()));
            req.getRequestDispatcher("/WEB-INF/views/member/dashboard.jsp").forward(req, resp);
        } catch (Exception e) {
            req.setAttribute("error", "Could not load dashboard.");
            req.getRequestDispatcher("/WEB-INF/views/member/dashboard.jsp").forward(req, resp);
        }
    }
}
