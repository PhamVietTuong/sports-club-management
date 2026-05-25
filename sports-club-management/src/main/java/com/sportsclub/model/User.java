package com.sportsclub.model;

import com.sportsclub.pattern.prototype.SportClubPrototype;
import java.time.LocalDateTime;

/**
 * PROTOTYPE PATTERN — abstract base for all user types.
 * Subclasses override clone() for shallow copy of their own fields.
 */
public abstract class User implements SportClubPrototype, Cloneable {

    public enum Role { ADMIN, COACH, MEMBER }

    protected int           id;
    protected String        username;
    protected String        passwordHash;
    protected String        email;
    protected String        phone;
    protected Role          role;
    protected LocalDateTime createdAt;

    public User() {}

    public User(int id, String username, String passwordHash,
                String email, String phone, Role role, LocalDateTime createdAt) {
        this.id           = id;
        this.username     = username;
        this.passwordHash = passwordHash;
        this.email        = email;
        this.phone        = phone;
        this.role         = role;
        this.createdAt    = createdAt;
    }

    // PROTOTYPE PATTERN — delegates to Object.clone(); subclasses narrow the return type
    @Override
    public User clone() {
        try { return (User) super.clone(); }
        catch (CloneNotSupportedException e) { throw new RuntimeException(e); }
    }

    // Getters / Setters
    public int           getId()           { return id; }
    public void          setId(int id)     { this.id = id; }
    public String        getUsername()     { return username; }
    public void          setUsername(String username) { this.username = username; }
    public String        getPasswordHash() { return passwordHash; }
    public void          setPasswordHash(String h)   { this.passwordHash = h; }
    public String        getEmail()        { return email; }
    public void          setEmail(String email)      { this.email = email; }
    public String        getPhone()        { return phone; }
    public void          setPhone(String phone)      { this.phone = phone; }
    public Role          getRole()         { return role; }
    public void          setRole(Role role){ this.role = role; }
    public LocalDateTime getCreatedAt()   { return createdAt; }
    public void          setCreatedAt(LocalDateTime t) { this.createdAt = t; }
}
