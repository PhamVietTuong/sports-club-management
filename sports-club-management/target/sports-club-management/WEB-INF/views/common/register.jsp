<%@ page contentType="text/html;charset=UTF-8" pageEncoding="UTF-8" %>
<%@ taglib prefix="c" uri="http://java.sun.com/jsp/jstl/core" %>
<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Đăng ký — Sports Club</title>
    <link rel="stylesheet" href="${pageContext.request.contextPath}/assets/css/style.css">
</head>
<body>
<div class="register-shell">
    <div class="register-card">
        <div class="register-logo">
            <div class="register-monogram">SC</div>
            <div>
                <div class="register-title">Tạo tài khoản</div>
                <div class="register-subtitle">Gia nhập câu lạc bộ ngay hôm nay</div>
            </div>
        </div>

        <c:if test="${not empty error}">
            <div class="alert alert-danger"><c:out value="${error}"/></div>
        </c:if>

        <form method="post" action="${pageContext.request.contextPath}/register">
            <%-- CSRF PREVENTION --%>
            <input type="hidden" name="_csrf" value="${csrfToken}">

            <div class="form-grid-2">
                <div class="form-group">
                    <label class="form-label">Họ và tên <span class="req">*</span></label>
                    <input type="text" name="fullName" class="form-control"
                           required maxlength="100" placeholder="Nguyễn Văn A">
                </div>
                <div class="form-group">
                    <label class="form-label">Tên đăng nhập <span class="req">*</span></label>
                    <input type="text" name="username" class="form-control"
                           required maxlength="50" pattern="[A-Za-z0-9_]{3,50}"
                           placeholder="johndoe123">
                </div>
            </div>

            <div class="form-grid-2">
                <div class="form-group">
                    <label class="form-label">Email <span class="req">*</span></label>
                    <input type="email" name="email" class="form-control"
                           required maxlength="100" placeholder="ban@email.com">
                </div>
                <div class="form-group">
                    <label class="form-label">Số điện thoại</label>
                    <input type="tel" name="phone" class="form-control"
                           maxlength="20" placeholder="+1 555 000 0000">
                </div>
            </div>

            <div class="form-group">
                <label class="form-label">Giới tính</label>
                <select name="gender" class="form-select">
                    <option value="">Chọn...</option>
                    <option value="MALE">Nam</option>
                    <option value="FEMALE">Nữ</option>
                    <option value="OTHER">Khác</option>
                </select>
            </div>

            <div class="form-group">
                <label class="form-label">Mật khẩu <span class="req">*</span></label>
                <input type="password" name="password" class="form-control"
                       required minlength="8" placeholder="Tối thiểu 8 ký tự">
                <p class="form-hint">Tối thiểu 8 ký tự.</p>
            </div>

            <div class="form-group">
                <label class="form-label">Xác nhận mật khẩu <span class="req">*</span></label>
                <input type="password" name="confirmPassword" class="form-control"
                       required placeholder="Nhập lại mật khẩu">
            </div>

            <button type="submit" class="btn btn-primary w-100 mt-8px">
                Tạo tài khoản
            </button>
        </form>

        <hr class="divider">
        <p class="text-center text-muted fs-13">
            Đã có tài khoản?
            <a href="${pageContext.request.contextPath}/login">Đăng nhập</a>
        </p>
    </div>
</div>
</body>
</html>
