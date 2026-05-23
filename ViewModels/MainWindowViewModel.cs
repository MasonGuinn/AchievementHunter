using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AchievementHunter.Models;
using AchievementHunter.Services;

namespace AchievementHunter.ViewModels
{
    public partial class MainWindowViewModel(SteamApiService steamService) : ViewModelBase
    {
        private readonly SteamApiService _steamService = steamService;

        [ObservableProperty]
        private string _statusMessage = "Ready to load library.";

        [ObservableProperty]
        private bool _isLoading = false;

        // ObservableCollection automatically updates the Avalonia UI when items are added
        public ObservableCollection<SteamGame> Games { get; } = [];

        [RelayCommand]
        private async Task LoadLibraryAsync()
        {
            IsLoading = true;
            StatusMessage = "Fetching games from Steam...";
            Games.Clear(); // Clear the list in case you click the button twice

            try
            {
                var fetchedGames = await _steamService.GetOwnedGamesAsync();

                foreach (var game in fetchedGames)
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
    }
}