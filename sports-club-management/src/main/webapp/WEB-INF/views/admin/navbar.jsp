<%@ taglib prefix="c" uri="http://java.sun.com/jsp/jstl/core" %>
<nav class="navbar navbar-expand-lg navbar-dark bg-dark">
    <div class="container">
        <a class="navbar-brand fw-bold" href="${pageContext.request.contextPath}/admin/dashboard">
            Sports Club Admin
        </a>
        <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navAdmin">
            <span class="navbar-toggler-icon"></span>
        </button>
        <div class="collapse navbar-collapse" id="navAdmin">
            <ul class="navbar-nav me-auto">
                <li class="nav-item">
                    <a class="nav-link" href="${pageContext.request.contextPath}/admin/members">Members</a>
                </li>
                <li class="nav-item">
                    <a class="nav-link" href="${pageContext.request.contextPath}/admin/coaches">Coaches</a>
                </li>
                <li class="nav-item">
                    <a class="nav-link" href="${pageContext.request.contextPath}/admin/classes">Classes</a>
                </li>
                <li class="nav-item">
                    <a class="nav-link" href="${pageContext.request.contextPath}/admin/schedules">Schedules</a>
                </li>
                <li class="nav-item">
                    <a class="nav-link" href="${pageContext.request.contextPath}/admin/packages">Packages</a>
                </li>
            </ul>
            <ul class="navbar-nav">
                <li class="nav-item">
                    <span class="nav-link text-light">
                        <c:out value="${sessionScope.loggedInUser.username}"/> (Admin)
                    </span>
                </li>
                <li class="nav-item">
                    <form method="post" action="${pageContext.request.contextPath}/logout" class="d-inline">
                        <input type="hidden" name="_csrf" value="${sessionScope.csrfToken}">
                        <button type="submit" class="btn btn-sm btn-outline-light ms-2">Logout</button>
                    </form>
                </li>
            </ul>
        </div>
    </div>
</nav>
