package com.sportsclub.pattern.iterator;

/**
 * ITERATOR PATTERN — generic collection interface that creates iterators.
 */
public interface ClubCollection<T> {
    ClubIterator<T> createIterator();
    void add(T item);
    int size();
}
