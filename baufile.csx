using System.Diagnostics;

var version = File.ReadAllText("src/CommonAssemblyInfo.cs").Split(new[] { "AssemblyInformationalVersion(\"" }, 2, StringSplitOptions.None).ElementAt(1).Split(new[] { '"' }).First();
var msBuildCommand = Path.Combine(Environment.GetEnvironmentVariable("WINDIR"), @"Microsoft.NET\Framework\v4.0.30319\MSBuild.exe");
var nugetCommand = @"packages\NuGet.CommandLine.2.8.1\tools\NuGet.exe";
var xunitCommand = @"packages\xunit.runners.1.9.2\tools\xunit.console.clr4.exe";
var solution = @"src\Bau.sln";
var output = "artifacts";
var component = @"src\test\Bau.Test.Component\bin\Release\Bau.Test.Component.dll";
var acceptance = @"src\test\Bau.Test.Acceptance\bin\Release\Bau.Test.Acceptance.dll";
var nuspecs = new[] { @"src\Bau\Bau.csproj", @"src\Bau.Exec\Bau.Exec.csproj", };

Require<BauPack>()
.Task("default").DependsOn("component", "accept", "pack")
.Exec("clean")
    .Do(exec =>
    {
        if (Directory.Exists(output))
        {
            Directory.Delete(output, true);
        }

        exec.Run(msBuildCommand).With(solution, "/target:Clean", "/property:Configuration=Release");
    })
.Exec("restore")
    .Do(exec => exec
        .Run(nugetCommand)
        .With("restore", solution))
.Exec("build").DependsOn("clean", "restore")
    .Do(exec => exec
        .Run(msBuildCommand)
        .With(solution, "/target:Build", "/property:Configuration=Release"))
.Exec("component").DependsOn("build")
    .Do(exec => exec
        .Run(xunitCommand)
        .With(component, "/html", component + "TestResults.html", "/xml", component + "TestResults.xml"))
.Exec("accept").DependsOn("build")
    .Do(exec => exec
        .Run(xunitCommand)
        .With(acceptance, "/html", acceptance + "TestResults.html", "/xml", acceptance + "TestResults.xml"))
.Task("pack").DependsOn("build")
    .Do(() =>
    {
        Directory.CreateDirectory(output);
        foreach (var nuspec in nuspecs)
        {
            new ExecTask()
                .Run(nugetCommand)
                .With(
                    "pack", nuspec,
                    "-Version", version,
                    "-OutputDirectory", output,
                    "-Properties", "Configuration=Release",
                    "-IncludeReferencedProjects")
                .Execute();
        }
    })
.Execute();
