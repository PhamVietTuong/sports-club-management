package com.sportsclub.util;

/**
 * SECURITY — XSS prevention.
 * Encodes all user-supplied data before rendering in HTML to neutralise
 * script injection (<script>, event handlers, etc.).
 */
public class HtmlUtils {

    private HtmlUtils() {}

    public static String escapeHtml(String input) {
        if (input == null) return "";
        return input
            .replace("&",  "&amp;")
            .replace("<",  "&lt;")
            .replace(">",  "&gt;")
            .replace("\"", "&quot;")
            .replace("'",  "&#x27;")
            .replace("/",  "&#x2F;");
    }
}
