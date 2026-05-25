package com.sportsclub.pattern.prototype;

/**
 * PROTOTYPE PATTERN — interface for all cloneable domain objects.
 * Allows cloning objects without depending on their concrete classes.
 */
public interface SportClubPrototype {
    SportClubPrototype clone();
}
