using System;
using System.Collections.Generic;

public enum UnitAction {Move, Skill}

public class Unit : Entity
{
    public int UnitId;
    public int Cost;

    public Move Move;
    public List<Skill> Skills;
    public List<Passive> Passives;
    
    public Unit(int unitId, int cost, int health, Move move, List<Skill> skills, List<Passive> passives)
    {
        UnitId = unitId;
        Cost = cost;
        Health = health;
        Move = move;
        Skills = skills;
        Passives = passives;
    }

    // public Unit(UnitStruct unitStruct)
    // : this(unitStruct.Id, unitStruct.Name, unitStruct.Cost, unitStruct.StartingHealth, new Move(unitStruct.Move), new Skill(unitStruct.Skill1), new Skill(unitStruct.Skill2)) {}
}