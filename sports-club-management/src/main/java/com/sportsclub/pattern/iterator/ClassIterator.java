package com.sportsclub.pattern.iterator;

import com.sportsclub.model.TrainingClass;
import java.util.List;

// ITERATOR PATTERN — concrete iterator for TrainingClass collections
public class ClassIterator implements ClubIterator<TrainingClass> {
    private final List<TrainingClass> classes;
    private int index = 0;

    public ClassIterator(List<TrainingClass> classes) {
        this.classes = classes;
    }

    @Override public boolean       hasNext() { return index < classes.size(); }
    @Override public TrainingClass next()    { return classes.get(index++); }
    @Override public void          reset()   { index = 0; }
}
