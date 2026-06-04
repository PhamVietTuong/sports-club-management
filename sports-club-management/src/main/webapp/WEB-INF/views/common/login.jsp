<%@ page contentType="text/html;charset=UTF-8" pageEncoding="UTF-8" %>
<%@ taglib prefix="c" uri="http://java.sun.com/jsp/jstl/core" %>
<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Đăng nhập — Sports Club</title>
    <link rel="stylesheet" href="${pageContext.request.contextPath}/assets/css/style.css">
</head>
<body>
<div class="login-shell">

    <!-- Left Brand Panel -->
    <div class="login-brand">
        <div class="login-brand-icon">🏆</div>
        <div class="login-brand-name">SPORTS CLUB</div>
        <p class="login-brand-tagline">Vượt qua giới hạn. Theo dõi tiến bộ.</p>
    </div>

    <!-- Right Form Area -->
    <div class="login-form-area">
        <div class="login-form-card">
            <h2>Chào mừng trở lại</h2>
            <p class="subtitle">Đăng nhập vào tài khoản của bạn</p>

            <%-- Display error message (XSS safe via c:out) --%>
            <c:if test="${not empty error}">
                <div class="alert alert-danger"><c:out value="${error}"/></div>
            </c:if>
            <c:if test="${param.registered eq 'true'}">
                <div class="alert alert-success">Đăng ký thành công! Vui lòng đăng nhập.</div>
            </c:if>

            <form method="post" action="${pageContext.request.contextPath}/login">
                <%-- CSRF PREVENTION — hidden token matched against session --%>
                <input type="hidden" name="_csrf" value="${csrfToken}">

                <div class="form-group">
                    <label class="form-label">Tên đăng nhập</label>
                    <input type="text" name="username" class="form-control"
                           required autocomplete="username" maxlength="50"
                           placeholder="Nhập tên đăng nhập">
                </div>
                <div class="form-group">
                    <label class="form-label">Mật khẩu</label>
                    <input type="password" name="password" class="form-control"
                           required autocomplete="current-password"
                           placeholder="Nhập mật khẩu">
                </div>
                <button type="submit" class="btn btn-primary w-100 mt-8px">Đăng nhập</button>
            </form>

            <hr class="divider">
            <p class="text-center text-muted fs-13">
                Chưa có tài khoản?
                <a href="${pageContext.request.contextPath}/register">Đăng ký</a>
            </p>
        </div>
    </div>

</div>
</body>
</html>
