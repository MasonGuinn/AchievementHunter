using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AchievementHunter.Models
{
    public class PlayerAchievementsResponse
    {
        [JsonPropertyName("playerstats")]
        public PlayerStatsResult PlayerStats { get; set; } = new();
    }

    public class PlayerStatsResult
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("achievements")]
        public List<SteamAchievement> Achievements { get; set; } = [];
    }

    public class SteamAchievement
    {
        [JsonPropertyName("apiname")]
        public string ApiName { get; set; } = string.Empty;

        [JsonPropertyName("achieved")]
        public int Achieved { get; set; } // Steam returns 1 for unlocked, 0 for locked
    }
}