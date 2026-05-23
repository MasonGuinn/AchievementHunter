using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AchievementHunter.Models;
using AchievementHunter.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AchievementHunter.ViewModels
{
    public partial class MainWindowViewModel(SteamApiService steamService) : ViewModelBase
    {
        [ObservableProperty] public partial string StatusMessage { get; set; } = "Ready to load library.";
        [ObservableProperty] public partial bool IsLoading { get; set; } = false;
        [ObservableProperty] public partial SteamGame? SelectedGame { get; set; }
        [ObservableProperty] public partial bool ShowOnlyAchievements { get; set; } = true;

        // NEW: Search Bar
        [ObservableProperty] public partial string SearchQuery { get; set; } = string.Empty;

        // NEW: Progress Bar Data
        [ObservableProperty] public partial int UnlockedCount { get; set; } = 0;
        [ObservableProperty] public partial int TotalCount { get; set; } = 0;
        [ObservableProperty] public partial bool IsProgressVisible { get; set; } = false;

        private List<SteamGame> _masterGameList = new();
        public ObservableCollection<SteamGame> Games { get; } = new();
        public ObservableCollection<SteamAchievement> Achievements { get; } = new();

        [RelayCommand]
        private async Task LoadLibraryAsync()
        {
            IsLoading = true;
            StatusMessage = "Fetching games from Steam...";
            try
            {
                _masterGameList = await steamService.GetOwnedGamesAsync();
                ApplyGameFilter();
                StatusMessage = $"Successfully loaded {_masterGameList.Count} games.";
            }
            catch (System.Exception ex) { StatusMessage = $"ERROR: {ex.Message}"; }
            finally { IsLoading = false; }
        }

        partial void OnShowOnlyAchievementsChanged(bool value) => ApplyGameFilter();
        partial void OnSearchQueryChanged(string value) => ApplyGameFilter();

        private void ApplyGameFilter()
        {
            Games.Clear();
            foreach (var game in _masterGameList)
            {
                if (ShowOnlyAchievements && !game.HasCommunityVisibleStats)
                    continue;
                if (!string.IsNullOrWhiteSpace(SearchQuery) && !game.Name.Contains(SearchQuery, System.StringComparison.OrdinalIgnoreCase))
                    continue;
                Games.Add(game);
            }
        }

        async partial void OnSelectedGameChanged(SteamGame? value)
        {
            if (value == null)
                return;
            Achievements.Clear();
            IsProgressVisible = false;
            StatusMessage = $"Loading achievements for {value.Name}...";

            try
            {
                var fetchedAchievements = await steamService.GetAchievementsAsync(value.AppId);
                if (fetchedAchievements.Count == 0)
                {
                    StatusMessage = $"{value.Name} does not have Steam achievements.";
                    return;
                }

                // HIT LIST SORTING: Locked (0) comes first, then sorted by easiest to achieve globally
                var sortedList = fetchedAchievements
                    .OrderBy(a => a.Achieved)
                    .ThenByDescending(a => a.GlobalPercentage)
                    .ToList();

                foreach (var ach in sortedList)
                    Achievements.Add(ach);

                // Update Progress Bar
                UnlockedCount = fetchedAchievements.Count(a => a.Achieved == 1);
                TotalCount = fetchedAchievements.Count;
                IsProgressVisible = true;
                StatusMessage = $"{value.Name} Loaded";
            }
            catch (System.Exception ex)
            {
                StatusMessage = $"Failed to load achievements: {ex.Message}";
            }
        }
    }
}