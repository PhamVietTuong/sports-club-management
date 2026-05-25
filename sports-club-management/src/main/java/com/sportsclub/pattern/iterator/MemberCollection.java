package com.sportsclub.pattern.iterator;

import com.sportsclub.model.Member;
import java.util.ArrayList;
import java.util.List;

// ITERATOR PATTERN — concrete collection for Member objects
public class MemberCollection implements ClubCollection<Member> {
    private final List<Member> members = new ArrayList<>();

    @Override public void add(Member m)  { members.add(m); }
    @Override public int  size()         { return members.size(); }

    @Override
    public ClubIterator<Member> createIterator() {
        return new MemberIterator(members);
    }
}
