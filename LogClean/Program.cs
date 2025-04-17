using CommandLine;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace LogClean
{
    class Program
    {
        static int Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Log.Logger = new LoggerConfiguration()
                        .ReadFrom.AppSettings()
                        .WriteTo.Console()
                        .CreateLogger();

            int result = Parser.Default.ParseArguments<Options>(args).MapResult(
                                (options) => RunOptions(options),
                                (errors) => HandleParseError(errors));
            return result;
        }

        static int RunOptions(Options opts)
        {
            Log.Logger.Information($"{CurrentProgramName} Command Start".DashedLine());
            if (opts.SimulationMode)
                Log.Logger.Information("SIMULATION MODE ON (no files will be affected)".DashedLine());
            Log.Logger.Information($"Command Line Options: {JsonConvert.SerializeObject(opts, Formatting.Indented)}");

            int exitCode = (int)ExitCode.Success;
            int cleanFileCounter = 0;
            int cleanFolderCounter = 0;
            int logFileCounter = 0;
            try
            {
                Validator.ValidateOptions(opts);
                if (opts.RecursiveMode)
                {
                    string[] allSubdirectories = Directory.GetDirectories(opts.LogDirectory, "*.*", SearchOption.AllDirectories);
                    foreach (string dir in allSubdirectories)
                    {
                        string[] dirLogFiles = Directory.GetFiles(dir, opts.FilePattern, SearchOption.TopDirectoryOnly);
                        Log.Logger.Information($"Found {dirLogFiles.Length} files in {dir}");
                        logFileCounter += dirLogFiles.Length;
                        foreach (string logFile in dirLogFiles)
                        {
                            if (DeleteLogFileIfNeeded(logFile, opts))
                                cleanFileCounter++;
                        }
                    }
                }
                string[] logFiles = Directory.GetFiles(opts.LogDirectory, opts.FilePattern, SearchOption.TopDirectoryOnly);
                Log.Logger.Information($"Found {logFiles.Length} files in {opts.LogDirectory}");
                logFileCounter += logFiles.Length;
                foreach (string logFile in logFiles)
                {
                    if (DeleteLogFileIfNeeded(logFile, opts))
                        cleanFileCounter++;
                }

                if (opts.RecursiveMode)
                    Log.Logger.Information($"Found total {logFileCounter} files in {opts.LogDirectory} and subdirectories");

                if (opts.EmptySubdirectoriesClean)
                {
                    string[] allSubdirectories = Directory.GetDirectories(opts.LogDirectory, "*.*", SearchOption.AllDirectories);
                    string[] allSubDirectoriesByDepth = allSubdirectories
                                                            .OrderByDescending(d => d.Count(c => c == '\\'))
                                                            .ToArray();
                    foreach (string dir in allSubDirectoriesByDepth)
                    {
                        DirectoryInfo di = new DirectoryInfo(dir);
                        var files = di.GetFiles();
                        if (files.Length == 0)
                        {
                            if (opts.SimulationMode)
                            {
                                Log.Logger.Verbose($"Simulating deletion of empty directory {dir}");
                            }
                            else
                            {
                                Log.Logger.Verbose($"deleting empty directory {dir}");
                                Directory.Delete(di.FullName);
                            }
                            cleanFolderCounter++;
                        }
                    }
                }
            }
            catch (OptionValidationException ex)
            {
                Log.Logger.Error(ex, "Option validation exception");
                exitCode = (int)ExitCode.InvalidArguments;
            }
            catch (IOException e)
            {
                Log.Logger.Error(e, "IO exception");
                exitCode = (int)ExitCode.ApplicationError;
            }

            if (opts.SimulationMode)
            {
                Log.Logger.Information($"Simulated deletion of {cleanFileCounter} file(s) and {cleanFolderCounter} folder(s)");
            }
            else
            {
                Log.Logger.Information($"deletion of {cleanFileCounter} file(s) and {cleanFolderCounter} folder(s)");
            }
            Log.Logger.Information($"{CurrentProgramName} Command End".DashedLine());
            return exitCode;
        }

        private static bool DeleteLogFileIfNeeded(string logFile, Options opts)
        {
            try
            {
                Log.Logger.Verbose($"Processing {logFile}");
                FileInfo fi = new FileInfo(logFile);
                TimeSpan ts = DateTime.UtcNow - fi.LastWriteTimeUtc;
                int daysOld = (int)ts.TotalDays;
                Log.Logger.Verbose($"{logFile} was modified {daysOld} day(s) ago");
                if (daysOld > opts.KeepDays)
                {
                    if (opts.SimulationMode)
                    {
                        Log.Logger.Verbose($"Simulating deletion of {logFile}");
                    }
                    else
                    {
                        Log.Logger.Verbose($"deleting {logFile}");
                        File.Delete(logFile);
                    }
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "In processing exception");
                //do not propagate exception
            }
            return false;
        }

        static int HandleParseError(IEnumerable<Error> errors)
        {
            if (errors.Any(x => x is HelpRequestedError || x is VersionRequestedError))
                return (int)ExitCode.Success;

            return (int)ExitCode.InvalidArguments;
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Logger.Error((Exception)e.ExceptionObject, "Unhandled exception");
            if (e.IsTerminating)
                Log.Logger.Information($"{CurrentProgramName} Command End".DashedLine());
        }

        private static string CurrentProgramName => Assembly.GetExecutingAssembly().GetName().Name;
    }
}
