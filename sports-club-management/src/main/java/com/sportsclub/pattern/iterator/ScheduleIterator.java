package com.sportsclub.pattern.iterator;

import com.sportsclub.model.Schedule;
import java.util.List;

// ITERATOR PATTERN — concrete iterator for Schedule collections
public class ScheduleIterator implements ClubIterator<Schedule> {
    private final List<Schedule> schedules;
    private int index = 0;

    public ScheduleIterator(List<Schedule> schedules) {
        this.schedules = schedules;
    }

    @Override public boolean  hasNext() { return index < schedules.size(); }
    @Override public Schedule next()    { return schedules.get(index++); }
    @Override public void     reset()   { index = 0; }
}
