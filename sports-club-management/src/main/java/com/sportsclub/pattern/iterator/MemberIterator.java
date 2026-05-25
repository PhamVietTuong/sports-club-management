package com.sportsclub.pattern.iterator;

import com.sportsclub.model.Member;
import java.util.List;

// ITERATOR PATTERN — concrete iterator for Member collections
public class MemberIterator implements ClubIterator<Member> {
    private final List<Member> members;
    private int index = 0;

    public MemberIterator(List<Member> members) {
        this.members = members;
    }

    @Override public boolean hasNext() { return index < members.size(); }
    @Override public Member  next()    { return members.get(index++); }
    @Override public void    reset()   { index = 0; }
}
