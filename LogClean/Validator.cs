using System;
using System.Linq;

namespace LogClean
{
    static class Validator
    {
        internal static void ValidateOptions(Options opts)
        {
            if (opts == null)
                throw new ArgumentNullException(nameof(opts));

            string logDirectory = opts.LogDirectory;
            var allowedDirectories = MySettings.Default.AllowedFolders.Cast<string>();
            if (!allowedDirectories.Any(x => x.Equals(logDirectory, StringComparison.OrdinalIgnoreCase)))
            {
                throw new OptionValidationException($"{logDirectory} is not in the allowed list");
            }

            int keepDays = opts.KeepDays;
            int allowedKeepDaysMin = MySettings.Default.AllowedKeepDaysMin;
            if (keepDays < allowedKeepDaysMin)
            {
                throw new OptionValidationException($"KeepDays should not be less than {allowedKeepDaysMin}");
            }
        }
    }
}
