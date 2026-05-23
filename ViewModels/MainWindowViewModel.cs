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
        [ObservableProperty]
        private string _statusMessage = "Ready to load library.";

        [ObservableProperty]
        private bool _isLoading = false;

        // Tracks the currently clicked game in the UI
        [ObservableProperty]
        private SteamGame? _selectedGame;

        public ObservableCollection<SteamGame> Games { get; } = [];
        public ObservableCollection<SteamAchievement> Achievements { get; } = []; // New list for achievements

        [RelayCommand]
        private async Task LoadLibraryAsync()
        {
            IsLoading = true;
            StatusMessage = "Fetching games from Steam...";
            Games.Clear();
            Achievements.Clear();

            try
            {
                List<SteamGame> fetchedGames = await steamService.GetOwnedGamesAsync();
                foreach (SteamGame game in fetchedGames)
                {
                    Games.Add(game);
                }

                StatusMessage = $"Successfully loaded {Games.Count} games.";
            }
            catch (System.Exception ex)
            {
                StatusMessage = $"ERROR: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        // The Source Generator automatically triggers this method the exact millisecond you click a new game!
        async partial void OnSelectedGameChanged(SteamGame? value)
        {
            if (value == null)
                return;

            Achievements.Clear();
            StatusMessage = $"Loading achievements for {value.Name}...";

            List<SteamAchievement> fetchedAchievements = await steamService.GetAchievementsAsync(value.AppId);

            if (fetchedAchievements.Count == 0)
            {
                StatusMessage = $"{value.Name} does not have Steam achievements.";
                return;
            }
                
            foreach (SteamAchievement ach in fetchedAchievements)
            {
                Achievements.Add(ach);
            }

            int unlockedCount = fetchedAchievements.Count(a => a.Achieved == 1);
            StatusMessage = $"{value.Name}: {unlockedCount} / {fetchedAchievements.Count} Unlocked";
        }
    }
}