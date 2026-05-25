package com.sportsclub.model;

import com.sportsclub.pattern.prototype.SportClubPrototype;

// PROTOTYPE PATTERN — TrainingPackage is a cloneable domain object
// Use case: clone a package template to create a new similar package
public class TrainingPackage implements SportClubPrototype, Cloneable {

    private int     id;
    private String  name;
    private int     durationMonths;
    private double  price;
    private int     maxClasses;
    private String  description;
    private boolean isActive;

    public TrainingPackage() {}

    // PROTOTYPE PATTERN — shallow clone creates a copy of this package template
    @Override
    public TrainingPackage clone() {
        try { return (TrainingPackage) super.clone(); }
        catch (CloneNotSupportedException e) { throw new RuntimeException(e); }
    }

    // Getters / Setters
    public int     getId()                         { return id; }
    public void    setId(int id)                   { this.id = id; }
    public String  getName()                       { return name; }
    public void    setName(String name)            { this.name = name; }
    public int     getDurationMonths()             { return durationMonths; }
    public void    setDurationMonths(int d)        { this.durationMonths = d; }
    public double  getPrice()                      { return price; }
    public void    setPrice(double price)          { this.price = price; }
    public int     getMaxClasses()                 { return maxClasses; }
    public void    setMaxClasses(int m)            { this.maxClasses = m; }
    public String  getDescription()               { return description; }
    public void    setDescription(String d)        { this.description = d; }
    public boolean isActive()                      { return isActive; }
    public void    setActive(boolean active)       { this.isActive = active; }
}
