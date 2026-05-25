package com.sportsclub.model;

import com.sportsclub.pattern.prototype.SportClubPrototype;
import java.time.LocalTime;

// PROTOTYPE PATTERN — Schedule is a cloneable domain object
// Use case: clone this week's schedule → next week
public class Schedule implements SportClubPrototype, Cloneable {

    private int       id;
    private int       classId;
    private String    className;
    private String    dayOfWeek;
    private LocalTime startTime;
    private LocalTime endTime;
    private String    room;
    private boolean   repeatWeekly;

    public Schedule() {}

    // PROTOTYPE PATTERN — shallow clone duplicates the schedule (e.g., for next week)
    @Override
    public Schedule clone() {
        try { return (Schedule) super.clone(); }
        catch (CloneNotSupportedException e) { throw new RuntimeException(e); }
    }

    // Getters / Setters
    public int       getId()                       { return id; }
    public void      setId(int id)                 { this.id = id; }
    public int       getClassId()                  { return classId; }
    public void      setClassId(int classId)       { this.classId = classId; }
    public String    getClassName()                { return className; }
    public void      setClassName(String n)        { this.className = n; }
    public String    getDayOfWeek()                { return dayOfWeek; }
    public void      setDayOfWeek(String day)      { this.dayOfWeek = day; }
    public LocalTime getStartTime()                { return startTime; }
    public void      setStartTime(LocalTime t)     { this.startTime = t; }
    public LocalTime getEndTime()                  { return endTime; }
    public void      setEndTime(LocalTime t)       { this.endTime = t; }
    public String    getRoom()                     { return room; }
    public void      setRoom(String room)          { this.room = room; }
    public boolean   isRepeatWeekly()              { return repeatWeekly; }
    public void      setRepeatWeekly(boolean r)    { this.repeatWeekly = r; }
}
