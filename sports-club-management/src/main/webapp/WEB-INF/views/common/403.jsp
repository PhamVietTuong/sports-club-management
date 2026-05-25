<%@ page contentType="text/html;charset=UTF-8" %>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <title>403 — Access Denied</title>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css">
</head>
<body class="bg-light">
<div class="container text-center py-5">
    <h1 class="display-1 text-danger">403</h1>
    <h3>Access Denied</h3>
    <p class="text-muted">You do not have permission to access this page.</p>
    <a href="${pageContext.request.contextPath}/login" class="btn btn-primary">Back to Login</a>
</div>
</body>
</html>
