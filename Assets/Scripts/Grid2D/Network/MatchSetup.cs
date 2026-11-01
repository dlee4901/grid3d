using System.Collections.Generic;
using System.IO;
using System.Text;

public sealed class MatchSetup
{
    public const byte Version = 1;
    public const int MaxPlayers = 8;

    public string MapId { get; }
    public IReadOnlyList<string> PlayerIds { get; }
    public IReadOnlyList<TeamData> Teams { get; }

    public MatchSetup(string mapId, IReadOnlyList<string> playerIds, IReadOnlyList<TeamData> teams)
    {
        MapId = mapId;
        PlayerIds = playerIds;
        Teams = teams;
    }

    public int SeatOf(string playerId)
    {
        for (var i = 0; i < PlayerIds.Count; i++)
            if (PlayerIds[i] == playerId) return i + 1;
        return 0;
    }

    public List<Command> SeedCommands()
    {
        var commands = new List<Command>();
        for (var i = 0; i < Teams.Count; i++) commands.Add(new LoadTeamCommand(i + 1, Teams[i]));
        commands.Add(new SpawnTeamsCommand());
        return commands;
    }

    public string Validate(GridDefinition map)
    {
        if (map == null || map.Id != MapId) return $"map '{MapId}' does not match";
        if (PlayerIds.Count != map.PlayerCount) return $"expected {map.PlayerCount} players, got {PlayerIds.Count}";
        if (Teams.Count != PlayerIds.Count) return $"expected {PlayerIds.Count} teams, got {Teams.Count}";

        var seen = new HashSet<string>();
        foreach (var playerId in PlayerIds)
            if (string.IsNullOrEmpty(playerId) || !seen.Add(playerId)) return $"player id '{playerId}' is empty or duplicated";

        for (var seat = 1; seat <= Teams.Count; seat++)
        {
            var issue = ValidateTeam(map, seat, Teams[seat - 1]);
            if (issue != null) return $"seat {seat}: {issue}";
        }
        return null;
    }

    private static string ValidateTeam(GridDefinition map, int seat, TeamData team)
    {
        if (team.MapId != map.Id) return $"team is for map '{team.MapId}'";
        if (team.UnitStartPositions.Count == 0) return "team has no units";
        var cost = 0;
        foreach (var unit in team.UnitStartPositions)
        {
            if (!IdRegistry<EntityConfig>.TryGet(unit.UnitId, out var config)) return $"unknown unit '{unit.UnitId}'";
            if (unit.Position < 0 || unit.Position >= map.Size) return $"position {unit.Position} is off the map";
            if (map.GetSpawnPlayer(new GridPosition(map.X, map.Y, unit.Position)) != seat)
                return $"position {unit.Position} is not a seat {seat} spawn";
            cost += config.Cost;
        }
        return cost > map.MaxTeamCost ? $"team cost {cost} exceeds {map.MaxTeamCost}" : null;
    }

    public byte[] Encode()
    {
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(Version);
            WireFormat.WriteId(writer, MapId);
            writer.Write((byte)PlayerIds.Count);
            for (var i = 0; i < PlayerIds.Count; i++)
            {
                WireFormat.WriteId(writer, PlayerIds[i]);
                WireFormat.WriteTeam(writer, Teams[i]);
            }
        }
        return stream.ToArray();
    }

    public static bool TryDecode(byte[] bytes, out MatchSetup setup)
        => WireFormat.TryRead(bytes, Read, out setup);

    private static MatchSetup Read(BinaryReader reader)
    {
        if (reader.ReadByte() != Version) return null;
        var mapId = WireFormat.ReadId(reader);
        if (mapId == null) return null;
        var playerCount = reader.ReadByte();
        if (playerCount > MaxPlayers) return null;

        var playerIds = new List<string>();
        var teams = new List<TeamData>();
        for (var i = 0; i < playerCount; i++)
        {
            var playerId = WireFormat.ReadId(reader);
            if (playerId == null) return null;
            var team = WireFormat.ReadTeam(reader, mapId);
            if (team == null) return null;
            playerIds.Add(playerId);
            teams.Add(team);
        }
        return new MatchSetup(mapId, playerIds, teams);
    }
}
