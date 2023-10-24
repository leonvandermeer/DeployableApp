using NuGet;
using Squirrel;
using System.ComponentModel;
using System.IO;
using System.Reflection;

namespace DeployableApp;

class MyViewModel : INotifyPropertyChanged {
    private const string repoUrl = "https://github.com/leonvandermeer/DeployableApp";
    private Exception? ex;
    private string updateResult = "";

    public MyViewModel() {
        DoUpdate();
    }

    private async void DoUpdate() {
        try {
            await DoUpdateAsync();
        } catch (Exception ex) {
            Ex = ex;
        }
    }

    private async Task DoUpdateAsync() {
        var disableUpdatesFile = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "DisableUpdates.txt"
        );
        if (File.Exists(disableUpdatesFile)) {
            UpdateResult = "Updates are disabled.";
            return;
        }

        using UpdateManager updateManager = await UpdateManager.GitHubUpdateManager(repoUrl);
        SemanticVersion currentVersion = updateManager.CurrentlyInstalledVersion();
        UpdateResult = $"Current version: {currentVersion}. Updating...";

        ReleaseEntry r = await updateManager.UpdateApp();
        if (r == null || r.Version == currentVersion) {
            UpdateResult = $"You have the latest version: {currentVersion}";
        } else if (r.Version > currentVersion) {
            UpdateResult = $"After restarting you will be running version {r.Version}.";
        } else {
            UpdateResult = $"You're running version {currentVersion} which is newer than the latest {r.Version}.";
        }
    }

    public string AssemblyInformationalVersion { get; } = ThisAssembly.AssemblyInformationalVersion;

    public string UpdateResult { get => updateResult; set { updateResult = value; OnPropertyChanged(nameof(UpdateResult)); } }

    public Exception? Ex { get => ex; private set { ex = value; OnPropertyChanged(nameof(Ex)); } }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
