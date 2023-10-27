using Microsoft.Build.Framework;
using Task = Microsoft.Build.Utilities.Task;

namespace InnoSetup.Tasks;

public class CreateInnoSetup : Task {

    [Required]
    public ITaskItem[]? PublishItems { get; set; }

    [Required]
    public ITaskItem? CompilerScript { get; set; }

    public override bool Execute() {
        try {
            ExecuteImpl();
        } catch (Exception ex) {
            Log.LogErrorFromException(ex, true, true, null);
        }
        return !Log.HasLoggedErrors;
    }

    private void ExecuteImpl() {
        string scriptPath = CompilerScript!.ItemSpec;
        using TextWriter script = File.CreateText(scriptPath);
        SetupSection(script);
        FilesSection(script);
    }

    private static void SetupSection(TextWriter script) {
        script.WriteLine("[Setup]");
        script.WriteLine("VersionInfoVersion={#MyVersionInfoVersion}");
        script.WriteLine("VersionInfoTextVersion={#MyAppVersion}");
        script.WriteLine("AppVersion={#MyAppVersion}");
        script.WriteLine(";AppVerName={#MyAppName} {#MyAppVersion}");
    }

    private void FilesSection(TextWriter script) {
        script.WriteLine("[Files]");
        foreach (ITaskItem item in PublishItems!) {
            string source = item.GetMetadata("OutputPath");
            string relDir = Path.GetDirectoryName(item.GetMetadata("RelativePath"));
            string destDir = Path.Combine("{app}", relDir);
            script.WriteLine("Source: \"{0}\"; DestDir: \"{1}\"; Flags: ignoreversion", source, destDir);
        }
    }
}
