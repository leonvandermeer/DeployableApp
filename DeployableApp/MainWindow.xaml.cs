namespace DeployableApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window {
    public static string AssemblyInformationalVersion { get; } = ThisAssembly.AssemblyInformationalVersion;

    public MainWindow() {
        InitializeComponent();
    }
}
