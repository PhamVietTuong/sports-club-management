<%@ page contentType="text/html;charset=UTF-8" %>
<%@ taglib prefix="c" uri="http://java.sun.com/jsp/jstl/core" %>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Login — Sports Club</title>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css">
    <link rel="stylesheet" href="${pageContext.request.contextPath}/assets/css/style.css">
</head>
<body class="bg-light">
<div class="container d-flex justify-content-center align-items-center min-vh-100">
    <div class="card shadow-sm" style="width:420px">
        <div class="card-body p-4">
            <h4 class="card-title text-center mb-4 text-primary fw-bold">
                <i class="bi bi-trophy-fill"></i> Sports Club
            </h4>
            <h5 class="text-center mb-4">Sign In</h5>

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

                <div class="mb-3">
                    <label class="form-label">Username</label>
                    <input type="text" name="username" class="form-control"
                           required autocomplete="username" maxlength="50">
                </div>
                <div class="mb-3">
                    <label class="form-label">Password</label>
                    <input type="password" name="password" class="form-control"
                           required autocomplete="current-password">
                </div>
                <button type="submit" class="btn btn-primary w-100">Login</button>
            </form>

            <hr>
            <div class="text-center">
                <a href="${pageContext.request.contextPath}/register">Don't have an account? Register</a>
            </div>
        </div>
    </div>
</div>
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>
