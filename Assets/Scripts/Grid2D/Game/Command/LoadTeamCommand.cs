public sealed class LoadTeamCommand : ICommand
{
    public int Player { get; }
    public TeamData TeamData { get; }

    public int IssuingPlayer => Player;          // reuse the team's player
    public bool RequiresActiveTurn => false;     // setup, runs before turns

    public LoadTeamCommand(int player, TeamData teamData)
    {
        Player = player;
        TeamData = teamData;
    }

    public bool ApplyTo(GridState state)
    {
        state.LoadPlayerTeam(Player, TeamData);
        return true;
    }
}
