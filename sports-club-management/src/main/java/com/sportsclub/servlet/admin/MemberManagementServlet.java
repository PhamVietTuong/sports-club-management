package com.sportsclub.servlet.admin;

import com.sportsclub.dao.MemberDAO;
import com.sportsclub.dao.PackageDAO;
import com.sportsclub.model.Member;
import com.sportsclub.pattern.iterator.ClubIterator;
import com.sportsclub.pattern.iterator.MemberCollection;
import com.sportsclub.util.CsrfUtils;
import jakarta.servlet.ServletException;
import jakarta.servlet.annotation.WebServlet;
import jakarta.servlet.http.*;
import java.io.IOException;
import java.util.ArrayList;
import java.util.List;

@WebServlet("/admin/members")
public class MemberManagementServlet extends HttpServlet {

    private final MemberDAO  memberDAO  = new MemberDAO();
    private final PackageDAO packageDAO = new PackageDAO();

    @Override
    protected void doGet(HttpServletRequest req, HttpServletResponse resp)
            throws ServletException, IOException {
        try {
            String statusFilter = req.getParameter("status");

            List<Member> allMembers = (statusFilter != null && !statusFilter.isEmpty())
                ? memberDAO.findByStatus(statusFilter)
                : memberDAO.findAll();

            // ITERATOR PATTERN — traverse members without exposing the underlying List
            MemberCollection collection = new MemberCollection();
            allMembers.forEach(collection::add);

            ClubIterator<Member> it = collection.createIterator(); // ITERATOR in action
            List<Member> displayList = new ArrayList<>();
            while (it.hasNext()) {
                displayList.add(it.next());
            }

            req.setAttribute("members", displayList);
            req.setAttribute("packages", packageDAO.findActive());
            req.setAttribute("statusFilter", statusFilter);
            req.setAttribute("csrfToken", CsrfUtils.generateToken(req.getSession()));
            req.getRequestDispatcher("/WEB-INF/views/admin/members.jsp").forward(req, resp);
        } catch (Exception e) {
            req.setAttribute("error", "Không thể tải danh sách thành viên.");
            req.getRequestDispatcher("/WEB-INF/views/admin/members.jsp").forward(req, resp);
        }
    }

    @Override
    protected void doPost(HttpServletRequest req, HttpServletResponse resp)
            throws ServletException, IOException {
        String action = req.getParameter("action");
        try {
            if ("updateStatus".equals(action)) {
                int    memberId = Integer.parseInt(req.getParameter("memberId"));
                String status   = req.getParameter("status");
                memberDAO.updateStatus(memberId, status);
            } else if ("cloneTemplate".equals(action)) {
                // PROTOTYPE PATTERN — clone a member template for bulk registration
                int     templateId = Integer.parseInt(req.getParameter("templateId"));
                Member  template   = memberDAO.findById(templateId);
                if (template != null) {
                    Member newMember = template.clone(); // PROTOTYPE in action
                    newMember.setId(0);
                    // (Caller sets a new username/email before persisting)
                    req.getSession().setAttribute("memberTemplate", newMember);
                }
            }
            resp.sendRedirect(req.getContextPath() + "/admin/members");
        } catch (Exception e) {
            resp.sendRedirect(req.getContextPath() + "/admin/members?error=1");
        }
    }
}
