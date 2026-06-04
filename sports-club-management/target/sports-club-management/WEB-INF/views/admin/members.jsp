<%@ page contentType="text/html;charset=UTF-8" pageEncoding="UTF-8" %>
<%@ taglib prefix="c" uri="http://java.sun.com/jsp/jstl/core" %>
<%@ taglib prefix="fmt" uri="http://java.sun.com/jsp/jstl/fmt" %>
<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Thành viên — Quản trị</title>
    <link rel="stylesheet" href="${pageContext.request.contextPath}/assets/css/style.css?v=2">
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
                <div class="flex-center-gap10">
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
                    <button class="btn btn-primary" data-bs-toggle="modal" data-bs-target="#addMemberModal">
                        <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                            <line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/>
                        </svg>
                        Thêm thành viên
                    </button>
                </div>
            </div>

            <c:if test="${not empty error}">
                <div class="alert alert-danger"><c:out value="${error}"/></div>
            </c:if>
            <c:if test="${not empty param.error}">
                <div class="alert alert-danger">
                    <c:choose>
                        <c:when test="${param.error eq 'fields'}">Vui lòng điền đầy đủ các trường bắt buộc.</c:when>
                        <c:when test="${param.error eq 'pwd'}">Mật khẩu phải có ít nhất 8 ký tự.</c:when>
                        <c:when test="${param.error eq 'dupuser'}">Tên đăng nhập đã được sử dụng.</c:when>
                        <c:when test="${param.error eq 'dupemail'}">Email đã được đăng ký.</c:when>
                        <c:otherwise>Không thể thêm thành viên. Vui lòng thử lại.</c:otherwise>
                    </c:choose>
                </div>
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

<!-- Add Member Modal -->
<div class="modal fade" id="addMemberModal" tabindex="-1">
    <div class="modal-dialog modal-lg">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title">Thêm thành viên mới</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <form method="post" action="${pageContext.request.contextPath}/admin/members">
                <input type="hidden" name="_csrf"  value="${csrfToken}">
                <input type="hidden" name="action" value="add">
                <div class="modal-body">
                    <div class="form-grid-2">
                        <div class="form-group">
                            <label class="form-label">Tên đăng nhập <span class="req">*</span></label>
                            <input type="text" name="username" class="form-control" required maxlength="50" placeholder="vd: member_jane">
                        </div>
                        <div class="form-group">
                            <label class="form-label">Họ và tên <span class="req">*</span></label>
                            <input type="text" name="fullName" class="form-control" required maxlength="100" placeholder="Tên hiển thị đầy đủ">
                        </div>
                        <div class="form-group">
                            <label class="form-label">Email <span class="req">*</span></label>
                            <input type="email" name="email" class="form-control" required placeholder="member@example.com">
                        </div>
                        <div class="form-group">
                            <label class="form-label">Số điện thoại</label>
                            <input type="text" name="phone" class="form-control" maxlength="20" placeholder="+84 555 000 0000">
                        </div>
                        <div class="form-group">
                            <label class="form-label">Mật khẩu <span class="req">*</span></label>
                            <input type="password" name="password" class="form-control" required minlength="8" placeholder="Tối thiểu 8 ký tự">
                        </div>
                        <div class="form-group">
                            <label class="form-label">Giới tính</label>
                            <select name="gender" class="form-select">
                                <option value="">— Chọn —</option>
                                <option value="MALE">Nam</option>
                                <option value="FEMALE">Nữ</option>
                                <option value="OTHER">Khác</option>
                            </select>
                        </div>
                        <div class="form-group">
                            <label class="form-label">Ngày sinh</label>
                            <input type="date" name="dateOfBirth" class="form-control">
                        </div>
                        <div class="form-group">
                            <label class="form-label">Gói tập</label>
                            <select name="packageId" class="form-select">
                                <option value="0">— Không có —</option>
                                <c:forEach var="p" items="${packages}">
                                    <option value="${p.id}"><c:out value="${p.name}"/></option>
                                </c:forEach>
                            </select>
                        </div>
                        <div class="form-group">
                            <label class="form-label">Ngày hết hạn</label>
                            <input type="date" name="expiryDate" class="form-control">
                        </div>
                    </div>
                    <div class="form-group">
                        <label class="form-label">Địa chỉ</label>
                        <input type="text" name="address" class="form-control" maxlength="255" placeholder="Địa chỉ liên hệ">
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-ghost" data-bs-dismiss="modal">Hủy</button>
                    <button type="submit" class="btn btn-primary">Lưu thành viên</button>
                </div>
            </form>
        </div>
    </div>
</div>

<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>
