using Newtonsoft.Json;
using System.Globalization;
using TimeZoneConverter;

namespace Ihelpers.Helpers
{
    public class TimezoneHelper
    {
        public static string getTimezoneOffset(string? timezoneSufix, DateTime? date = null)
        {
            
                try
                {
                    TimeZoneInfo tzi = TZConvert.GetTimeZoneInfo(timezoneSufix?.Trim());

                    TimeZoneInfo zone = TimeZoneInfo.FindSystemTimeZoneById(tzi.Id);

                    TimeSpan offset = zone.GetUtcOffset(date ?? DateTime.UtcNow);

                    return offset.ToString();
                }
                catch
                {
                    return "00:00";
                }

        }

    }


    public static class DateTimeExtensions
    {
        /// <summary>
        /// Adds or substracts time portions from given dateTime depending on negate parameter and timezone offset
        /// </summary>
        /// <param name="wichDate">The dateTime to add or substract time</param>
        /// <param name="offset">The timezone offset exm: "-04:00"</param>
        /// <param name="negate">Depends when the dateTime data needs to be sent to front end or sent to database</param>
        /// <returns></returns>
        public static DateTime SetTimezoneOffset(this DateTime wichDate, string? offset = "00:00", bool negate = false)
        {

            //Get raw TimesPan valuevalue
            TimeSpan rawUserTimezone = negate ? TimeSpan.Parse(offset).Negate() : TimeSpan.Parse(offset);


            //convert to UTC
            wichDate = wichDate.Add(rawUserTimezone);

            return wichDate;
        }   
        
