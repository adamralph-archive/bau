﻿// <copyright file="Program.cs" company="Bau contributors">
//  Copyright (c) Bau contributors. (baubuildch@gmail.com)
// </copyright>

namespace Bau
{
    using System;
    using System.Globalization;
    using CommandLine;
    using Common.Logging;
    using Common.Logging.Simple;
    using ServiceStack.Text;

    internal static class Program
    {
        public static int Main(string[] args)
        {
            var arguments = new Arguments();
            var parser = new Parser(settings =>
            {
                settings.CaseSensitive = true;
                settings.HelpWriter = Console.Out;
            });

            if (!parser.ParseArguments(args, arguments))
            {
                return 1;
            }

            if (arguments.Silent)
            {
                LogManager.Adapter = new ConsoleOutLoggerFactoryAdapter(Common.Logging.LogLevel.Off, false, false, false, "u");
            }
            else if (arguments.Quiet)
            {
                LogManager.Adapter = new ConsoleOutLoggerFactoryAdapter(Common.Logging.LogLevel.Warn, false, false, false, "u");
            }
            else if (arguments.Verbose)
            {
                LogManager.Adapter = new ConsoleOutLoggerFactoryAdapter(Common.Logging.LogLevel.Debug, true, true, true, "u");
            }
            else if (arguments.Trace)
            {
                LogManager.Adapter = new ConsoleOutLoggerFactoryAdapter(Common.Logging.LogLevel.Trace, true, true, true, "u");
            }
            else
            {
                LogManager.Adapter = new ConsoleOutLoggerFactoryAdapter(Common.Logging.LogLevel.Info, false, false, false, "u");
            }

            var log = LogManager.GetCurrentClassLogger();

            AppDomain.CurrentDomain.FirstChanceException +=
                (object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e) =>
                log.Trace("First chance exception.", e.Exception);

            AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs e) =>
            {
                var message = "Bau FAILED!";
                if (arguments.Verbose || arguments.Trace)
                {
                    log.Fatal(message, (Exception)e.ExceptionObject);
                }
                else
                {
                    log.FatalFormat(CultureInfo.InvariantCulture, "{0} {1}", message, ((Exception)e.ExceptionObject).Message);
                }

                Environment.Exit(1);
            };

            log.DebugFormat(CultureInfo.InvariantCulture, "Parsed arguments: {0}", arguments.ToJsv());

            var application = ApplicationFactory.Create(BaufileFinder.Find(), arguments);
            application.Execute();

            return 0;
        }
    }
}
