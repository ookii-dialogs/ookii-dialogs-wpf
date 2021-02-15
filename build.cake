#addin "nuget:?package=Cake.MinVer&version=1.0.0"

var target       = Argument<string>("target", "pack");
var buildVersion = MinVer(s => s.WithTagPrefix("v").WithDefaultPreReleasePhase("preview"));

Task("clean")
    .Does(() =>
{
    CleanDirectory("./build/artifacts");
    CleanDirectories("./src/**/bin");
    CleanDirectories("./src/**/obj");
    CleanDirectories("./test/**/bin");
    CleanDirectories("./test/**/obj");
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
    .Does(() =>
{
    DotNetCoreBuild("./Ookii.Dialogs.Wpf.sln", new DotNetCoreBuildSettings
    {
        Configuration = "Debug",
        NoRestore = true,
        NoIncremental = false,
        ArgumentCustomization = args =>
            args.AppendQuoted($"-p:Version={buildVersion.Version}")
                .AppendQuoted($"-p:FileVersion={buildVersion.FileVersion}")
                .AppendQuoted($"-p:ContinuousIntegrationBuild=true")
    });

    DotNetCoreBuild("./Ookii.Dialogs.Wpf.sln", new DotNetCoreBuildSettings
    {
        Configuration = "Release",
        NoRestore = true,
        NoIncremental = false,
        ArgumentCustomization = args =>
            args.AppendQuoted($"-p:Version={buildVersion.Version}")
                .AppendQuoted($"-p:FileVersion={buildVersion.FileVersion}")
                .AppendQuoted($"-p:ContinuousIntegrationBuild=true")
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
        OutputDirectory = "./build/artifacts",
        ArgumentCustomization = args =>
            args.AppendQuoted($"-p:Version={buildVersion.Version}")
                .AppendQuoted($"-p:PackageReleaseNotes={releaseNotes}")
    });

    DotNetCorePack("./src/Ookii.Dialogs/Ookii.Dialogs.csproj", new DotNetCorePackSettings
    {
        Configuration = "Release",
        NoRestore = true,
        NoBuild = true,
        IncludeSymbols = false,
        IncludeSource = false,
        OutputDirectory = "./build/artifacts",
        ArgumentCustomization = args =>
            args.AppendQuoted($"-p:Version={buildVersion.Version}")
                .AppendQuoted($"-p:PackageReleaseNotes={releaseNotes}")
    });
});

Task("publish")
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

    foreach (var nugetPackageFile in GetFiles("./build/artifacts/*.nupkg"))
    {
        DotNetCoreNuGetPush(nugetPackageFile.FullPath, nugetPushSettings);
    }
});

RunTarget(target);
