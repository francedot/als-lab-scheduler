using NUnit.Framework;
using System.Linq;

namespace AzureLabServices.LabScheduler.Tests
{
    public class ScheduleHelperTests
    {
        private ScheduleHelper _sh;
        private string _dateFormat;
        private string _weStdTimeZoneId; // default timezone id used for tests

        [SetUp]
        public void Setup()
        {
            _sh = new ScheduleHelper();
            _dateFormat = "MM/dd/yyyy h:mm tt";
            _weStdTimeZoneId = "W. Europe Standard Time";
        }

        [Test]
        public void Test30MinOnceEvent()
        {
            var occurrences = _sh.GetLabScheduleFromICalendar("ICS/1_11_2029-30min-once.ics");

            Assert.True(occurrences.Count == 1);

            var expectedSummary = "A 30 min non-recurring event";
            var expectedFromDate = "01/11/2029 7:00 AM";
            var expectedToDate = "01/11/2029 7:30 AM";

            var occurrence = occurrences.Single();

            Assert.True(occurrence.Summary == expectedSummary);
            Assert.True(occurrence.TimeZoneId == _weStdTimeZoneId);
            Assert.True(occurrence.FromDate.ToUniversalTime().ToString(_dateFormat) == expectedFromDate);
            Assert.True(occurrence.ToDate.ToUniversalTime().ToString(_dateFormat) == expectedToDate);
        }

        [Test]
        public void TestAllDayOnceEvent()
        {
            var occurrences = _sh.GetLabScheduleFromICalendar("ICS/1_11_2029-allday-once.ics");

            Assert.True(occurrences.Count == 1);

            var expectedSummary = "An all day non-recurring event";
            var expectedFromDate = "01/10/2029 11:00 PM";
            var expectedToDate = "01/11/2029 11:00 PM";

            var occurrence = occurrences.Single();

            Assert.True(occurrence.Summary == expectedSummary);
            // No timezone expected for an all day event
            Assert.True(occurrence.TimeZoneId == string.Empty);
            Assert.True(occurrence.FromDate.ToUniversalTime().ToString(_dateFormat) == expectedFromDate);
            Assert.True(occurrence.ToDate.ToUniversalTime().ToString(_dateFormat) == expectedToDate);
        }

        [Test]
        public void Test2HourRecurrentEvent()
        {
            var occurrences = _sh.GetLabScheduleFromICalendar("ICS/1_11_2029-2hour-recurrent.ics");

            Assert.True(occurrences.Count == 81);

            var expectedSummary = "A 30 min recurring event from 1/11/2029 to 7/18/2029";

            var firstOccurrence = occurrences.First();

            var expectedFirstFromDate = "01/11/2029 7:00 AM";
            var expectedFirstToDate = "01/11/2029 9:00 AM";

            Assert.True(firstOccurrence.Summary == expectedSummary);
            // No timezone expected for an all day event
            Assert.True(firstOccurrence.TimeZoneId == _weStdTimeZoneId);
            Assert.True(firstOccurrence.FromDate.ToUniversalTime().ToString(_dateFormat) == expectedFirstFromDate);
            Assert.True(firstOccurrence.ToDate.ToUniversalTime().ToString(_dateFormat) == expectedFirstToDate);

            var lastOccurrence = occurrences.Last();

            var expectedLastFromDate = "07/17/2029 6:00 AM";
            var expectedLastToDate = "07/17/2029 8:00 AM";

            Assert.True(lastOccurrence.Summary == expectedSummary);
            Assert.True(lastOccurrence.TimeZoneId == _weStdTimeZoneId);
            Assert.True(lastOccurrence.FromDate.ToUniversalTime().ToString(_dateFormat) == expectedLastFromDate);
            Assert.True(lastOccurrence.ToDate.ToUniversalTime().ToString(_dateFormat) == expectedLastToDate);
        }

        [Test]
        public void Test2HourRecurrentNoEndEvent()
        {
            var occurrences = _sh.GetLabScheduleFromICalendar("ICS/1_11_2029-2hour-recurrent-noend.ics");

            Assert.True(occurrences.Count == 1724);

            var expectedSummary = "A 2 hour recurring event with no end date from 1/11/2029";

            var firstOccurrence = occurrences.First();

            var expectedFirstFromDate = "01/11/2029 7:00 AM";
            var expectedFirstToDate = "01/11/2029 9:00 AM";

            Assert.True(firstOccurrence.Summary == expectedSummary);
            // No timezone expected for an all day event
            Assert.True(firstOccurrence.TimeZoneId == _weStdTimeZoneId);
            Assert.True(firstOccurrence.FromDate.ToUniversalTime().ToString(_dateFormat) == expectedFirstFromDate);
            Assert.True(firstOccurrence.ToDate.ToUniversalTime().ToString(_dateFormat) == expectedFirstToDate);

            var lastOccurrence = occurrences.Last();

            var expectedLastFromDate = "01/13/2040 7:00 AM";
            var expectedLastToDate = "01/13/2040 9:00 AM";

            Assert.True(lastOccurrence.Summary == expectedSummary);
            Assert.True(lastOccurrence.TimeZoneId == _weStdTimeZoneId);
            Assert.True(lastOccurrence.FromDate.ToUniversalTime().ToString(_dateFormat) == expectedLastFromDate);
            Assert.True(lastOccurrence.ToDate.ToUniversalTime().ToString(_dateFormat) == expectedLastToDate);
        }

        [Test]
        public void TestAllDayRecurrentEvent()
        {
            var occurrences = _sh.GetLabScheduleFromICalendar("ICS/1_11_2029-allday-recurrent.ics");

            Assert.True(occurrences.Count == 81);

            var expectedSummary = "A 2-day recurring event from 1/11/2029 to 7/18/2029";

            var firstOccurrence = occurrences.First();

            var expectedFirstFromDate = "01/10/2029 11:00 PM";
            var expectedFirstToDate = "01/12/2029 11:00 PM";

            Assert.True(firstOccurrence.Summary == expectedSummary);
            // No timezone expected for an all day event
            Assert.True(firstOccurrence.TimeZoneId == string.Empty);
            Assert.True(firstOccurrence.FromDate.ToUniversalTime().ToString(_dateFormat) == expectedFirstFromDate);
            Assert.True(firstOccurrence.ToDate.ToUniversalTime().ToString(_dateFormat) == expectedFirstToDate);

            var lastOccurrence = occurrences.Last();

            var expectedLastFromDate = "07/16/2029 10:00 PM";
            var expectedLastToDate = "07/18/2029 10:00 PM";

            Assert.True(lastOccurrence.Summary == expectedSummary);
            Assert.True(lastOccurrence.TimeZoneId == string.Empty);
            Assert.True(lastOccurrence.FromDate.ToUniversalTime().ToString(_dateFormat) == expectedLastFromDate);
            Assert.True(lastOccurrence.ToDate.ToUniversalTime().ToString(_dateFormat) == expectedLastToDate);
        }
    }
}