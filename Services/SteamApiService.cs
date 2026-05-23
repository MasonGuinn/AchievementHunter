using AchievementHunter.Models;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace AchievementHunter.Services
{
    public class SteamApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _steamId;
        private const string _baseUrl = "http://api.steampowered.com";

        // The constructor expects the keys to be passed in when the app starts
        public SteamApiService(string apiKey, string steamId)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("CRITICAL ERROR: Steam API Key is missing.");

            if (string.IsNullOrWhiteSpace(steamId))
                throw new ArgumentException("CRITICAL ERROR: Steam64 ID is missing.");

            _apiKey = apiKey;
            _steamId = steamId;

            // Initialize the HTTP Client
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
        }

        /// <summary>
        /// Pings the Steam API to verify credentials by requesting basic profile data.
        /// </summary>
        public async Task<string> CheckConnectionAsync()
        {
            string endpoint = $"{_baseUrl}/ISteamUser/GetPlayerSummaries/v0002/?key={_apiKey}&steamids={_steamId}";

            try
            {
                // Await the response so the UI thread doesn't freeze
                HttpResponseMessage response = await _httpClient.GetAsync(endpoint);

                // Automatically throw an exception if Steam returns a 403 Forbidden or 404
                response.EnsureSuccessStatusCode();

                // Read the JSON payload as a raw string
                string responseBody = await response.Content.ReadAsStringAsync();

                // For the handshake, we will just return the raw JSON string to prove it connected
                return responseBody;
            }
            catch (HttpRequestException httpErr)
            {
                throw new Exception($"HTTP Error occurred: {httpErr.Message}");
            }
            catch (TaskCanceledException)
            {
                throw new Exception("Connection timed out. Steam servers might be down.");
            }
        }

        /// <summary>
        /// Fetches the user's entire Steam library, including AppIDs and Game Names
        /// </summary>
        public async Task<List<SteamGame>> GetOwnedGamesAsync()
        {
            // include_appinfo=1 is crucial. Without it, Steam only returns numbers (AppIDs), not the actual game names
            string endpoint = $"{_baseUrl}/IPlayerService/GetOwnedGames/v0001/?key={_apiKey}&steamid={_steamId}&include_appinfo=1&format=json";

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(endpoint);
                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();

                // Deserialize the raw JSON directly into our strict C# objects
                var result = JsonSerializer.Deserialize<OwnedGamesResponse>(jsonResponse);

                // Return the list of games, or an empty list if something went wrong
                return result?.Response?.Games ?? [];
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to fetch game library: {ex.Message}");
            }
        }

        /// <summary>
        /// Fetches the achievement unlock status for a specific game.
        /// </summary>
        public async Task<List<SteamAchievement>> GetAchievementsAsync(int appId)
        {
            string statsEndpoint = $"{_baseUrl}/ISteamUserStats/GetPlayerAchievements/v0001/?appid={appId}&key={_apiKey}&steamid={_steamId}&l=english";
            string schemaEndpoint = $"{_baseUrl}/ISteamUserStats/GetSchemaForGame/v2/?key={_apiKey}&appid={appId}&l=english";
            string globalEndpoint = $"{_baseUrl}/ISteamUserStats/GetGlobalAchievementPercentagesForApp/v0002/?gameid={appId}";

            try
            {
                HttpResponseMessage statsResponse = await _httpClient.GetAsync(statsEndpoint);

                if (!statsResponse.IsSuccessStatusCode)
                {
                    if ((int)statsResponse.StatusCode == 400)
                        throw new Exception("0_PLAYTIME");

                    return new List<SteamAchievement>();
                }

                // CRITICAL FIX: Make the JSON parser bulletproof against Steam's inconsistent numerical formatting
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString |
                                     System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals
                };

                var statsResult = JsonSerializer.Deserialize<PlayerAchievementsResponse>(await statsResponse.Content.ReadAsStringAsync(), jsonOptions);
                var playerStats = statsResult?.PlayerStats?.Achievements ?? new List<SteamAchievement>();

                if (playerStats.Count == 0)
                    return playerStats;

                HttpResponseMessage schemaResponse = await _httpClient.GetAsync(schemaEndpoint);
                if (schemaResponse.IsSuccessStatusCode)
                {
                    var schemaResult = JsonSerializer.Deserialize<GameSchemaResponse>(await schemaResponse.Content.ReadAsStringAsync(), jsonOptions);
                    var schemaStats = schemaResult?.Game?.AvailableGameStats?.Achievements ?? new List<SchemaAchievement>();

                    foreach (var stat in playerStats)
                    {
                        var match = schemaStats.Find(s => s.Name == stat.ApiName);
                        if (match != null)
                        {
                            stat.DisplayName = match.DisplayName;
                            stat.Description = match.Description;
                            stat.IconUrl = stat.Achieved == 1 ? match.Icon : match.IconGray;
                        }
                    }
                }

                HttpResponseMessage globalResponse = await _httpClient.GetAsync(globalEndpoint);
                if (globalResponse.IsSuccessStatusCode)
                {
                    var globalResult = JsonSerializer.Deserialize<GlobalPercentageResponse>(await globalResponse.Content.ReadAsStringAsync(), jsonOptions);
                    var globalStats = globalResult?.Percentages?.Achievements ?? new List<GlobalAchievement>();

                    foreach (var stat in playerStats)
                    {
                        var match = globalStats.Find(g => g.Name == stat.ApiName);
                        if (match != null)
                            stat.GlobalPercentage = match.Percent;
                    }
                }

                return playerStats;
            }
            catch (Exception ex) when (ex.Message != "0_PLAYTIME")
            {
                throw new Exception($"API Fetch Error: {ex.Message}");
            }
        }
    }
}