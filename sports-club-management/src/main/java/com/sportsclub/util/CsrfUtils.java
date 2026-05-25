package com.sportsclub.util;

import jakarta.servlet.http.HttpSession;
import java.util.UUID;

/**
 * SECURITY — CSRF token management.
 * A unique UUID token is bound to the user's session.
 * Every POST request must include this token; mismatches are rejected.
 */
public class CsrfUtils {

    public static final String CSRF_TOKEN_KEY = "_csrfToken";

    private CsrfUtils() {}

    // Generate a new token and store it in the session
    public static String generateToken(HttpSession session) {
        String token = UUID.randomUUID().toString();
        session.setAttribute(CSRF_TOKEN_KEY, token);
        return token;
    }

    // Validate that the submitted token matches the session token
    public static boolean isValidToken(HttpSession session, String submittedToken) {
        if (session == null || submittedToken == null) return false;
        String sessionToken = (String) session.getAttribute(CSRF_TOKEN_KEY);
        return submittedToken.equals(sessionToken);
    }
}
