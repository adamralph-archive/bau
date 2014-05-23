﻿// <copyright file="TaskDependencies.cs" company="Bau contributors">
//  Copyright (c) Bau contributors. (baubuildch@gmail.com)
// </copyright>

namespace Bau.Test.Acceptance
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using Bau.Test.Acceptance.Support;
    using FluentAssertions;
    using Xbehave;
    using Xunit;

    public static class TaskDependencies
    {
        // happy path
        [Scenario]
        public static void SingleDependency(Baufile baufile, string tempFile, string[] executedTasks, string output)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given bau is required"
                .f(() => baufile = Baufile.Create(scenario).WriteLine(
@"var bau = Require<Bau>();"));

            "And a non-default task"
                .f(c => baufile.WriteLine(
@"
var executed = new List<string>();

bau.Task(""non-default"")
.Do(() => executed.Add(""non-default""));"));

            "And a default task which depends on the non-default task"
                .f(() =>
            {
                tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture));
                baufile.WriteLine(
@"
bau.Task(""default"")
.DependsOn(""non-default"")
.Do(() =>
{
    executed.Add(""default"");
    using(var file = File.CreateText(@""" + tempFile + @"""))
    {
        file.Write(string.Join(Environment.NewLine, executed));
    };
});");
            })
            .Teardown(() => File.Delete(tempFile));

            "And the tasks are executed"
                .f(() => baufile.WriteLine(
@"
bau.Run();"));

            "When I execute the baufile"
                .f(() => output = baufile.Run());

            "Then two tasks are executed"
                .f(() =>
                {
                    File.Exists(tempFile).Should().BeTrue();
                    executedTasks = File.ReadAllText(tempFile).Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    executedTasks.Length.Should().Be(2);
                });

            "And the non-default task is executed first"
                .f(() => executedTasks[0].Should().Be("non-default"));

            "And the default task is executed second"
                .f(() => executedTasks[1].Should().Be("default"));

            "And I am informed that the non-default task was executed"
                .f(() => output.Should().ContainEquivalentOf("starting 'non-default'"));

            "And I am informed that the default task was executed"
                .f(() => output.Should().ContainEquivalentOf("starting 'default'"));
        }

        [Scenario]
        public static void MultipleDependencies(Baufile baufile, string tempFile, string[] executedTasks, string output)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given bau is required"
                .f(() => baufile = Baufile.Create(scenario).WriteLine(
@"var bau = Require<Bau>();"));

            "And a non-default task"
                .f(c => baufile.WriteLine(
@"
var executed = new List<string>();

bau.Task(""non-default1"")
.Do(() => executed.Add(""non-default1""));"));

            "And a second non-default task"
                .f(c => baufile.WriteLine(
@"
bau.Task(""non-default2"")
.Do(() => executed.Add(""non-default2""));"));

            "And a default task which depends on both non-default tasks"
                .f(() =>
                {
                    tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture));
                    baufile.WriteLine(
@"
bau.Task(""default"")
.DependsOn(""non-default1"", ""non-default2"")
.Do(() =>
{
    executed.Add(""default"");
    using(var file = File.CreateText(@""" + tempFile + @"""))
    {
        file.Write(string.Join(Environment.NewLine, executed));
    };
});");
                })
                .Teardown(() => File.Delete(tempFile));

            "And the tasks are executed"
                .f(() => baufile.WriteLine(
@"
bau.Run();"));

            "When I execute the baufile"
                .f(() => output = baufile.Run());

            "Then three tasks are executed"
                .f(() =>
                {
                    File.Exists(tempFile).Should().BeTrue();
                    executedTasks = File.ReadAllText(tempFile).Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    executedTasks.Length.Should().Be(3);
                });

            "And the first non-default task is executed first"
                .f(() => executedTasks[0].Should().Be("non-default1"));

            "And the second non-default task is executed second"
                .f(() => executedTasks[1].Should().Be("non-default2"));

            "And the default task is executed third"
                .f(() => executedTasks[2].Should().Be("default"));

            "And I am informed that the first non-default task was executed"
                .f(() => output.Should().ContainEquivalentOf("starting 'non-default1'"));

            "And I am informed that the second non-default task was executed"
                .f(() => output.Should().ContainEquivalentOf("starting 'non-default2'"));

            "And I am informed that the default task was executed"
                .f(() => output.Should().ContainEquivalentOf("starting 'default'"));
        }

        [Scenario]
        public static void NestedDependencies(Baufile baufile, string tempFile, string[] executedTasks, string output)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given bau is required"
                .f(() => baufile = Baufile.Create(scenario).WriteLine(
@"var bau = Require<Bau>();"));

            "And a non-default task"
                .f(c => baufile.WriteLine(
@"
var executed = new List<string>();

bau.Task(""non-default1"")
.Do(() => executed.Add(""non-default1""));"));

            "And a second non-default task which depends on the first non-default task"
                .f(c => baufile.WriteLine(
@"
bau.Task(""non-default2"")
.DependsOn(""non-default1"")
.Do(() => executed.Add(""non-default2""));"));

            "And a default task which depends on the second non-default task"
                .f(() =>
                {
                    tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture));
                    baufile.WriteLine(
    @"
bau.Task(""default"")
.DependsOn(""non-default2"")
.Do(() =>
{
    executed.Add(""default"");
    using(var file = File.CreateText(@""" + tempFile + @"""))
    {
        file.Write(string.Join(Environment.NewLine, executed));
    };
});");
                })
                .Teardown(() => File.Delete(tempFile));

            "And the tasks are executed"
                .f(() => baufile.WriteLine(
@"
bau.Run();"));

            "When I execute the baufile"
                .f(() => output = baufile.Run());

            "Then three tasks are executed"
                .f(() =>
                {
                    File.Exists(tempFile).Should().BeTrue();
                    executedTasks = File.ReadAllText(tempFile).Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    executedTasks.Length.Should().Be(3);
                });

            "And the first non-default task is executed first"
                .f(() => executedTasks[0].Should().Be("non-default1"));

            "And the second non-default task is executed second"
                .f(() => executedTasks[1].Should().Be("non-default2"));

            "And the default task is executed third"
                .f(() => executedTasks[2].Should().Be("default"));

            "And I am informed that the first non-default task was executed"
                .f(() => output.Should().ContainEquivalentOf("starting 'non-default1'"));

            "And I am informed that the second non-default task was executed"
                .f(() => output.Should().ContainEquivalentOf("starting 'non-default2'"));

            "And I am informed that the default task was executed"
                .f(() => output.Should().ContainEquivalentOf("starting 'default'"));
        }

        [Scenario]
        public static void RepeatedDependency(Baufile baufile, string tempFile, string[] executedTasks, string output)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given bau is required"
                .f(() => baufile = Baufile.Create(scenario).WriteLine(
@"var bau = Require<Bau>();"));

            "And a non-default task"
                .f(c => baufile.WriteLine(
@"
var executed = new List<string>();

bau.Task(""non-default1"")
.Do(() => executed.Add(""non-default1""));"));

            "And a second non-default task which depends on the first non-default task"
                .f(c => baufile.WriteLine(
@"
bau.Task(""non-default2"")
.DependsOn(""non-default1"")
.Do(() => executed.Add(""non-default2""));"));

            "And a default task which depends on both the non-default tasks"
                .f(() =>
                {
                    tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture));
                    baufile.WriteLine(
@"
bau.Task(""default"")
.DependsOn(""non-default2"", ""non-default1"")
.Do(() =>
{
    executed.Add(""default"");
    using(var file = File.CreateText(@""" + tempFile + @"""))
    {
        file.Write(string.Join(Environment.NewLine, executed));
    };
});");
                })
                .Teardown(() => File.Delete(tempFile));

            "And the tasks are executed"
                .f(() => baufile.WriteLine(
@"
bau.Run();"));

            "When I execute the baufile"
                .f(() => output = baufile.Run());

            "Then three tasks are executed"
                .f(() =>
                {
                    File.Exists(tempFile).Should().BeTrue();
                    executedTasks = File.ReadAllText(tempFile).Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    executedTasks.Length.Should().Be(3);
                });

            "And the first non-default task is executed first"
                .f(() => executedTasks[0].Should().Be("non-default1"));

            "And the second non-default task is executed second"
                .f(() => executedTasks[1].Should().Be("non-default2"));

            "And the default task is executed third"
                .f(() => executedTasks[2].Should().Be("default"));

            "And I am informed that the first non-default task was executed"
                .f(() => output.Should().ContainEquivalentOf("starting 'non-default1'"));

            "And I am informed that the second non-default task was executed"
                .f(() => output.Should().ContainEquivalentOf("starting 'non-default2'"));

            "And I am informed that the default task was executed"
                .f(() => output.Should().ContainEquivalentOf("starting 'default'"));
        }

        [Scenario]
        public static void CircularDependency(Baufile baufile, string tempFile, string[] executedTasks, string output)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given bau is required"
                .f(() => baufile = Baufile.Create(scenario).WriteLine(
@"var bau = Require<Bau>();"));

            "And a non-default task which depends on the default task"
                .f(c => baufile.WriteLine(
@"
var executed = new List<string>();

bau.Task(""non-default"")
.DependsOn(""default"")
.Do(() => executed.Add(""non-default""));"));

            "And a default task which depends on the non-default task"
                .f(() =>
                {
                    tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture));
                    baufile.WriteLine(
@"
bau.Task(""default"")
.DependsOn(""non-default"")
.Do(() =>
{
    executed.Add(""default"");
    using(var file = File.CreateText(@""" + tempFile + @"""))
    {
        file.Write(string.Join(Environment.NewLine, executed));
    };
});");
                })
                .Teardown(() => File.Delete(tempFile));

            "And the tasks are executed"
                .f(() => baufile.WriteLine(
@"
bau.Run();"));

            "When I execute the baufile"
                .f(() => output = baufile.Run());

            "Then two tasks are executed"
                .f(() =>
                {
                    File.Exists(tempFile).Should().BeTrue();
                    executedTasks = File.ReadAllText(tempFile).Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    executedTasks.Length.Should().Be(2);
                });

            "And the non-default task is executed first"
                .f(() => executedTasks[0].Should().Be("non-default"));

            "And the default task is executed second"
                .f(() => executedTasks[1].Should().Be("default"));

            "And I am informed that the non-default task was executed"
                .f(() => output.Should().ContainEquivalentOf("starting 'non-default'"));

            "And I am informed that the default task was executed"
                .f(() => output.Should().ContainEquivalentOf("starting 'default'"));
        }

        // sad path
        [Scenario]
        public static void NonexistentDependency(Baufile baufile, string tempFile, Exception ex)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given a default task with a non-existent dependency"
                .f(() =>
                {
                    tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture));
                    baufile = Baufile.Create(scenario).WriteLine(
@"Require<Bau>().Task(""default"").DependsOn(""non-existent"")
.Do(() => File.CreateText(@""" + tempFile + @""").Dispose()).Run();");
                })
                .Teardown(() => File.Delete(tempFile));

            "When I execute the baufile"
                .f(() => ex = Record.Exception(() => baufile.Run()));

            "Then execution should fail"
                .f(() => ex.Should().NotBeNull());

            "And the task is not executed"
                .f(() => File.Exists(tempFile).Should().BeFalse());

            "And I am informed that the non-existent task was not found"
                .f(() => ex.Message.Should().ContainEquivalentOf("'non-existent' task not found"));
        }

        [Scenario]
        public static void DependencyFails(Baufile baufile, string tempFile, Exception ex)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given bau is required"
                .f(() => baufile = Baufile.Create(scenario).WriteLine(
@"var bau = Require<Bau>();"));

            "And a non-default task which fails"
                .f(c => baufile.WriteLine(
@"
bau.Task(""non-default"")
.Do(() => { throw new Exception();} );"));

            "And a default task which depends on the non-default task"
                .f(() =>
                {
                    tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture));
                    baufile.WriteLine(
@"
bau.Task(""default"")
.DependsOn(""non-default"")
.Do(() => File.CreateText(@""" + tempFile + @""").Dispose());");
                })
                .Teardown(() => File.Delete(tempFile));

            "And the tasks are executed"
                .f(() => baufile.WriteLine(
@"
bau.Run();"));

            "When I execute the baufile"
                .f(() => ex = Record.Exception(() => baufile.Run()));

            "Then execution should fail"
                .f(() => ex.Should().NotBeNull());

            "And the default task is not executed"
                .f(() => File.Exists(tempFile).Should().BeFalse());

            "And I am informed that the non-default task was executed"
                .f(() => ex.Message.Should().ContainEquivalentOf("'non-default' task failed."));
        }
    }
}
