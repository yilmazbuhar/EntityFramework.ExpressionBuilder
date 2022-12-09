using System.Globalization;

namespace LambdaBuilder
{
    public class CultureFormatHelper
    {
        public static DateTime? ParseDateTimeStringFromCulture(string datestring, CultureInfo cultureInfo)
        {
            var shortDatePattern = cultureInfo.DateTimeFormat.ShortDatePattern;
            string shortTimePattern = cultureInfo.DateTimeFormat.ShortTimePattern;

            string longDateTimePattern = $"{shortDatePattern} {shortTimePattern}";

            if (DateTime.TryParseExact(datestring, 
                shortDatePattern, 
                CultureInfo.InvariantCulture, 
                DateTimeStyles.AdjustToUniversal, 
                out var date))
            {
                return date;
            }

            if (DateTime.TryParseExact(datestring, 
                longDateTimePattern, 
                CultureInfo.InvariantCulture, 
                DateTimeStyles.AdjustToUniversal, 
                out var datetime))
            {
                return datetime;
            }


            throw new FormatException($"Datetime format must be {shortDatePattern} or {longDateTimePattern} for your culture. You can specify culture on query.");
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
