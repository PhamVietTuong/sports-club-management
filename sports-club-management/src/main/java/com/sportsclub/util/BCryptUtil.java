package com.sportsclub.util;

import org.mindrot.jbcrypt.BCrypt;

/**
 * SECURITY — BCrypt password hashing utility.
 * Cost factor 12 provides ~300ms hash time, making brute-force attacks expensive.
 */
public class BCryptUtil {

    private static final int WORK_FACTOR = 12;

    private BCryptUtil() {}

    // Hash a plain-text password — never store raw passwords
    public static String hashPassword(String plainText) {
        return BCrypt.hashpw(plainText, BCrypt.gensalt(WORK_FACTOR));
    }

    // Verify a plain-text password against a stored BCrypt hash
    public static boolean checkPassword(String plainText, String hashed) {
        return BCrypt.checkpw(plainText, hashed);
    }
}
