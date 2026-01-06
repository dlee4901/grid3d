using System.Collections.Generic;

public class EntityConfig
{
    public string Name;
    public int Cost = 0;
    public int Health = 0;
    public List<string> Skills;
    public MoveComponent Move;
}
