using System;
using System.Collections.Generic;
using System.Linq;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;

namespace Ical.Net.Extensions
{
    static class CalendarExtensions
    {
        // Currently limiting Max Date for a event
        static readonly DateTime IcsMaxDate = DateTime.Now.AddYears(20);

        public static HashSet<Occurrence> GetAllOccurrences(this CalendarEvent @this)
        {
            return @this.GetOccurrences(@this.DtStart.AsSystemLocal, IcsMaxDate);
        }

        public static string GetTimeZoneId(this IDateTime @this)
        {
            return @this.Parameters.FirstOrDefault(p => p.Name == "TZID")?.Value ?? string.Empty;
        }

        public static bool IsIndefinite(this CalendarEvent @this)
        {
            return @this.DtStart.HasTime && !@this.Properties.ContainsKey("DTEND") && !@this.Properties.ContainsKey("DURATION");
        }
    }
}
