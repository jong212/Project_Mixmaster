using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameDB : Singleton<GameDB>
{
    //초기화 하고
    private Dictionary<string, MonsterData> monsterDictionary = new Dictionary<string, MonsterData>();
    public Dictionary<string, PlayerData>  playerDictionary = new Dictionary<string, PlayerData>();
    public Dictionary<string, TeamData> TeamDictionary = new Dictionary<string, TeamData>();

    void Awake()
    {
        InitializeMonsterData();
    }

    //데이터 세팅
    private void InitializeMonsterData()
    {
        AddMonster("mon1", new MonsterData("Monster1", 1, 500, 10,"S",0));
        AddMonster("mon2", new MonsterData("Monster2", 2, 200, 20,"L",10));
        AddMonster("mon3", new MonsterData("Monster3", 3, 300, 30,"L",20));
        AddMonster("mon4", new MonsterData("Monster4", 4, 400, 40,"L",30));
        AddMonster("mon5", new MonsterData("Monster5", 5, 500, 50,"L",40));
        AddMonster("mon6", new MonsterData("Monster6", 6, 600, 60,"L", 50));
        AddMonster("mon7", new MonsterData("Monster7", 7, 700, 70, "L", 60));
        AddMonster("mon8", new MonsterData("Monster8", 8, 800, 80, "L", 70));
        AddMonster("mon9", new MonsterData("Monster9", 9, 900, 90, "L", 80));
        AddMonster("mon10", new MonsterData("Monster10", 10, 1000, 100, "L", 90));

        AddTeam("team1", new TeamData("team1monster", 1, 1000, 10, "L", 90,true));
        AddTeam("team2", new TeamData("team2monster", 2, 1000, 100, "L", 90, true));
        AddTeam("team3", new TeamData("team3monster", 3, 1000, 100, "L", 90, true));
        AddPlayer("Player", new PlayerData("Hi", 1, 100, 100));
    }

    public void AddMonster(string monsterID, MonsterData monsterData)
    {
        if (!monsterDictionary.ContainsKey(monsterID))
        {
            monsterDictionary.Add(monsterID, monsterData);
        }
        else
        {
            Debug.LogWarning("Monster with ID " + monsterID + " already exists in the dictionary.");
        }
    }
    public void AddPlayer(string playterId, PlayerData monsterData)
    {
        if (!playerDictionary.ContainsKey(playterId))
        {
            playerDictionary.Add(playterId, monsterData);
        }
        else
        {
            Debug.LogWarning("Monster with ID " + playterId + " already exists in the dictionary.");
        }
    }
    public void AddTeam(string teamId, TeamData teamData)
    {
        if (!TeamDictionary.ContainsKey(teamId))
        {
            TeamDictionary.Add(teamId, teamData);
        }
        else
        {
            Debug.LogWarning("Monster with ID " + teamId + " already exists in the dictionary.");
        }
    }
    public MonsterData GetMonster(string monsterID)
    {
        if (monsterDictionary.ContainsKey(monsterID))
        {
            return monsterDictionary[monsterID];
        }
        else
        {
            Debug.LogWarning("Monster with ID " + monsterID + " does not exist in the dictionary.");
            return null;
        }
    }
    public TeamData GetTeam(string teamID)
    {
        if (TeamDictionary.ContainsKey(teamID))
        {
            return TeamDictionary[teamID];
        }
        else
        {
            Debug.LogWarning("Monster with ID " + teamID + " does not exist in the dictionary.");
            return null;
        }
    }
}

public class TeamData
{
    public string name;
    public int level;
    public float health;
    public int str;
    public string rangetype;
    public float range;
    public bool state;
    public TeamData(string name, int level, float health, int str, string rangetype, float range, bool state)
    {
        this.name = name;
        this.level = level;
        this.health = health;
        this.str = str;
        this.rangetype = rangetype;
        this.range = range;
        this.state = state;
    }
}

public class MonsterData
{
    public string name;
    public int level;    
    public float health;
    public int str;
    public string rangetype;
    public float range;

    public MonsterData(string name, int level, float health, int str, string rangetype ,float range)
    {
        this.name = name;
        this.level = level;
        this.health = health;
        this.str = str;
        this.rangetype = rangetype;
        this.range = range;
    }
}
public class PlayerData
{
    public string name;
    public int level;
    public int str;
    public float health;

    public PlayerData(string name, int level, float health, int str)
    {
        this.name = name;
        this.level = level;
        this.str = str;
        this.health = health;
    }
}