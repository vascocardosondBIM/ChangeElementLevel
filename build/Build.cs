using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

class Build : NukeBuild
{
    AbsolutePath PluginProject => RootDirectory / "ChangeElementLevel" / "ChangeElementLevel.csproj";
    AbsolutePath PluginOutput => RootDirectory / "ChangeElementLevel" / "bin" / "x64" / "Release" / "net8.0-windows";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    AbsolutePath StagingDirectory => ArtifactsDirectory / "staging";
    AbsolutePath IssFile => RootDirectory / "installer" / "ChangeElementLevel.iss";
    AbsolutePath InstallerOutputDirectory => ArtifactsDirectory / "installer";

    [Parameter("Versão mostrada no instalador (ex.: 1.0.0)")]
    readonly string InstallerVersion = "1.0.0";

    [Parameter("Ano do Revit de destino (pasta Addins)")]
    readonly string RevitYear = "2025";

    public static int Main() => Execute<Build>(x => x.Pack);

    Target Clean => _ => _
        .Executes(() =>
        {
            ArtifactsDirectory.CreateOrCleanDirectory();
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s.SetProjectFile(PluginProject));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(PluginProject)
                .SetConfiguration("Release")
                .SetProperty("Platform", "x64"));
        });

    Target StageAddin => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            Assert.DirectoryExists(PluginOutput,
                $"Compila o projeto em Release|x64 antes do pack. Pasta esperada: {PluginOutput}");
            StagingDirectory.CreateOrCleanDirectory();
            FileSystemTasks.CopyDirectoryRecursively(PluginOutput, StagingDirectory, DirectoryExistsPolicy.Merge, FileExistsPolicy.Overwrite);
        });

    Target Installer => _ => _
        .DependsOn(StageAddin)
        .Executes(() =>
        {
            InstallerOutputDirectory.CreateDirectory();
            AbsolutePath iscc = ResolveIsccCompiler();
            string stagingArg = StagingDirectory.ToString().Replace('\\', '/');
            string args =
                $"/Qp " +
                $"/O\"{InstallerOutputDirectory}\" " +
                $"/DMyAppVersion={InstallerVersion} " +
                $"/DRevitYear={RevitYear} " +
                $"/DStagingDir=\"{stagingArg}\" " +
                $"\"{IssFile}\"";
            ProcessTasks.StartProcess(iscc, args).AssertZeroExitCode();
        });

    Target Pack => _ => _
        .Description("Compila o add-in e gera o Setup.exe (Inno Setup).")
        .DependsOn(Installer);

    AbsolutePath ResolveIsccCompiler()
    {
        var nugetRoot = Environment.GetEnvironmentVariable("NUGET_PACKAGES");
        AbsolutePath root = !string.IsNullOrEmpty(nugetRoot)
            ? (AbsolutePath)nugetRoot
            : (AbsolutePath)Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".nuget",
                "packages");

        var candidates = root.GlobFiles("tools.innosetup/*/tools/ISCC.exe").OrderByDescending(x => x.ToString()).ToList();
        Assert.True(candidates.Count > 0,
            "Não foi encontrado ISCC.exe (pacote NuGet Tools.InnoSetup). Executa `dotnet restore` na pasta build.");

        return candidates[0];
    }
}
