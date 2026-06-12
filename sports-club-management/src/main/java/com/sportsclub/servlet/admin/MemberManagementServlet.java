package com.sportsclub.servlet.admin;

import com.sportsclub.dao.MemberDAO;
import com.sportsclub.dao.PackageDAO;
import com.sportsclub.dao.UserDAO;
import com.sportsclub.model.Member;
import com.sportsclub.pattern.iterator.ClubIterator;
import com.sportsclub.pattern.iterator.MemberCollection;
import com.sportsclub.util.BCryptUtil;
import com.sportsclub.util.CsrfUtils;
import com.sportsclub.util.PasswordPolicy;
import jakarta.servlet.ServletException;
import jakarta.servlet.annotation.WebServlet;
import jakarta.servlet.http.*;
import java.io.IOException;
import java.time.LocalDate;
import java.util.ArrayList;
import java.util.List;

@WebServlet("/admin/members")
public class MemberManagementServlet extends HttpServlet {

    private final MemberDAO  memberDAO  = new MemberDAO();
    private final PackageDAO packageDAO = new PackageDAO();
    private final UserDAO    userDAO    = new UserDAO();

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
            if ("add".equals(action)) {
                addMember(req, resp);
                return;
            } else if ("updateStatus".equals(action)) {
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

    /**
     * Creates a new member: inserts the user row (role MEMBER, BCrypt-hashed
     * password) then the linked member profile row.
     * Validation mirrors RegisterServlet — required fields, password length,
     * and uniqueness of username/email. Errors are signalled via redirect
     * query codes the JSP maps to localized messages.
     */
    private void addMember(HttpServletRequest req, HttpServletResponse resp)
            throws IOException, java.sql.SQLException {
        String username = req.getParameter("username");
        String email    = req.getParameter("email");
        String password = req.getParameter("password");
        String fullName = req.getParameter("fullName");
        String phone    = req.getParameter("phone");
        String gender   = req.getParameter("gender");
        String address  = req.getParameter("address");
        String dobStr   = req.getParameter("dateOfBirth");
        String expStr   = req.getParameter("expiryDate");
        String pkgStr   = req.getParameter("packageId");

        String ctx = req.getContextPath();
        if (isBlank(username) || isBlank(email) || isBlank(password) || isBlank(fullName)) {
            resp.sendRedirect(ctx + "/admin/members?error=fields");
            return;
        }
        if (!PasswordPolicy.isValid(password)) {
            resp.sendRedirect(ctx + "/admin/members?error=pwd");
            return;
        }
        username = username.trim();
        email    = email.trim();
        if (userDAO.findByUsername(username) != null) {
            resp.sendRedirect(ctx + "/admin/members?error=dupuser");
            return;
        }
        if (userDAO.findByEmail(email) != null) {
            resp.sendRedirect(ctx + "/admin/members?error=dupemail");
            return;
        }

        int       packageId = (pkgStr != null && !pkgStr.isEmpty()) ? Integer.parseInt(pkgStr) : 0;
        LocalDate dob        = (dobStr != null && !dobStr.isEmpty()) ? LocalDate.parse(dobStr) : null;
        LocalDate expiry     = (expStr != null && !expStr.isEmpty()) ? LocalDate.parse(expStr) : null;

        // BCrypt hash — never store plaintext passwords
        String hash   = BCryptUtil.hashPassword(password);
        int    userId = userDAO.insert(username, hash, email, phone, "MEMBER");
        if (userId > 0) {
            memberDAO.insert(userId, fullName.trim(), gender, dob, address, packageId, expiry);
        }
        resp.sendRedirect(ctx + "/admin/members");
    }

    private boolean isBlank(String s) {
        return s == null || s.trim().isEmpty();
    }
}
