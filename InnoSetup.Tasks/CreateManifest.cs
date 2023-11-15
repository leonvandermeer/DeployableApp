using Microsoft.Build.Framework;
using System.Text.Json;
using Updates.Types;
using Task = Microsoft.Build.Utilities.Task;

namespace InnoSetup.Tasks;

public class CreateManifest : Task {
    [Required]
    public ITaskItem? Installer { get; set; }

    [Required]
    public ITaskItem? Version { get; set; }

    [Required]
    public ITaskItem? OutputDir { get; set; }

    public override bool Execute() {
        try {
            ExecuteImpl();
        } catch (Exception ex) {
            Log.LogErrorFromException(ex, true, true, null);
        }
        return !Log.HasLoggedErrors;
    }

    private void ExecuteImpl() {
        Manifest manifest = new(Version!.ItemSpec, Installer!.ItemSpec);
        string scriptPath = Path.Combine(OutputDir!.ItemSpec, "manifest.json");
        using Stream script = File.Create(scriptPath);
        JsonSerializer.Serialize(script, manifest, new JsonSerializerOptions() { WriteIndented = true });
    }
}
