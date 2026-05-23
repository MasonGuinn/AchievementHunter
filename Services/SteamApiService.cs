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
        /// Fetches the user's entire Steam library, including AppIDs and Game Names.
        /// </summary>
        public async Task<List<SteamGame>> GetOwnedGamesAsync()
        {
            // include_appinfo=1 is crucial—without it, Steam only returns numbers (AppIDs), not the actual game names!
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
    }
}