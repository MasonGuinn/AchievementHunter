using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Configuration;
using AchievementHunter.ViewModels;
using AchievementHunter.Views;
using AchievementHunter.Services;

namespace AchievementHunter
{
    public partial class App : Application
    {
        public override void Initialize() => AvaloniaXamlLoader.Load(this);

        public override void OnFrameworkInitializationCompleted()
        {
            // 1. Build the configuration to safely read your hidden user secrets once
            IConfigurationRoot config = new ConfigurationBuilder()
                .AddUserSecrets<App>()
                .Build();

            // 2. Extract the keys
            string apiKey = config["Steam:ApiKey"] ?? string.Empty;
            string steamId = config["Steam:SteamId"] ?? string.Empty;

            // 3. Initialize your engine
            var steamService = new SteamApiService(apiKey, steamId);

            // 4. Pass the engine into the ViewModel when the window loads
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(steamService),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}