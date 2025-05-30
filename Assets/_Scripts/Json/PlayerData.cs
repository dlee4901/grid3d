using System;
using System.Collections.Generic;

[Serializable]
public class PlayerData
{
    public List<TeamData> Teams { get; set; }

    public List<TeamData> GetTeams(string mapName)
    {
        List<TeamData> teams = new();
        foreach (TeamData team in Teams)
        {
            if (team.MapName == mapName)
            {
                teams.Add(team);
            }
        }
        return teams;
    }

    public List<string> GetTeamNames(string mapName)
    {
        List<string> teamNames = new();
        List<TeamData> teams = GetTeams(mapName);
        foreach (TeamData team in teams)
        {
            teamNames.Add(team.Name);
        }
        return teamNames;
    }
}