using System;

public enum UnitAction {Move, Skill}

[Serializable]
public class Unit : Entity
{
    public int UnitId;// { get; private set; }

    public int Cost;// { get; private set; }
    public int StartingHealth;// { get; private set; }

    public Move Move;// { get; private set; }
    public Skill Skill1;// { get; private set; }
    public Skill Skill2;// { get; private set; }

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