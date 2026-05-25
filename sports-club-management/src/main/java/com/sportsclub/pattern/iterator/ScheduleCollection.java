package com.sportsclub.pattern.iterator;

import com.sportsclub.model.Schedule;
import java.util.ArrayList;
import java.util.List;

// ITERATOR PATTERN — concrete collection for Schedule objects
public class ScheduleCollection implements ClubCollection<Schedule> {
    private final List<Schedule> schedules = new ArrayList<>();

    @Override public void add(Schedule s) { schedules.add(s); }
    @Override public int  size()          { return schedules.size(); }

    @Override
    public ClubIterator<Schedule> createIterator() {
        return new ScheduleIterator(schedules);
    }
}
