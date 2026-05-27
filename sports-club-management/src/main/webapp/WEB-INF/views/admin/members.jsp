<%@ page contentType="text/html;charset=UTF-8" %>
<%@ taglib prefix="c" uri="http://java.sun.com/jsp/jstl/core" %>
<%@ taglib prefix="fmt" uri="http://java.sun.com/jsp/jstl/fmt" %>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Members — Admin</title>
    <link rel="stylesheet" href="${pageContext.request.contextPath}/assets/css/style.css">
</head>
<body>
<c:set var="activeNav" value="members"/>
<div class="app-shell">
    <%@ include file="navbar.jsp" %>
    <div class="main-content">
        <header class="top-bar">
            <span class="top-bar-title">Members</span>
            <div class="top-bar-user">
                <span><c:out value="${sessionScope.loggedInUser.username}"/> (Admin)</span>
                <div class="user-avatar">
                    ${sessionScope.loggedInUser.username.substring(0,1).toUpperCase()}
                </div>
            </div>
        </header>

        <div class="page-body">
            <div class="page-header">
                <div>
                    <div class="page-title">Members</div>
                    <div class="page-subtitle">Manage member accounts and membership status</div>
                </div>
                <div class="filter-tabs">
                    <a href="${pageContext.request.contextPath}/admin/members"
                       class="filter-tab ${empty statusFilter ? 'active' : ''}">All</a>
                    <a href="${pageContext.request.contextPath}/admin/members?status=ACTIVE"
                       class="filter-tab ${statusFilter eq 'ACTIVE' ? 'active' : ''}">Active</a>
                    <a href="${pageContext.request.contextPath}/admin/members?status=INACTIVE"
                       class="filter-tab ${statusFilter eq 'INACTIVE' ? 'active' : ''}">Inactive</a>
                    <a href="${pageContext.request.contextPath}/admin/members?status=SUSPENDED"
                       class="filter-tab ${statusFilter eq 'SUSPENDED' ? 'active' : ''}">Suspended</a>
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
                            <th>Full Name</th>
                            <th>Username</th>
                            <th>Email</th>
                            <th>Gender</th>
                            <th>Join Date</th>
                            <th>Expiry</th>
                            <th>Status</th>
                            <th>Action</th>
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
                                        <span class="badge badge-success">ACTIVE</span>
                                    </c:when>
                                    <c:when test="${m.status eq 'SUSPENDED'}">
                                        <span class="badge badge-danger">SUSPENDED</span>
                                    </c:when>
                                    <c:otherwise>
                                        <span class="badge badge-warning">INACTIVE</span>
                                    </c:otherwise>
                                </c:choose>
                            </td>
                            <td>
                                <form method="post"
                                      action="${pageContext.request.contextPath}/admin/members"
                                      style="display:inline-flex;align-items:center;gap:6px;">
                                    <input type="hidden" name="_csrf"    value="${csrfToken}">
                                    <input type="hidden" name="action"   value="updateStatus">
                                    <input type="hidden" name="memberId" value="${m.id}">
                                    <select name="status" class="form-select"
                                            style="width:auto;padding:4px 28px 4px 8px;font-size:12px;">
                                        <option value="ACTIVE">Active</option>
                                        <option value="INACTIVE">Inactive</option>
                                        <option value="SUSPENDED">Suspended</option>
                                    </select>
                                    <button type="submit" class="btn btn-primary btn-sm">Update</button>
                                </form>
                            </td>
                        </tr>
                        </c:forEach>
                        <c:if test="${empty members}">
                            <tr>
                                <td colspan="9" class="text-center text-muted"
                                    style="padding:40px 14px;">No members found.</td>
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
