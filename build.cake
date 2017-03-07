#tool "nuget:?package=xunit.runner.console"
#tool "nuget:?package=GitVersion.CommandLine"

using Cake.Common.Tools.GitVersion;

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
string buildDir = Directory("./src/EasyHash/bin") + Directory(configuration);
string relativeSlnPath = "./src/EasyHash.sln";


Task("Clean")
    .Does(() =>
{
    #break
    Information("Build directory is " + buildDir);
    CleanDirectory(buildDir);
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
    Information("AssemblySemVer is " + version.AssemblySemVer);
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

Task("Default")
    .IsDependentOn("Run-Unit-Tests");


RunTarget(target);