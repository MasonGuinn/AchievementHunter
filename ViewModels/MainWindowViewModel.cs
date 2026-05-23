using AchievementHunter.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Data;
using System.Threading.Tasks;

namespace AchievementHunter.ViewModels
{
    public partial class MainWindowViewModel(SteamApiService steamService) : ViewModelBase
    {
        private readonly SteamApiService _steamService = steamService;

        [ObservableProperty]
        private string _connectionStatus = "Click 'Test Connection' to ping the Steam API.";

        // We use this to disable the button while loading so you don't spam the API
        [ObservableProperty]
        private bool _isButtonEnabled = true;

        // This automatically generates an ICommand named "TestConnectionCommand" for the UI
        [RelayCommand]
        private async Task TestConnectionAsync()
        {
            IsButtonEnabled = false;
            ConnectionStatus = "Pinging Valve servers...";

            try
            {
                string rawJson = await _steamService.CheckConnectionAsync();

                if (rawJson.Contains("personaname"))
                {
                    ConnectionStatus = "SUCCESS! Secure connection established to the Steam Web API.";
                }
                else
                {
                    ConnectionStatus = "Connected successfully, but profile may be private.";
                }
            }
            catch (System.Exception ex)
            {
                ConnectionStatus = $"CONNECTION FAILED:\n{ex.Message}";
            }
            finally
            {
                IsButtonEnabled = true;
            }
        }
    }
}