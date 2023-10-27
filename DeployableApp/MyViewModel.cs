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

    private Task DoUpdateAsync() {
        string disableUpdatesFile = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "DisableUpdates.txt"
        );
        if (File.Exists(disableUpdatesFile)) {
            UpdateResult = "Updates are disabled.";
            return Task.CompletedTask;
        }
        throw new NotImplementedException();
    }

    public string AssemblyInformationalVersion { get; } = ThisAssembly.AssemblyInformationalVersion;

    public string UpdateResult { get => updateResult; set { updateResult = value; OnPropertyChanged(nameof(UpdateResult)); } }

    public Exception? Ex { get => ex; private set { ex = value; OnPropertyChanged(nameof(Ex)); } }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
