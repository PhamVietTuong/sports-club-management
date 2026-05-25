package com.sportsclub.pattern.iterator;

/**
 * ITERATOR PATTERN — generic iterator interface for club collections.
 * Allows sequential traversal without exposing the internal structure.
 */
public interface ClubIterator<T> {
    boolean hasNext();
    T next();
    default void reset() {}
}
