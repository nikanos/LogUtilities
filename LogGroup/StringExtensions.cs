using System;

namespace LogGroup
{
    public static class StringExtensions
    {
        public static string DashedLine(this string str, int lineLengh)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));

            const char DASH_CHAR = '-';
            int dashPartLength = (lineLengh - str.Length) / 2;
            string dashLeftPart = new string(DASH_CHAR, dashPartLength);
            return $"{dashLeftPart}{str}".PadRight(lineLengh, DASH_CHAR);
        }

        public static string DashedLine(this string str)
        {
            return DashedLine(str, Constants.DEFAULT_LINE_WIDTH);
        }
    }
}
