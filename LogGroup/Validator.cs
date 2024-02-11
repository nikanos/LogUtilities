using System;
using System.IO;
using System.Linq;

namespace LogGroup
{
    static class Validator
    {
        internal static void ValidateConfiguration()
        {
            if (MySettings.Default.MinutesLastModifiedSkip <= 0)
            {
                throw new ConfigurationValidationException($"MinutesLastModifiedSkip ({MySettings.Default.MinutesLastModifiedSkip}) should be greater than zero");
            }

            if (!MySettings.Default.GroupName.Contains(Constants.GROUP_DATE_FORMAT_TEMPLATE))
            {
                throw new ConfigurationValidationException($"GroupName ({MySettings.Default.GroupName}) should contain the tag {Constants.GROUP_DATE_FORMAT_TEMPLATE}");
            }
        }

        internal static void ValidateOptions(Options opts)
        {
            if (opts == null)
                throw new ArgumentNullException(nameof(opts));

            var allowedDirectories = MySettings.Default.AllowedFolders.Cast<string>();

            string logDirectory = opts.LogDirectory;
            if (!allowedDirectories.Any(x => x.Equals(logDirectory, StringComparison.OrdinalIgnoreCase)))
            {
                throw new OptionValidationException($"Log Directory {logDirectory} is not in the allowed list");
            }

            string targetDirectory = opts.TargetDirectory;
            if (!allowedDirectories.Any(x => x.Equals(targetDirectory, StringComparison.OrdinalIgnoreCase)))
            {
                throw new OptionValidationException($"Target Directory {targetDirectory} is not in the allowed list");
            }

            if (logDirectory.Equals(targetDirectory, StringComparison.OrdinalIgnoreCase))
            {
                throw new OptionValidationException("Log Directory and Target Directory should not be the same");
            }

            if (!Directory.Exists(targetDirectory))
            {
                throw new OptionValidationException($"Target Directory {targetDirectory} does not exist");
            }
        }
    }
}