        /// <summary>
        /// Calculates the POSIX epoch for given date.
        /// </summary>
        /// <param name="wichDate">The dateTime to calculate posix epoch</param>
        /// <returns>The posix epoch of given date</returns>
        public static long GetDateTimeEpoch(this DateTime wichDate)
        {
            return (long)(wichDate - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }

        /// <summary>
        /// Adds or substracts time portions from given dateTime depending on negate parameter and timezone offset
        /// </summary>
        /// <param name="wichDate">The dateTime to add or substract time</param>
        /// <param name="timezone">The timezone IANA</param>
        /// <param name="negate">Depends when the dateTime data needs to be sent to front end or sent to database</param>
        /// <returns></returns>
        public static DateTime FromDateToTimezone(this DateTime wichDate, string? timezone = "", bool negate = false)
        {

            string offset = TimezoneHelper.getTimezoneOffset(timezone, wichDate);

            //Get raw TimesPan valuevalue
            TimeSpan rawUserTimezone = negate ? TimeSpan.Parse(offset).Negate() : TimeSpan.Parse(offset);

            //convert to UTC
            wichDate = wichDate.Add(rawUserTimezone);

            return wichDate;
        }

        /// <summary>
        /// This method attempts to parse the given string representation of a date and time
        /// using the formats specified in the configuration file. If a specific format is set,
        /// it will be used. Otherwise, the default parser will be used.
        /// </summary>
        /// <param name="wichDateString">The string representation of the date and time to parse</param>
        /// <returns>A nullable DateTime representing the parsed date and time</returns>
        public static DateTime? ParseWithConfig(string? wichDateString)
        {
            DateTime? response = null;
            if (!string.IsNullOrEmpty(wichDateString))
            {
                string[]? validDTFormats = ConfigurationHelper.GetConfig<string[]?>("DefaultConfigs:DateFormats");

                // Check if the list of valid date formats is not null
                if (validDTFormats != null)
                {
                    // If specific format was set, use it to parse the date string
                    response = DateTime.ParseExact(wichDateString, validDTFormats, null);
                }
                else
                {
                    // If no specific format was set, use the default parser to parse the date string
                    response = DateTime.Parse(wichDateString);
                }
            }

            // Return the parsed date and time
            return response;
        }

        /// <summary>
        /// Returns the first day of the month for the specified date.
        /// </summary>
        /// <param name="wichDate">The date for which to find the first day of the month.</param>
        /// <returns>A DateTime representing the first day of the month.</returns>
        public static DateTime? FirstDayOfMonth(this DateTime wichDate)
        {
            return new DateTime(wichDate.Year, wichDate.Month, 1);
        }

        /// <summary>
        /// Returns the last day of the month for the specified date.
        /// </summary>
        /// <param name="wichDate">The date for which to find the last day of the month.</param>
        /// <returns>A DateTime representing the last day of the month.</returns>
        public static DateTime? LastDayOfMonth(this DateTime wichDate)
        {
            return wichDate.FirstDayOfMonth().Value.AddMonths(1).AddMinutes(-1);
        }

        /// <summary>
        /// Returns a SQL string representation of the specified date.
        /// </summary>
        /// <param name="wichDate">The date to convert to a SQL string representation.</param>
        /// <returns>A SQL string representation of the specified date.</returns>
        public static string? ToSqlString(this DateTime wichDate)
        {
            return wichDate.ToString("yyyy-MM-dd HH:mm");
        }

        public static (string, string) GetRangeOfTimeFromTo(string? typeDateRange, string? dateRangeFromFront = null, string? dateRangeToFront = null)
        {

            //Default Dates
            var dateRangeFrom = "";
            var dateRangeTo = "";
            DateTime dateRangeFromParse;

            //Case values
            switch (typeDateRange)
            {
                case "today":
                    dateRangeFrom = DateTime.Now.ToString("yyyy'-'MM'-'dd' '00':'00':'00");
                    dateRangeTo = DateTime.Now.ToString("yyyy'-'MM'-'dd' '23':'59':'59");
                    break;
                case "yesterday":
                    dateRangeFrom = DateTime.Today.AddDays(-1).ToString("yyyy'-'MM'-'dd' '00':'00':'00");
                    dateRangeTo = DateTime.Today.AddDays(-1).ToString("yyyy'-'MM'-'dd' '23':'59':'59");
                    break;
                case "tomorrow":
                    dateRangeFrom = DateTime.Today.AddDays(1).ToString("yyyy'-'MM'-'dd' '00':'00':'00");
                    dateRangeTo = DateTime.Today.AddDays(1).ToString("yyyy'-'MM'-'dd' '23':'59':'59");
                    break;
                case "lastSevenDays":
                    dateRangeFrom = DateTime.Today.AddDays(-6).ToString("yyyy'-'MM'-'dd' '00':'00':'00");
                    dateRangeTo = DateTime.Now.ToString("yyyy'-'MM'-'dd' '23':'59':'59");
                    break;
                case "currentWeek":
                    dateRangeFrom = DateTime.Today.AddDays(-1 * (int)(DateTime.Today.DayOfWeek)).ToString("yyyy'-'MM'-'dd' '00':'00':'00");
                    dateRangeFromParse = DateTime.Parse(dateRangeFrom);
                    dateRangeTo = dateRangeFromParse.AddDays(7).AddSeconds(-1).ToString("yyyy'-'MM'-'dd' '23':'59':'59");
                    break;
                case "lastWeek":
                    dateRangeFrom = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek - 6).ToString("yyyy'-'MM'-'dd' '00':'00':'00");
                    dateRangeFromParse = DateTime.Parse(dateRangeFrom);
                    dateRangeTo = dateRangeFromParse.AddDays(6).ToString("yyyy'-'MM'-'dd' '23':'59':'59");
                    break;
                case "nextWeek":
                    dateRangeFrom = DateTime.Today.AddDays(((int)DayOfWeek.Monday - (int)DateTime.Today.DayOfWeek + 7) % 7).ToString("yyyy'-'MM'-'dd' '00':'00':'00");
                    dateRangeFromParse = DateTime.Parse(dateRangeFrom);
                    dateRangeTo = dateRangeFromParse.AddDays(6).ToString("yyyy'-'MM'-'dd' '23':'59':'59");
                    break;
                case "lastThirtyDays":
                    dateRangeFrom = DateTime.Today.AddDays(-29).ToString("yyyy'-'MM'-'dd' '00':'00':'00");
                    dateRangeTo = DateTime.Now.ToString("yyyy'-'MM'-'dd' '23':'59':'59");
                    break;
                case "lastSixtyDays":
                    dateRangeFrom = DateTime.Today.AddDays(-59).ToString("yyyy'-'MM'-'dd' '00':'00':'00");
                    dateRangeTo = DateTime.Now.ToString("yyyy'-'MM'-'dd' '23':'59':'59");
                    break;
                case "currentMonth":
                    dateRangeFrom = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).ToString("yyyy'-'MM'-'dd' '00':'00':'00");
                    dateRangeFromParse = DateTime.Parse(dateRangeFrom);
                    dateRangeTo = dateRangeFromParse.AddMonths(1).AddDays(-1).ToString("yyyy'-'MM'-'dd' '23':'59':'59"); ;
                    break;
                case "lastMonth":
                    dateRangeFrom = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-1).ToString("yyyy'-'MM'-'dd' '00':'00':'00");
                    dateRangeFromParse = DateTime.Parse(dateRangeFrom);
                    dateRangeTo = dateRangeFromParse.AddMonths(1).AddDays(-1).ToString("yyyy'-'MM'-'dd' '23':'59':'59"); ;
                    break;
                case "nextMonth":
                    dateRangeFrom = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(1).ToString("yyyy'-'MM'-'dd' '00':'00':'00");
                    dateRangeFromParse = DateTime.Parse(dateRangeFrom);
                    dateRangeTo = dateRangeFromParse.AddMonths(1).AddDays(-1).ToString("yyyy'-'MM'-'dd' '23':'59':'59");
                    break;
                case "twoMonthsAgo":
                    dateRangeFrom = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-2).ToString("yyyy'-'MM'-'dd' '00':'00':'00");
                    dateRangeFromParse = DateTime.Parse(dateRangeFrom);
                    dateRangeTo = dateRangeFromParse.AddMonths(1).AddDays(-1).ToString("yyyy'-'MM'-'dd' '23':'59':'59");
                    break;
                case "twoYearsAgo":
                    dateRangeFrom = new DateTime(DateTime.Now.Year, 1, 1).AddYears(-2).ToString("yyyy'-'MM'-'dd' '00':'00':'00");
                    dateRangeTo = new DateTime(DateTime.Now.Year, 12, 31).AddYears(-2).ToString("yyyy'-'MM'-'dd' '23':'59':'59");
                    break;
                case "lastYear":
                    dateRangeFrom = new DateTime(DateTime.Now.Year, 1, 1).AddYears(-1).ToString("yyyy'-'MM'-'dd' '00':'00':'00");
                    dateRangeTo = new DateTime(DateTime.Now.Year, 12, 31).AddYears(-1).ToString("yyyy'-'MM'-'dd' '23':'59':'59");
                    break;
                case "lastTwoYears":
                    dateRangeFrom = new DateTime(DateTime.Now.Year, 1, 1).AddYears(-1).ToString("yyyy'-'MM'-'dd' '00':'00':'00");
                    dateRangeTo = new DateTime(DateTime.Now.Year, 12, 31).ToString("yyyy'-'MM'-'dd' '23':'59':'59");
                    break;
                case "currentYear":
                    dateRangeFrom = new DateTime(DateTime.Now.Year, 1, 1).ToString("yyyy'-'MM'-'dd' '00':'00':'00");
                    dateRangeTo = new DateTime(DateTime.Now.Year, 12, 31).ToString("yyyy'-'MM'-'dd' '23':'59':'59");
                    break;
                case "15daysAroundToday":
                    dateRangeFrom = DateTime.Today.AddDays(-15).ToString("yyyy'-'MM'-'dd' '00':'00':'00");
                    dateRangeTo = DateTime.Today.AddDays(15).ToString("yyyy'-'MM'-'dd' '23':'59':'59");
                    break;
                case "5daysAroundToday": 
                    dateRangeFrom = DateTime.Today.AddDays(-5).ToString("yyyy'-'MM'-'dd' '00':'00':'00");
                    dateRangeTo = DateTime.Today.AddDays(5).ToString("yyyy'-'MM'-'dd' '23':'59':'59");
                    break;
                case "currentMonthUntilToday":
                    dateRangeFrom = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).ToString("yyyy'-'MM'-'dd' '00':'00':'00");
                    dateRangeTo = DateTime.UtcNow.Date.ToString("yyyy'-'MM'-'dd' '23':'59':'59");
                    break;
                case "currentMonthUntilYesterday":
                    dateRangeFrom = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).ToString("yyyy'-'MM'-'dd' '00':'00':'00");
                    dateRangeTo = DateTime.UtcNow.Date.AddDays(-1).ToString("yyyy'-'MM'-'dd' '23':'59':'59");
                    break;
                case "next15Days":
                    dateRangeFrom = DateTime.UtcNow.ToString("yyyy'-'MM'-'dd' '00':'00':'00");
                    dateRangeTo = DateTime.UtcNow.AddDays(15).ToString("yyyy'-'MM'-'dd' '23':'59':'59");
                    break;
                case "next30Days":
                    dateRangeFrom = DateTime.UtcNow.ToString("yyyy'-'MM'-'dd' '00':'00':'00");
                    dateRangeTo = DateTime.UtcNow.AddDays(30).ToString("yyyy'-'MM'-'dd' '23':'59':'59");
                    break;
                case "next90Days":
                    dateRangeFrom = DateTime.UtcNow.ToString("yyyy'-'MM'-'dd' '00':'00':'00");
                    dateRangeTo = DateTime.UtcNow.AddDays(90).ToString("yyyy'-'MM'-'dd' '23':'59':'59");
                    break;
                case "lastWeekToCurrentWeek":
                    var startOfLastWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek - 7);
                    var endOfThisWeek = DateTime.Today.AddDays((6 - (int)DateTime.Today.DayOfWeek) % 7);
                    dateRangeFrom = startOfLastWeek.ToString("yyyy'-'MM'-'dd' '00':'00':'00");
                    dateRangeTo = endOfThisWeek.ToString("yyyy'-'MM'-'dd' '23':'59':'59");
                    break;
                case "CurrentWeekToNextWeek":
                    dateRangeFrom = DateTime.Today.AddDays(-1 * (int)(DateTime.Today.DayOfWeek)).ToString("yyyy'-'MM'-'dd' '00':'00':'00");
                    dateRangeFromParse = DateTime.Parse(dateRangeFrom);
                    dateRangeTo = dateRangeFromParse.AddDays(14).ToString("yyyy'-'MM'-'dd' '23':'59':'59");
                    break;
                case "customRange":
                    dateRangeFrom = dateRangeFromFront;
                    dateRangeTo = dateRangeToFront;
                    break;
            }

            return (dateRangeFrom, dateRangeTo);
        }



        public static bool ValidateOnDay(string onDay)
        {
            try
            {
                List<int> daysOfWeek = JsonConvert.DeserializeObject<List<int>>(onDay);
                foreach (int day in daysOfWeek)
                {
                    if (day < 1 || day > 7)
                    {
                        return false;
                    }
                }
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }


    }
}
