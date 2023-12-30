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
        CodeSection(script);
    }

    private static void SetupSection(TextWriter script) {
        script.WriteLine("[Setup]");
        script.WriteLine("VersionInfoVersion={#MyVersionInfoVersion}");
        script.WriteLine("VersionInfoTextVersion={#MyAppVersion}");
        script.WriteLine("AppVersion={#MyAppVersion}");
        script.WriteLine(";AppVerName={#MyAppName} {#MyAppVersion}");
        script.WriteLine("SignTool=innosetup.tasks");
    }

    private void FilesSection(TextWriter script) {
        script.WriteLine("[Files]");
        foreach (ITaskItem item in PublishItems!) {
            string source = item.GetMetadata("OutputPath");
            string relDir = Path.GetDirectoryName(item.GetMetadata("RelativePath"));
            string destDir = Path.Combine("{app}", relDir);
            string destFile = Path.Combine(destDir, Path.GetFileName(source));
            script.WriteLine("Source: \"{0}\"; DestDir: \"{1}\"; Flags: ignoreversion; AfterInstall: OnInstalled('{2}')", source, destDir, destFile);
        }
    }

    private void CodeSection(TextWriter script) {
        script.WriteLine(@"
[UninstallDelete]
Type: files; Name: ""{app}\FileListAbsolute.txt""

[Code]
var
  FileWrites: TStringList;
  Superfluous: TStringList;

procedure OnInstalled(FileName: String);
var
  Pos: Integer;
begin
  Log('OnInstalled: ' + FileName);
  FileWrites.Add(ExpandConstant(FileName));
  Pos := Superfluous.IndexOf(ExpandConstant(FileName));
  if Pos >= 0 then
    begin
      Superfluous.Delete(Pos);
    end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
var i: Integer;
begin
  Log('CurStepChanged(' + IntToStr(Ord(CurStep)) + ') called');
  if CurStep = ssInstall then
    begin
      FileWrites := TStringList.Create;
      Superfluous := TStringList.Create;
      if FileExists(ExpandConstant('{app}\FileListAbsolute.txt')) then
        begin
          Superfluous.LoadFromFile(ExpandConstant('{app}\FileListAbsolute.txt'));
        end;
    end
  else if CurStep = ssPostInstall then
    begin
      for i := 0 to Superfluous.Count - 1 do
        begin
          DeleteFile(Superfluous[i]);
          RemoveDir(ExtractFileDir(Superfluous[i]));
        end;
      FileWrites.SaveToFile(ExpandConstant('{app}\FileListAbsolute.txt'));
    end;
end;");
    }
}
