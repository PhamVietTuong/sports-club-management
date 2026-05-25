<%@ page contentType="text/html;charset=UTF-8" %>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <title>404 — Not Found</title>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css">
</head>
<body class="bg-light">
<div class="container text-center py-5">
    <h1 class="display-1 text-secondary">404</h1>
    <h3>Page Not Found</h3>
    <p class="text-muted">The page you are looking for does not exist.</p>
    <a href="${pageContext.request.contextPath}/login" class="btn btn-primary">Back to Login</a>
</div>
</body>
</html>
