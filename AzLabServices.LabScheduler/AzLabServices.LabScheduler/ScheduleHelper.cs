using Ical.Net;
using Ical.Net.Extensions;
using System;
using System.Collections.Generic;
using System.IO;

namespace AzLabServices.LabScheduler
{
    public class ScheduleHelper
    {
        public List<LabSchedule> GetLabScheduleFromICalendar(string icsFilePath)
        {
            if (string.IsNullOrWhiteSpace(icsFilePath))
            {
                throw new ArgumentException(icsFilePath);
            }

            var icsFileContent = File.ReadAllText(icsFilePath);
            var calendar = Calendar.Load(icsFileContent);

            var labSchedules = new List<LabSchedule>();

            foreach (var calendarEvent in calendar.Events)
            {
                var eventOccurrences = calendarEvent.GetAllOccurrences();
                foreach (var eventOccurrence in eventOccurrences)
                {
                    var startTime = eventOccurrence.Period.StartTime.AsSystemLocal;
                    var endTime = eventOccurrence.Period.EndTime.AsSystemLocal;

                    var timeZoneId = eventOccurrence.Period.StartTime.GetTimeZoneId();

                    labSchedules.Add(new LabSchedule
                    {
                        FromDate = startTime,
                        ToDate = endTime,
                        TimeZoneId = timeZoneId,
                        Summary = calendarEvent.Summary
                    });
                }
            }

            return labSchedules;
        }
    }
}
