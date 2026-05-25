package com.sportsclub.model;

import java.time.LocalDateTime;

// PROTOTYPE PATTERN — Coach is a cloneable domain object
public class Coach extends User {

    private String fullName;
    private String specialization;
    private String bio;
    private int    experience;
    private double salary;

    public Coach() {}

    public Coach(int id, String username, String passwordHash,
                 String email, String phone, LocalDateTime createdAt,
                 String fullName, String specialization,
                 String bio, int experience, double salary) {
        super(id, username, passwordHash, email, phone, Role.COACH, createdAt);
        this.fullName       = fullName;
        this.specialization = specialization;
        this.bio            = bio;
        this.experience     = experience;
        this.salary         = salary;
    }

    // PROTOTYPE PATTERN — shallow clone creates a copy of this Coach
    @Override
    public Coach clone() {
        return (Coach) super.clone(); // User.clone() handles CloneNotSupportedException
    }

    // Getters / Setters
    public String getFullName()               { return fullName; }
    public void   setFullName(String n)       { this.fullName = n; }
    public String getSpecialization()         { return specialization; }
    public void   setSpecialization(String s) { this.specialization = s; }
    public String getBio()                    { return bio; }
    public void   setBio(String b)            { this.bio = b; }
    public int    getExperience()             { return experience; }
    public void   setExperience(int e)        { this.experience = e; }
    public double getSalary()                 { return salary; }
    public void   setSalary(double s)         { this.salary = s; }
}
