package com.sportsclub.model;

import com.sportsclub.pattern.prototype.SportClubPrototype;

// PROTOTYPE PATTERN — TrainingClass is a cloneable domain object
public class TrainingClass implements SportClubPrototype, Cloneable {

    private int     id;
    private String  name;
    private int     coachId;
    private String  coachName;
    private int     capacity;
    private int     currentEnrolled;
    private String  level;     // BEGINNER / INTERMEDIATE / ADVANCED
    private String  description;
    private boolean isActive;

    public TrainingClass() {}

    // PROTOTYPE PATTERN — shallow clone creates a duplicate class template
    @Override
    public TrainingClass clone() {
        try { return (TrainingClass) super.clone(); }
        catch (CloneNotSupportedException e) { throw new RuntimeException(e); }
    }

    // Getters / Setters
    public int     getId()                      { return id; }
    public void    setId(int id)                { this.id = id; }
    public String  getName()                    { return name; }
    public void    setName(String name)         { this.name = name; }
    public int     getCoachId()                 { return coachId; }
    public void    setCoachId(int coachId)      { this.coachId = coachId; }
    public String  getCoachName()               { return coachName; }
    public void    setCoachName(String n)       { this.coachName = n; }
    public int     getCapacity()                { return capacity; }
    public void    setCapacity(int capacity)    { this.capacity = capacity; }
    public int     getCurrentEnrolled()         { return currentEnrolled; }
    public void    setCurrentEnrolled(int n)    { this.currentEnrolled = n; }
    public String  getLevel()                   { return level; }
    public void    setLevel(String level)       { this.level = level; }
    public String  getDescription()             { return description; }
    public void    setDescription(String d)     { this.description = d; }
    public boolean isActive()                   { return isActive; }
    public void    setActive(boolean active)    { this.isActive = active; }
    public int     getAvailableSlots()          { return capacity - currentEnrolled; }
}
