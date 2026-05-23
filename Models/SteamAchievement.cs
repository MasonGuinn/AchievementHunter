using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AchievementHunter.Models;

public class PlayerAchievementsResponse
{
    [JsonPropertyName("playerstats")]
    public PlayerStatsResult PlayerStats { get; set; } = new();
}

public class PlayerStatsResult
{
    [JsonPropertyName("achievements")]
    public List<SteamAchievement> Achievements { get; set; } = [];
}

public class SteamAchievement
{
    [JsonPropertyName("apiname")]
    public string ApiName { get; set; } = string.Empty;

    [JsonPropertyName("achieved")]
    public int Achieved { get; set; }

    // We will manually populate these from the Schema
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
    public double GlobalPercentage { get; set; } = 0.0;
    public string FormattedPercentage => $"{GlobalPercentage:0.0}% of players have this";
    public bool IsRare => GlobalPercentage < 10.0;
}

// --- THE GAME SCHEMA ---
public class GameSchemaResponse
{
    [JsonPropertyName("game")]
    public GameSchema Game { get; set; } = new();
}

public class GameSchema
{
    [JsonPropertyName("availableGameStats")]
    public AvailableGameStats? AvailableGameStats { get; set; }
}

public class AvailableGameStats
{
    [JsonPropertyName("achievements")]
    public List<SchemaAchievement> Achievements { get; set; } = [];
}

public class SchemaAchievement
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("icon")]
    public string Icon { get; set; } = string.Empty; // Unlocked image

    [JsonPropertyName("icongray")]
    public string IconGray { get; set; } = string.Empty; // Locked image
}

public class GlobalPercentageResponse
{
    [JsonPropertyName("achievementpercentages")]
    public GlobalPercentageResult Percentages { get; set; } = new();
}

public class GlobalPercentageResult
{
    [JsonPropertyName("achievements")]
    public List<GlobalAchievement> Achievements { get; set; } = [];
}

public class GlobalAchievement
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("percent")]
    public double Percent { get; set; }
}