<%@ page contentType="text/html;charset=UTF-8" %>
<%@ taglib prefix="c" uri="http://java.sun.com/jsp/jstl/core" %>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Login — Sports Club</title>
    <link rel="stylesheet" href="${pageContext.request.contextPath}/assets/css/style.css">
</head>
<body>
<div class="login-shell">

    <!-- Left Brand Panel -->
    <div class="login-brand">
        <div class="login-brand-icon">🏆</div>
        <div class="login-brand-name">SPORTS CLUB</div>
        <p class="login-brand-tagline">Push your limits. Track your progress.</p>
    </div>

    <!-- Right Form Area -->
    <div class="login-form-area">
        <div class="login-form-card">
            <h2>Welcome Back</h2>
            <p class="subtitle">Sign in to your account</p>

            <%-- Display error message (XSS safe via c:out) --%>
            <c:if test="${not empty error}">
                <div class="alert alert-danger"><c:out value="${error}"/></div>
            </c:if>
            <c:if test="${param.registered eq 'true'}">
                <div class="alert alert-success">Registration successful! Please log in.</div>
            </c:if>

            <form method="post" action="${pageContext.request.contextPath}/login">
                <%-- CSRF PREVENTION — hidden token matched against session --%>
                <input type="hidden" name="_csrf" value="${csrfToken}">

                <div class="form-group">
                    <label class="form-label">Username</label>
                    <input type="text" name="username" class="form-control"
                           required autocomplete="username" maxlength="50"
                           placeholder="Enter your username">
                </div>
                <div class="form-group">
                    <label class="form-label">Password</label>
                    <input type="password" name="password" class="form-control"
                           required autocomplete="current-password"
                           placeholder="Enter your password">
                </div>
                <button type="submit" class="btn btn-primary w-100" style="margin-top:8px;">Sign In</button>
            </form>

            <hr class="divider">
            <p class="text-center text-muted" style="font-size:13px;">
                Don't have an account?
                <a href="${pageContext.request.contextPath}/register">Register</a>
            </p>
        </div>
    </div>

</div>
</body>
</html>
