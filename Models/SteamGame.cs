using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AchievementHunter.Models
{
    // These classes perfectly mirror the nested JSON structure Steam sends back
    public class OwnedGamesResponse
    {
        [JsonPropertyName("response")]
        public OwnedGamesResult Response { get; set; } = new();
    }

    public class OwnedGamesResult
    {
        [JsonPropertyName("game_count")]
        public int GameCount { get; set; }

        [JsonPropertyName("games")]
        public List<SteamGame> Games { get; set; } = [];
    }

    public class SteamGame
    {
        [JsonPropertyName("appid")]
        public int AppId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("playtime_forever")]
        public int PlaytimeForever { get; set; } // Tracked in total minutes

        [JsonPropertyName("has_community_visible_stats")]
        public bool HasCommunityVisibleStats { get; set; }
    }
}