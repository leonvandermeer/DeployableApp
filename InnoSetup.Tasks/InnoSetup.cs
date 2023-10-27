//#define DEBUGTHISTASK

using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;
using Microsoft.Build.Utilities;
using System.Globalization;
using System.Text.RegularExpressions;

namespace InnoSetup.Tasks;

public class InnoSetup : ToolTask {
    private const string installationFolder = @"C:\Program Files (x86)\Inno Setup 6";

    [Required]
    public ITaskItem? CompilerScript { get; set; }

    public ITaskItem[]? Includes { get; set; }

    [Required]
    public ITaskItem? AppName { get; set; }

    [Required]
    public ITaskItem? AppVersion { get; set; }

    [Required]
    public ITaskItem? VersionInfoVersion { get; set; }

    [Required]
    public ITaskItem? AppPublisher { get; set; }

    [Required]
    public ITaskItem? AppURL { get; set; }

    [Required]
    public ITaskItem? AppExeName { get; set; }

    [Required]
    public ITaskItem? OutputDir { get; set; }

    [Required]
    public ITaskItem? OutputBaseFilename { get; set; }

    public InnoSetup() {
#if DEBUGTHISTASK
        Debugger.Launch();
#endif
    }

    protected override string ToolName => "ISCC.exe"; // abstract

    protected override string GenerateFullPathToTool() { // abstract
        // Look form Inno Setup Compiler in it's installation folder. When found, return that.
        string iscc = Path.Combine(installationFolder, ToolName);
        if (File.Exists(iscc)) { return iscc; }
        // Search for the file on the system path.
        return ToolName;
    }

    protected override string GenerateCommandLineCommands() {
        CommandLineBuilderExtension commandLineBuilder = new();
        AddCommandLineCommands(commandLineBuilder);
        return commandLineBuilder.ToString();
    }

    private void AddCommandLineCommands(CommandLineBuilderExtension commandLine) {
        commandLine.AppendSwitch("/V10");
        if (Includes != null) {
            foreach (ITaskItem item in Includes) {
                commandLine.AppendSwitchIfNotNull("/J", item.ItemSpec);
            }
        }
        commandLine.AppendSwitchIfNotNull("/D", $"MyAppName={AppName!.ItemSpec}");
        commandLine.AppendSwitchIfNotNull("/D", $"MyAppVersion={AppVersion!.ItemSpec}");
        commandLine.AppendSwitchIfNotNull("/D", $"MyVersionInfoVersion={VersionInfoVersion!.ItemSpec}");
        commandLine.AppendSwitchIfNotNull("/D", $"MyAppPublisher={AppPublisher!.ItemSpec}");
        commandLine.AppendSwitchIfNotNull("/D", $"MyAppURL={AppURL!.ItemSpec}");
        commandLine.AppendSwitchIfNotNull("/D", $"MyAppExeName={AppExeName!.ItemSpec}");
        commandLine.AppendSwitchIfNotNull("/O", OutputDir);
        commandLine.AppendSwitchIfNotNull("/F", OutputBaseFilename);
        commandLine.AppendFileNameIfNotNull(CompilerScript);
    }

    protected override MessageImportance StandardErrorLoggingImportance =>
#if DEBUGTHISTASK
        MessageImportance.High;
#else
        base.StandardErrorLoggingImportance;
#endif

    protected override MessageImportance StandardOutputLoggingImportance =>
#if DEBUGTHISTASK
        MessageImportance.High;
#else
        base.StandardErrorLoggingImportance;
#endif

    private static readonly Regex crackError1 = new(@"^Error on line (\d+) in (.*?): (.*)$", RegexOptions.CultureInvariant | RegexOptions.Compiled);
    private static readonly Regex crackError2 = new("^Error in (.*?): (.*)$", RegexOptions.CultureInvariant | RegexOptions.Compiled);

    protected override void LogEventsFromTextOutput(string singleLine, MessageImportance messageImportance) {
        Match m = crackError1.Match(singleLine);
        if (m.Success) {
            int line = int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
            string file = m.Groups[2].Value;
            string message = m.Groups[3].Value;
            Log.LogError(null, null, null, file, line, 1, line, 1, message);
        } else {
            m = crackError2.Match(singleLine);
            if (m.Success) {
                string file = m.Groups[1].Value;
                string message = m.Groups[2].Value;
                Log.LogError(null, null, null, file, 1, 1, 1, 1, message);
            } else {
                base.LogEventsFromTextOutput(singleLine, messageImportance);
            }
        }
    }
}
