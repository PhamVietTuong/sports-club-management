<%@ page contentType="text/html;charset=UTF-8" %>
<%@ taglib prefix="c" uri="http://java.sun.com/jsp/jstl/core" %>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Register — Sports Club</title>
    <link rel="stylesheet" href="${pageContext.request.contextPath}/assets/css/style.css">
    <style>
        .register-shell {
            min-height: 100vh;
            display: flex;
            align-items: flex-start;
            justify-content: center;
            background: var(--bg);
            padding: 40px 20px;
        }
        .register-card {
            width: 100%;
            max-width: 560px;
            background: var(--surface);
            border: 1px solid var(--border);
            border-radius: 10px;
            padding: 36px 36px 28px;
        }
        .register-logo {
            display: flex;
            align-items: center;
            gap: 10px;
            margin-bottom: 24px;
        }
        .register-monogram {
            width: 36px;
            height: 36px;
            background: var(--primary);
            color: #fff;
            font-family: 'Syne', sans-serif;
            font-weight: 800;
            font-size: 14px;
            border-radius: 8px;
            display: flex;
            align-items: center;
            justify-content: center;
        }
        .register-title {
            font-family: 'Syne', sans-serif;
            font-size: 22px;
            font-weight: 800;
            color: var(--text);
        }
        .register-subtitle {
            font-size: 13px;
            color: var(--muted);
            margin-top: 2px;
        }
    </style>
</head>
<body>
<div class="register-shell">
    <div class="register-card">
        <div class="register-logo">
            <div class="register-monogram">SC</div>
            <div>
                <div class="register-title">Create Account</div>
                <div class="register-subtitle">Join the club today</div>
            </div>
        </div>

        <c:if test="${not empty error}">
            <div class="alert alert-danger"><c:out value="${error}"/></div>
        </c:if>

        <form method="post" action="${pageContext.request.contextPath}/register">
            <%-- CSRF PREVENTION --%>
            <input type="hidden" name="_csrf" value="${csrfToken}">

            <div class="form-grid-2">
                <div class="form-group">
                    <label class="form-label">Full Name <span class="req">*</span></label>
                    <input type="text" name="fullName" class="form-control"
                           required maxlength="100" placeholder="John Doe">
                </div>
                <div class="form-group">
                    <label class="form-label">Username <span class="req">*</span></label>
                    <input type="text" name="username" class="form-control"
                           required maxlength="50" pattern="[A-Za-z0-9_]{3,50}"
                           placeholder="johndoe123">
                </div>
            </div>

            <div class="form-grid-2">
                <div class="form-group">
                    <label class="form-label">Email <span class="req">*</span></label>
                    <input type="email" name="email" class="form-control"
                           required maxlength="100" placeholder="you@email.com">
                </div>
                <div class="form-group">
                    <label class="form-label">Phone</label>
                    <input type="tel" name="phone" class="form-control"
                           maxlength="20" placeholder="+1 555 000 0000">
                </div>
            </div>

            <div class="form-group">
                <label class="form-label">Gender</label>
                <select name="gender" class="form-select">
                    <option value="">Select...</option>
                    <option value="MALE">Male</option>
                    <option value="FEMALE">Female</option>
                    <option value="OTHER">Other</option>
                </select>
            </div>

            <div class="form-group">
                <label class="form-label">Password <span class="req">*</span></label>
                <input type="password" name="password" class="form-control"
                       required minlength="8" placeholder="Minimum 8 characters">
                <p class="form-hint">Minimum 8 characters.</p>
            </div>

            <div class="form-group">
                <label class="form-label">Confirm Password <span class="req">*</span></label>
                <input type="password" name="confirmPassword" class="form-control"
                       required placeholder="Re-enter password">
            </div>

            <button type="submit" class="btn btn-primary w-100" style="margin-top:8px;">
                Create Account
            </button>
        </form>

        <hr class="divider">
        <p class="text-center text-muted" style="font-size:13px;">
            Already have an account?
            <a href="${pageContext.request.contextPath}/login">Sign In</a>
        </p>
    </div>
</div>
</body>
</html>
