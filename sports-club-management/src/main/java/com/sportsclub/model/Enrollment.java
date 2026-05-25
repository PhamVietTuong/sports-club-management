package com.sportsclub.model;

import java.time.LocalDate;

public class Enrollment {
    private int       id;
    private int       memberId;
    private String    memberName;
    private int       classId;
    private String    className;
    private LocalDate enrollDate;
    private String    status;   // ACTIVE / CANCELLED

    public Enrollment() {}

    public int       getId()                     { return id; }
    public void      setId(int id)               { this.id = id; }
    public int       getMemberId()               { return memberId; }
    public void      setMemberId(int memberId)   { this.memberId = memberId; }
    public String    getMemberName()             { return memberName; }
    public void      setMemberName(String n)     { this.memberName = n; }
    public int       getClassId()               { return classId; }
    public void      setClassId(int classId)    { this.classId = classId; }
    public String    getClassName()             { return className; }
    public void      setClassName(String n)     { this.className = n; }
    public LocalDate getEnrollDate()            { return enrollDate; }
    public void      setEnrollDate(LocalDate d) { this.enrollDate = d; }
    public String    getStatus()               { return status; }
    public void      setStatus(String status)  { this.status = status; }
}
