package com.sportsclub.servlet.member;

import com.sportsclub.dao.MemberDAO;
import com.sportsclub.dao.PackageDAO;
import com.sportsclub.dao.UserDAO;
import com.sportsclub.model.Member;
import com.sportsclub.model.User;
import com.sportsclub.util.BCryptUtil;
import com.sportsclub.util.CsrfUtils;
import jakarta.servlet.ServletException;
import jakarta.servlet.annotation.WebServlet;
import jakarta.servlet.http.*;
import java.io.IOException;

@WebServlet("/member/profile")
public class MemberProfileServlet extends HttpServlet {

    private final MemberDAO  memberDAO  = new MemberDAO();
    private final UserDAO    userDAO    = new UserDAO();
    private final PackageDAO packageDAO = new PackageDAO();

    @Override
    protected void doGet(HttpServletRequest req, HttpServletResponse resp)
            throws ServletException, IOException {
        try {
            User   loggedIn = (User) req.getSession().getAttribute("loggedInUser");
            Member member   = memberDAO.findByUserId(loggedIn.getId());
            req.setAttribute("member", member);
            req.setAttribute("packages", packageDAO.findActive());
            req.setAttribute("csrfToken", CsrfUtils.generateToken(req.getSession()));
            req.getRequestDispatcher("/WEB-INF/views/member/profile.jsp").forward(req, resp);
        } catch (Exception e) {
            req.setAttribute("error", "Không thể tải hồ sơ.");
            req.getRequestDispatcher("/WEB-INF/views/member/profile.jsp").forward(req, resp);
        }
    }

    @Override
    protected void doPost(HttpServletRequest req, HttpServletResponse resp)
            throws ServletException, IOException {
        try {
            User   loggedIn = (User) req.getSession().getAttribute("loggedInUser");
            Member member   = memberDAO.findByUserId(loggedIn.getId());

            String fullName = req.getParameter("fullName");
            if (isBlank(fullName)) {
                resp.sendRedirect(req.getContextPath() + "/member/profile?error=1");
                return;
            }

            member.setFullName(fullName.trim());
            member.setPhone(req.getParameter("phone"));
            member.setAddress(req.getParameter("address"));
            memberDAO.update(member);

            String newPassword = req.getParameter("newPassword");
            if (newPassword != null && !newPassword.trim().isEmpty()) {
                String hashed = BCryptUtil.hashPassword(newPassword);
                userDAO.updatePassword(loggedIn.getId(), hashed);
            }

            req.getSession().setAttribute("flash", "Cập nhật hồ sơ thành công.");
            resp.sendRedirect(req.getContextPath() + "/member/profile");
        } catch (Exception e) {
            resp.sendRedirect(req.getContextPath() + "/member/profile?error=1");
        }
    }

    private boolean isBlank(String s) {
        return s == null || s.trim().isEmpty();
    }
}
