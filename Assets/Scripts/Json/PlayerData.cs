using System;
using System.Collections.Generic;

[Serializable]
public class PlayerData
{
    public List<TeamData> Teams { get; set; }

    public List<TeamData> GetTeams(int mapId)
    {
        List<TeamData> teams = new();
        foreach (TeamData team in Teams)
        {
            if (team.MapId == mapId)
            {
                teams.Add(team);
            }
        }
        return teams;
    }

    public List<string> GetTeamNames(int mapId)
    {
        List<string> teamNames = new();
        List<TeamData> teams = GetTeams(mapId);
        foreach (TeamData team in teams)
        {
            teamNames.Add(team.Name);
        }
        return teamNames;
    }
}