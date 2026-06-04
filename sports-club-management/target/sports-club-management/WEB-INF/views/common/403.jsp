<%@ page contentType="text/html;charset=UTF-8" pageEncoding="UTF-8" %>
<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="UTF-8">
    <title>403 — Truy cập bị từ chối</title>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css">
</head>
<body class="bg-light">
<div class="container text-center py-5">
    <h1 class="display-1 text-danger">403</h1>
    <h3>Truy cập bị từ chối</h3>
    <p class="text-muted">Bạn không có quyền truy cập trang này.</p>
    <a href="${pageContext.request.contextPath}/login" class="btn btn-primary">Quay lại đăng nhập</a>
</div>
</body>
</html>
