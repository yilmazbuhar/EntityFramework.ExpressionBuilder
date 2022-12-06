using System.Globalization;

namespace LambdaBuilder
{
    public class CultureFormatter
    {
        public static DateTime? ParseDateTimeStringFromCulture(string datestring, CultureInfo cultureInfo)
        {
            var datetimeformatString = cultureInfo.DateTimeFormat.ShortDatePattern;
            string shortTimeFormatString = cultureInfo.DateTimeFormat.ShortTimePattern;

            string format = $"{datetimeformatString} {shortTimeFormatString}";

            return ParseDateTimeStringFromFormat(datestring, format);
        }

        // OpenAI wrote this code
        public static DateTime? ParseDateTimeStringFromFormat(string datestring, string format)
        {
            // Define a nullable DateTime variable
            DateTime? dateValue = null;

            // Parse the string and try to convert it to a DateTime
            if (DateTime.TryParseExact(datestring, format, null, DateTimeStyles.None, out DateTime parsedDate))
            {
                // If the conversion succeeded, assign the parsed DateTime to the nullable DateTime variable
                dateValue = parsedDate;
            }

            return dateValue;
        }
    }
}
