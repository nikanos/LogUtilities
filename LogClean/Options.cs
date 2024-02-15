using CommandLine;

namespace LogClean
{
    class Options
    {
        [Option('d', "directory", Required = true, HelpText = "Log directory.")]
        public string LogDirectory { get; set; }

        [Option('p', "pattern", HelpText = "File pattern.", Default = "*.*")]
        public string FilePattern { get; set; }

        [Option('r', "recursive", HelpText = "Enable recursive mode.")]
        public bool RecursiveMode { get; set; }

        [Option('s', "simulation", HelpText = "Enable simulation mode.")]
        public bool SimulationMode { get; set; }

        [Option('k', "keep-days", HelpText = "Days to keep logs.", Default = Constants.DEFAULT_KEEP_DAYS)]
        public int KeepDays { get; set; }

        [Option('e', "empty-subdirectories-clean", HelpText = "Delete empty subdirectories.")]
        public bool  EmptySubdirectoriesClean { get; set; }
    }
}
