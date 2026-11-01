using System.Collections.Generic;

public static class TestGame
{
    public const int Player1Position = 0;
    public const int Player2Position = 4;

    public static GridDefinition Map(int manaPerTurn = 3, int player2Position = Player2Position, int maxTeamCost = 10)
    {
        var map = new GridDefinition
        {
            Id = "Test.Map", X = 5, Y = 1, ManaPerTurn = manaPerTurn, MaxTeamCost = maxTeamCost,
            PlayerStartPositions = new List<PositionConfig>
            {
                new() { PositionValues = new List<int> { Player1Position } },
                new() { PositionValues = new List<int> { player2Position } }
            }
        };
        map.Init();
        return map;
    }

    public static TurnExecutor Start(string player1Ability, string player2Ability, int manaPerTurn = 3,
                                     int player2Position = Player2Position)
    {
        var map = Map(manaPerTurn, player2Position);
        var executor = TurnExecutor.ForDefinition(map);
        executor.Apply(new LoadTeamCommand(1, Team(map, Player1Position, player1Ability)));
        executor.Apply(new LoadTeamCommand(2, Team(map, player2Position, player2Ability)));
        executor.Apply(new SpawnTeamsCommand());
        return executor;
    }

    public static Ability FirstAbilityAt(GridState state, int position)
    {
        state.GetEntity(position).TryGetComponent<AbilityComponent>(out var abilities);
        return abilities.List[0];
    }

    public static TeamData Team(GridDefinition map, int position, string abilityId)
        => new("Test.Team", map.Id, new List<(int, string)> { (position, TestContent.UnitWith(abilityId)) });
}
