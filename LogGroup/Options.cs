using CommandLine;

namespace LogGroup
{
    class Options
    {
        [Option('d', "directory", Required = true, HelpText = "Log directory.")]
        public string LogDirectory { get; set; }

        [Option('t', "target", Required = true, HelpText = "Target directory.")]
        public string TargetDirectory { get; set; }

        [Option('p', "pattern", HelpText = "File pattern.", Default = "*.*")]
        public string FilePattern { get; set; }

        [Option('r', "recursive", HelpText = "Enable recursive mode.")]
        public bool RecursiveMode { get; set; }

        [Option('s', "simulation", HelpText = "Enable simulation mode.")]
        public bool SimulationMode { get; set; }

    }
}
