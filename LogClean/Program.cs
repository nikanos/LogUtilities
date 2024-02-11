using CommandLine;
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

            int exitCode = (int)ExitCode.Success;
            try
            {
                Validator.ValidateOptions(opts);
                SearchOption searchOption = opts.RecursiveMode ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                string[] logFiles = Directory.GetFiles(opts.LogDirectory, opts.FilePattern, searchOption);
                Log.Logger.Debug($"Found {logFiles.Length} files");
                foreach (string logFile in logFiles)
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
                                Log.Logger.Information($"Simulating deletion of {logFile}");
                            }
                            else
                            {
                                Log.Logger.Information($"deleting {logFile}");
                                File.Delete(logFile);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error(ex, "In processing exception");
                        //do not propagate exception to continue processing
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

            Log.Logger.Information($"{CurrentProgramName} Command End".DashedLine());
            return exitCode;
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
