#addin "nuget:?package=Cake.MinVer&version=1.0.1"
#addin "nuget:?package=Cake.Args&version=1.0.1"

var target       = ArgumentOrDefault<string>("target") ?? "pack";
var buildVersion = MinVer(s => s.WithTagPrefix("v").WithDefaultPreReleasePhase("preview"));

Task("clean")
    .Does(() =>
{
    CleanDirectories("./artifact/**");
    CleanDirectories("./**/^{bin,obj}");
});

Task("restore")
    .IsDependentOn("clean")
    .Does(() =>
{
    DotNetCoreRestore("./Ookii.Dialogs.Wpf.sln", new DotNetCoreRestoreSettings
    {
        LockedMode = true,
    });
});

Task("build")
    .IsDependentOn("restore")
    .DoesForEach(new[] { "Debug", "Release" }, (configuration) =>
{
    DotNetCoreBuild("./Ookii.Dialogs.Wpf.sln", new DotNetCoreBuildSettings
    {
        Configuration = configuration,
        NoRestore = true,
        NoIncremental = false,
        MSBuildSettings = new DotNetCoreMSBuildSettings()
            .WithProperty("Version", buildVersion.Version)
            .WithProperty("AssemblyVersion", buildVersion.AssemblyVersion)
            .WithProperty("FileVersion", buildVersion.FileVersion)
            .WithProperty("ContinuousIntegrationBuild", BuildSystem.IsLocalBuild ? "false" : "true")
    });
});

Task("test")
    .IsDependentOn("build")
    .Does(() =>
{
    var settings = new DotNetCoreTestSettings
    {
        Configuration = "Release",
        NoRestore = true,
        NoBuild = true,
    };

    var projectFiles = GetFiles("./test/**/*.csproj");
    foreach (var file in projectFiles)
    {
        DotNetCoreTest(file.FullPath, settings);
    }
});

Task("pack")
    .IsDependentOn("test")
    .Does(() =>
{
    var releaseNotes = $"https://github.com/augustoproiete/ookii-dialogs-wpf/releases/tag/v{buildVersion.Version}";

    DotNetCorePack("./src/Ookii.Dialogs.Wpf/Ookii.Dialogs.Wpf.csproj", new DotNetCorePackSettings
    {
        Configuration = "Release",
        NoRestore = true,
        NoBuild = true,
        IncludeSymbols = true,
        IncludeSource = true,
        OutputDirectory = "./artifact/nuget",
        MSBuildSettings = new DotNetCoreMSBuildSettings()
            .WithProperty("Version", buildVersion.Version)
            .WithProperty("PackageReleaseNotes", releaseNotes)
    });

    DotNetCorePack("./src/Ookii.Dialogs/Ookii.Dialogs.csproj", new DotNetCorePackSettings
    {
        Configuration = "Release",
        NoRestore = true,
        NoBuild = true,
        IncludeSymbols = false,
        IncludeSource = false,
        OutputDirectory = "./artifact/nuget",
        MSBuildSettings = new DotNetCoreMSBuildSettings()
            .WithProperty("Version", buildVersion.Version)
            .WithProperty("PackageReleaseNotes", releaseNotes)
    });
});

Task("push")
    .IsDependentOn("pack")
    .Does(context =>
{
    var url =  context.EnvironmentVariable("NUGET_URL");
    if (string.IsNullOrWhiteSpace(url))
    {
        context.Information("No NuGet URL specified. Skipping publishing of NuGet packages");
        return;
    }

    var apiKey =  context.EnvironmentVariable("NUGET_API_KEY");
    if (string.IsNullOrWhiteSpace(apiKey))
    {
        context.Information("No NuGet API key specified. Skipping publishing of NuGet packages");
        return;
    }

    var nugetPushSettings = new DotNetCoreNuGetPushSettings
    {
        Source = url,
        ApiKey = apiKey,
    };

    foreach (var nugetPackageFile in GetFiles("./artifact/nuget/*.nupkg"))
    {
        DotNetCoreNuGetPush(nugetPackageFile.FullPath, nugetPushSettings);
    }
});

RunTarget(target);
