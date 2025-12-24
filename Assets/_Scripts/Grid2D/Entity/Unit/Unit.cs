using System;

public enum UnitAction {Move, Skill}

[Serializable]
public class Unit : Entity
{
    public int UnitId;

    public int Cost;
    public int StartingHealth;

    public Move Move;
    public Skill Skill1;
    public Skill Skill2;
    
    public Unit(int unitId=0, int cost=0, int startingHealth=0, Move move=null, Skill skill1=null, Skill skill2=null) : base()
    {
        UnitId = unitId;
        Cost = cost;
        StartingHealth = startingHealth;
        Move = move;
        Skill1 = skill1;
        Skill2 = skill2;
    }

    // public Unit(UnitStruct unitStruct)
    // : this(unitStruct.Id, unitStruct.Name, unitStruct.Cost, unitStruct.StartingHealth, new Move(unitStruct.Move), new Skill(unitStruct.Skill1), new Skill(unitStruct.Skill2)) {}
}