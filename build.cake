#tool "nuget:?package=xunit.runner.console"
#tool "nuget:?package=GitVersion.CommandLine"
#tool "nuget:?package=ILRepack"
#load "build/utilities.cake"

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

Task("Update-Assembly-Info")
    .Does(() =>
{
    GitVersion version = GitVersion(new GitVersionSettings { UpdateAssemblyInfo = true });
    PrintGitVersion(version);
});

Task("Build")
    .IsDependentOn("Update-Assembly-Info")
    .IsDependentOn("Restore-NuGet-Packages")
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

Task("Merge-Libraries")
    .IsDependentOn("Run-Unit-Tests")
    .Does(() =>
{
    FilePathCollection assemblyPaths = GetFiles(buildDir + "/*.dll");
    string outputFile = IoPath.Combine(outputDir, primaryDllName);
    string primaryAssembly = IoPath.Combine(buildDir, primaryDllName);

    ILRepack(outputFile, primaryAssembly, assemblyPaths);
    Information("Build directory is " + buildDir);
});

Task("Create-Nuget-Package")
    .IsDependentOn("Merge-Libraries")
    .Does(() =>
{
     GitVersion version = GitVersion(new GitVersionSettings());
     var nuGetPackSettings   = new NuGetPackSettings {
                                Version                 = version.AssemblySemVer,
                                NoPackageAnalysis       = true,
                                Files                   = new [] {
                                                                    new NuSpecContent {Source = @"./output/EasyHash.dll", Target = "lib/net45" },
                                                                 },
                                BasePath                = ".",
                                OutputDirectory         = nugetDir
                            };

     FilePathCollection nuspecFiles = GetFiles("./**/EasyHash.nuspec");
     Information("Generating nuget with version " + nuGetPackSettings.Version);
     NuGetPack(nuspecFiles, nuGetPackSettings);
});

Task("Push-Nuget-Package")
    .IsDependentOn("Create-Nuget-Package")
    .Does(() =>
{
    string semVersion = GitVersion(new GitVersionSettings()).SemVer;
    ConvertableFilePath package = Directory(nugetDir) + File("EasyHash." + semVersion + ".nupkg");

    Information("Publishing nuget package " + package);
    NuGetPush(package, new NuGetPushSettings {
        Source = "https://api.nuget.org/v3/index.json",
        ApiKey = GetEnvironmentVariable("NugetApiKey")
    });
});

Task("Default").IsDependentOn("Run-Unit-Tests");
Task("Package").IsDependentOn("Create-Nuget-Package");
Task("Publish").IsDependentOn("Push-Nuget-Package");
RunTarget(target);
