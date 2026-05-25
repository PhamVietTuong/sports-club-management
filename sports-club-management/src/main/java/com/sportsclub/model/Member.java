package com.sportsclub.model;

import java.time.LocalDate;
import java.time.LocalDateTime;

// PROTOTYPE PATTERN — Member is a cloneable domain object
public class Member extends User {

    private String    fullName;
    private String    gender;
    private LocalDate dateOfBirth;
    private String    address;
    private int       packageId;
    private LocalDate joinDate;
    private LocalDate expiryDate;
    private String    status;   // ACTIVE / INACTIVE / SUSPENDED

    public Member() {}

    public Member(int id, String username, String passwordHash,
                  String email, String phone, LocalDateTime createdAt,
                  String fullName, String gender, LocalDate dateOfBirth,
                  String address, int packageId,
                  LocalDate joinDate, LocalDate expiryDate, String status) {
        super(id, username, passwordHash, email, phone, Role.MEMBER, createdAt);
        this.fullName    = fullName;
        this.gender      = gender;
        this.dateOfBirth = dateOfBirth;
        this.address     = address;
        this.packageId   = packageId;
        this.joinDate    = joinDate;
        this.expiryDate  = expiryDate;
        this.status      = status;
    }

    // PROTOTYPE PATTERN — shallow clone creates a copy of this Member
    @Override
    public Member clone() {
        return (Member) super.clone(); // User.clone() handles CloneNotSupportedException
    }

    // Getters / Setters
    public String    getFullName()              { return fullName; }
    public void      setFullName(String n)      { this.fullName = n; }
    public String    getGender()                { return gender; }
    public void      setGender(String g)        { this.gender = g; }
    public LocalDate getDateOfBirth()           { return dateOfBirth; }
    public void      setDateOfBirth(LocalDate d){ this.dateOfBirth = d; }
    public String    getAddress()               { return address; }
    public void      setAddress(String a)       { this.address = a; }
    public int       getPackageId()             { return packageId; }
    public void      setPackageId(int p)        { this.packageId = p; }
    public LocalDate getJoinDate()              { return joinDate; }
    public void      setJoinDate(LocalDate d)   { this.joinDate = d; }
    public LocalDate getExpiryDate()            { return expiryDate; }
    public void      setExpiryDate(LocalDate d) { this.expiryDate = d; }
    public String    getStatus()                { return status; }
    public void      setStatus(String s)        { this.status = s; }
}
