public sealed class LoadTeamCommand : Command
{
    public int Player { get; }
    public TeamData TeamData { get; }

    public override int IssuingPlayer => Player;
    public override bool RequiresActiveTurn => false;

    public LoadTeamCommand(int player, TeamData teamData)
    {
        Player = player;
        TeamData = teamData;
    }

    internal override bool ApplyTo(GridState state)
    {
        state.LoadPlayerTeam(Player, TeamData);
        return true;
    }
}
