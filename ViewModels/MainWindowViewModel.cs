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

        // THE NEW TOGGLE (Defaults to True)
        [ObservableProperty] public partial bool ShowOnlyAchievements { get; set; } = true;

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

        // Triggers instantly whenever you click the Toggle Switch in the UI
        partial void OnShowOnlyAchievementsChanged(bool value) => ApplyGameFilter();

        private void ApplyGameFilter()
        {
            Games.Clear();
            foreach (var game in _masterGameList)
            {
                // If toggle is ON, skip games that don't have community stats
                if (ShowOnlyAchievements && !game.HasCommunityVisibleStats)
                    continue;
                Games.Add(game);
            }
        }

        async partial void OnSelectedGameChanged(SteamGame? value)
        {
            if (value == null)
                return;
            Achievements.Clear();
            StatusMessage = $"Loading achievements for {value.Name}...";

            var fetchedAchievements = await steamService.GetAchievementsAsync(value.AppId);
            if (fetchedAchievements.Count == 0)
            {
                StatusMessage = $"{value.Name} does not have Steam achievements.";
                return;
            }

            foreach (var ach in fetchedAchievements)
                Achievements.Add(ach);
            int unlockedCount = fetchedAchievements.Count(a => a.Achieved == 1);
            StatusMessage = $"{value.Name}: {unlockedCount} / {fetchedAchievements.Count} Unlocked";
        }
    }
}