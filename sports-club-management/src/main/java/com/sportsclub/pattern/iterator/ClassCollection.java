package com.sportsclub.pattern.iterator;

import com.sportsclub.model.TrainingClass;
import java.util.ArrayList;
import java.util.List;

// ITERATOR PATTERN — concrete collection for TrainingClass objects
public class ClassCollection implements ClubCollection<TrainingClass> {
    private final List<TrainingClass> classes = new ArrayList<>();

    @Override public void add(TrainingClass c) { classes.add(c); }
    @Override public int  size()               { return classes.size(); }

    @Override
    public ClubIterator<TrainingClass> createIterator() {
        return new ClassIterator(classes);
    }
}
