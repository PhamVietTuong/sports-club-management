package com.sportsclub.util;

/**
 * SECURITY — central password strength policy.
 * Requires a minimum length plus at least one letter and one digit,
 * so trivially weak passwords (e.g. "12345678" or "password") are rejected.
 */
public class PasswordPolicy {

    public static final int MIN_LENGTH = 8;

    private PasswordPolicy() {}

    /**
     * @return null if the password is acceptable, otherwise a localized
     *         message describing why it was rejected.
     */
    public static String validate(String password) {
        if (password == null || password.length() < MIN_LENGTH) {
            return "Mật khẩu phải có ít nhất " + MIN_LENGTH + " ký tự.";
        }
        if (!password.matches(".*[A-Za-z].*")) {
            return "Mật khẩu phải chứa ít nhất một chữ cái.";
        }
        if (!password.matches(".*\\d.*")) {
            return "Mật khẩu phải chứa ít nhất một chữ số.";
        }
        return null;
    }

    public static boolean isValid(String password) {
        return validate(password) == null;
    }
}
