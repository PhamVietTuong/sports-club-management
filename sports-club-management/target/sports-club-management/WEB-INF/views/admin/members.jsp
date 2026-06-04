<%@ page contentType="text/html;charset=UTF-8" pageEncoding="UTF-8" %>
<%@ taglib prefix="c" uri="http://java.sun.com/jsp/jstl/core" %>
<%@ taglib prefix="fmt" uri="http://java.sun.com/jsp/jstl/fmt" %>
<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Thành viên — Quản trị</title>
    <link rel="stylesheet" href="${pageContext.request.contextPath}/assets/css/style.css">
</head>
<body>
<c:set var="activeNav" value="members"/>
<div class="app-shell">
    <%@ include file="navbar.jsp" %>
    <div class="main-content">
        <header class="top-bar">
            <span class="top-bar-title">Thành viên</span>
            <div class="top-bar-user">
                <span><c:out value="${sessionScope.loggedInUser.username}"/> (Admin)</span>
                <div class="user-avatar">
                    <c:out value="${not empty sessionScope.loggedInUser.username ? sessionScope.loggedInUser.username.substring(0,1).toUpperCase() : '?'}"/>
                </div>
            </div>
        </header>

        <div class="page-body">
            <div class="page-header">
                <div>
                    <div class="page-title">Thành viên</div>
                    <div class="page-subtitle">Quản lý tài khoản và trạng thái thành viên</div>
                </div>
                <div class="filter-tabs">
                    <a href="${pageContext.request.contextPath}/admin/members"
                       class="filter-tab ${empty statusFilter ? 'active' : ''}">Tất cả</a>
                    <a href="${pageContext.request.contextPath}/admin/members?status=ACTIVE"
                       class="filter-tab ${statusFilter eq 'ACTIVE' ? 'active' : ''}">Hoạt động</a>
                    <a href="${pageContext.request.contextPath}/admin/members?status=INACTIVE"
                       class="filter-tab ${statusFilter eq 'INACTIVE' ? 'active' : ''}">Ngừng hoạt động</a>
                    <a href="${pageContext.request.contextPath}/admin/members?status=SUSPENDED"
                       class="filter-tab ${statusFilter eq 'SUSPENDED' ? 'active' : ''}">Tạm khóa</a>
                </div>
            </div>

            <c:if test="${not empty error}">
                <div class="alert alert-danger"><c:out value="${error}"/></div>
            </c:if>

            <div class="table-wrap">
                <table class="data-table">
                    <thead>
                        <tr>
                            <th>ID</th>
                            <th>Họ và tên</th>
                            <th>Tên đăng nhập</th>
                            <th>Email</th>
                            <th>Giới tính</th>
                            <th>Ngày tham gia</th>
                            <th>Ngày hết hạn</th>
                            <th>Trạng thái</th>
                            <th>Thao tác</th>
                        </tr>
                    </thead>
                    <tbody>
                        <c:forEach var="m" items="${members}">
                        <tr>
                            <td><c:out value="${m.id}"/></td>
                            <%-- XSS PREVENTION — c:out escapes HTML entities --%>
                            <td><c:out value="${m.fullName}"/></td>
                            <td><c:out value="${m.username}"/></td>
                            <td><c:out value="${m.email}"/></td>
                            <td><c:out value="${m.gender}"/></td>
                            <td><c:out value="${m.joinDate}"/></td>
                            <td><c:out value="${m.expiryDate}"/></td>
                            <td>
                                <c:choose>
                                    <c:when test="${m.status eq 'ACTIVE'}">
                                        <span class="badge badge-success">HOẠT ĐỘNG</span>
                                    </c:when>
                                    <c:when test="${m.status eq 'SUSPENDED'}">
                                        <span class="badge badge-danger">TẠM KHÓA</span>
                                    </c:when>
                                    <c:otherwise>
                                        <span class="badge badge-warning">NGỪNG HOẠT ĐỘNG</span>
                                    </c:otherwise>
                                </c:choose>
                            </td>
                            <td>
                                <form method="post"
                                      action="${pageContext.request.contextPath}/admin/members"
                                      class="inline-actions">
                                    <input type="hidden" name="_csrf"    value="${csrfToken}">
                                    <input type="hidden" name="action"   value="updateStatus">
                                    <input type="hidden" name="memberId" value="${m.id}">
                                    <select name="status" class="form-select select-inline">
                                        <option value="ACTIVE">Hoạt động</option>
                                        <option value="INACTIVE">Ngừng hoạt động</option>
                                        <option value="SUSPENDED">Tạm khóa</option>
                                    </select>
                                    <button type="submit" class="btn btn-primary btn-sm">Cập nhật</button>
                                </form>
                            </td>
                        </tr>
                        </c:forEach>
                        <c:if test="${empty members}">
                            <tr>
                                <td colspan="9" class="text-center text-muted empty-pad-40">Không tìm thấy thành viên nào.</td>
                            </tr>
                        </c:if>
                    </tbody>
                </table>
            </div>
        </div>
    </div>
</div>
</body>
</html>
