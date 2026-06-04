<%@ page contentType="text/html;charset=UTF-8" pageEncoding="UTF-8" %>
<%@ taglib prefix="c" uri="http://java.sun.com/jsp/jstl/core" %>
<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Huấn luyện viên — Quản trị</title>
    <link rel="stylesheet" href="${pageContext.request.contextPath}/assets/css/style.css">
</head>
<body>
<c:set var="activeNav" value="coaches"/>
<div class="app-shell">
    <%@ include file="navbar.jsp" %>
    <div class="main-content">

        <header class="top-bar">
            <span class="top-bar-title">Huấn luyện viên</span>
            <div class="top-bar-user">
                <span><c:out value="${sessionScope.loggedInUser.username}"/> (Admin)</span>
                <div class="user-avatar">
                    <c:out value="${not empty sessionScope.loggedInUser.username ? sessionScope.loggedInUser.username.substring(0,1).toUpperCase() : '?'}"/>
                </div>
            </div>
        </header>

        <div class="page-body">

            <c:if test="${not empty error}">
                <div class="alert alert-danger"><c:out value="${error}"/></div>
            </c:if>

            <div class="page-header">
                <div>
                    <div class="page-title">Huấn luyện viên</div>
                    <div class="page-subtitle">Quản lý đội ngũ huấn luyện viên và phân công</div>
                </div>
                <button class="btn btn-primary" data-bs-toggle="modal" data-bs-target="#addCoachModal">
                    <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/>
                    </svg>
                    Thêm huấn luyện viên
                </button>
            </div>

            <div class="table-wrap">
                <table class="data-table">
                    <thead>
                        <tr>
                            <th>ID</th>
                            <th>Họ và tên</th>
                            <th>Email</th>
                            <th>Chuyên môn</th>
                            <th>Kinh nghiệm</th>
                            <th>Lương</th>
                        </tr>
                    </thead>
                    <tbody>
                        <c:forEach var="c" items="${coaches}">
                        <tr>
                            <td class="text-muted small"><c:out value="${c.id}"/></td>
                            <td>
                                <div class="flex-center-gap10">
                                    <div class="table-avatar">
                                        <c:out value="${not empty c.fullName ? c.fullName.substring(0,1).toUpperCase() : '?'}"/>
                                    </div>
                                    <span class="fw-bold"><c:out value="${c.fullName}"/></span>
                                </div>
                            </td>
                            <td class="text-muted"><c:out value="${c.email}"/></td>
                            <td>
                                <c:if test="${not empty c.specialization}">
                                    <span class="badge badge-info"><c:out value="${c.specialization}"/></span>
                                </c:if>
                                <c:if test="${empty c.specialization}">
                                    <span class="text-muted small">—</span>
                                </c:if>
                            </td>
                            <td>
                                <span class="text-default">
                                    <c:out value="${c.experience}"/>
                                </span>
                                <span class="text-muted small"> năm</span>
                            </td>
                            <td>
                                <span class="text-success fw-600">
                                    $<c:out value="${c.salary}"/>
                                </span>
                            </td>
                        </tr>
                        </c:forEach>
                        <c:if test="${empty coaches}">
                            <tr>
                                <td colspan="6" class="text-center text-muted empty-pad-36">
                                    Không tìm thấy huấn luyện viên nào.
                                </td>
                            </tr>
                        </c:if>
                    </tbody>
                </table>
            </div>

        </div>
    </div>
</div>

<!-- Add Coach Modal -->
<div class="modal fade" id="addCoachModal" tabindex="-1">
    <div class="modal-dialog modal-lg">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title">Thêm huấn luyện viên mới</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <form method="post" action="${pageContext.request.contextPath}/admin/coaches">
                <input type="hidden" name="_csrf"  value="${csrfToken}">
                <input type="hidden" name="action" value="add">
                <div class="modal-body">
                    <div class="form-grid-2">
                        <div class="form-group">
                            <label class="form-label">Tên đăng nhập <span class="req">*</span></label>
                            <input type="text" name="username" class="form-control" required maxlength="50" placeholder="vd: coach_john">
                        </div>
                        <div class="form-group">
                            <label class="form-label">Họ và tên <span class="req">*</span></label>
                            <input type="text" name="fullName" class="form-control" required maxlength="100" placeholder="Tên hiển thị đầy đủ">
                        </div>
                        <div class="form-group">
                            <label class="form-label">Email <span class="req">*</span></label>
                            <input type="email" name="email" class="form-control" required placeholder="coach@example.com">
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
                            <label class="form-label">Chuyên môn</label>
                            <input type="text" name="specialization" class="form-control" maxlength="100" placeholder="vd: Sức mạnh & Thể lực">
                        </div>
                        <div class="form-group">
                            <label class="form-label">Kinh nghiệm (năm)</label>
                            <input type="number" name="experience" class="form-control" min="0" value="0">
                        </div>
                        <div class="form-group">
                            <label class="form-label">Lương ($)</label>
                            <input type="number" name="salary" class="form-control" step="0.01" min="0" value="0">
                        </div>
                    </div>
                    <div class="form-group">
                        <label class="form-label">Giới thiệu</label>
                        <textarea name="bio" class="form-control" rows="3" placeholder="Tiểu sử ngắn về huấn luyện viên..."></textarea>
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-ghost" data-bs-dismiss="modal">Hủy</button>
                    <button type="submit" class="btn btn-primary">Lưu huấn luyện viên</button>
                </div>
            </form>
        </div>
    </div>
</div>

<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>
