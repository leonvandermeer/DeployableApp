namespace Updates.Types;

public record Manifest {
    public static Version CurrentManifestVersion { get; } = new(1, 0);

    public Manifest(string version, string installer) : this(CurrentManifestVersion, version, installer) { }

    private Manifest(Version manifestVersion, string version, string installer) {
        ManifestVersion = manifestVersion;
        Version = version;
        Installer = installer;
    }

    public Version ManifestVersion { get; }

    public string Version { get; }

    public string Installer { get; }
}
