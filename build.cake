#tool "nuget:?package=xunit.runner.console"
#tool "nuget:?package=GitVersion.CommandLine"
#tool "nuget:?package=ILRepack"

using Cake.Common.Tools.GitVersion;
using IoPath = System.IO.Path;

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
string buildDir = Directory("./src/EasyHash/bin") + Directory(configuration);
string outputDir = Directory("./output");
string relativeSlnPath = "./src/EasyHash.sln";
string primaryDllName = "EasyHash.dll";
string nugetDir = "./nuget";

Task("Clean")
    .Does(() =>
{
    #break
    Information("Build directory is " + buildDir);
    CleanDirectory(buildDir);
    CleanDirectory(outputDir);
    CleanDirectory(nugetDir);
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore(relativeSlnPath);
});

Task("UpdateAssemblyInfo")
    .Does(() =>
{
    GitVersion version = GitVersion(new GitVersionSettings { UpdateAssemblyInfo = true });
    PrintGitVersion(version);
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .IsDependentOn("UpdateAssemblyInfo")
    .Does(() =>
{
    if(IsRunningOnWindows())
    {
      MSBuild(relativeSlnPath, settings => settings.SetConfiguration(configuration));
    }
    else
    {
      XBuild(relativeSlnPath, settings => settings.SetConfiguration(configuration));
    }
});

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    var testAssemblies = GetFiles("./src/**/bin/Release/*.Specs.dll");
    XUnit2(testAssemblies);
});

Task("MergeLibraries")
    .IsDependentOn("Run-Unit-Tests")
    .Does(() =>
{
    var assemblyPaths = GetFiles(buildDir + "/*.dll");
    string outputFile = IoPath.Combine(outputDir, primaryDllName);
    string primaryAssembly = IoPath.Combine(buildDir, primaryDllName);

    ILRepack(outputFile, primaryAssembly, assemblyPaths);
    Information("Build directory is " + buildDir);
});

Task("Create-Nuget-Package")
    .IsDependentOn("MergeLibraries")
    .Does(() =>
{
     GitVersion version = GitVersion(new GitVersionSettings());
     var nuGetPackSettings   = new NuGetPackSettings {
                                Version                 = version.AssemblySemVer,
                                NoPackageAnalysis       = true,
                                Files                   = new [] {
                                                                    new NuSpecContent {Source = @"./output/EasyHash.dll"},
                                                                 },
                                BasePath                = ".",
                                OutputDirectory         = nugetDir
                            };

     var nuspecFiles = GetFiles("./**/EasyHash.nuspec");
     Information("Generating nuget with version " + nuGetPackSettings.Version);
     NuGetPack(nuspecFiles, nuGetPackSettings);
});

Task("Default")
    .IsDependentOn("Create-Nuget-Package");


RunTarget(target);

private void PrintGitVersion(GitVersion version) {
    Information("--------------- GitVersion Info ---------------- ");
    Information("AssemblySemVer is       " + version.AssemblySemVer);
    Information("FullSemVer is           " + version.FullSemVer);
    Information("InformationalVersion is " + version.InformationalVersion);
    Information("LegacySemVer is         " + version.LegacySemVer);
    Information("Major is                " + version.Major);
    Information("MajorMinorPatch is      " + version.MajorMinorPatch);
    Information("Minor is                " + version.Minor);
    Information("Patch is                " + version.Patch);
    Information("SemVer is               " + version.SemVer);
    Information("------------------------------------------------ ");
}