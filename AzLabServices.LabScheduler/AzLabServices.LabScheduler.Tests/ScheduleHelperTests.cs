using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace AzLabServices.LabScheduler.Tests
{
    [TestClass]
    public class ScheduleHelperTests
    {
        private ScheduleHelper _sh;
        private string _dateFormat;
        private string _weStdTimeZoneId; // default timezone id used for tests

        public ScheduleHelperTests()
        {
            _sh = new ScheduleHelper();
            _dateFormat = "MM/dd/yyyy h:mm tt";
            _weStdTimeZoneId = "W. Europe Standard Time";
        }

        [TestMethod]
        public void Test30MinOnceEvent()
        {
            var occurrences = _sh.GetLabScheduleFromICalendar("ICS/1_11_2029-30min-once.ics");

            Assert.IsTrue(occurrences.Count == 1);

            var expectedSummary = "A 30 min non-recurring event";
            var expectedFromDate = "01/11/2029 7:00 AM";
            var expectedToDate = "01/11/2029 7:30 AM";

            var occurrence = occurrences.Single();

            Assert.IsTrue(occurrence.Summary == expectedSummary);
            Assert.IsTrue(occurrence.TimeZoneId == _weStdTimeZoneId);
            Assert.IsTrue(occurrence.FromDate.ToUniversalTime().ToString(_dateFormat) == expectedFromDate);
            Assert.IsTrue(occurrence.ToDate.ToUniversalTime().ToString(_dateFormat) == expectedToDate);
        }

        [TestMethod]
        public void TestAllDayOnceEvent()
        {
            var occurrences = _sh.GetLabScheduleFromICalendar("ICS/1_11_2029-allday-once.ics");

            Assert.IsTrue(occurrences.Count == 1);

            var expectedSummary = "An all day non-recurring event";
            var expectedFromDate = "01/10/2029 11:00 PM";
            var expectedToDate = "01/11/2029 11:00 PM";

            var occurrence = occurrences.Single();

            Assert.IsTrue(occurrence.Summary == expectedSummary);
            // No timezone expected for an all day event
            Assert.IsTrue(occurrence.TimeZoneId == string.Empty);
            Assert.IsTrue(occurrence.FromDate.ToUniversalTime().ToString(_dateFormat) == expectedFromDate);
            Assert.IsTrue(occurrence.ToDate.ToUniversalTime().ToString(_dateFormat) == expectedToDate);
        }

        [TestMethod]
        public void Test2HourRecurrentEvent()
        {
            var occurrences = _sh.GetLabScheduleFromICalendar("ICS/1_11_2029-2hour-recurrent.ics");

            Assert.IsTrue(occurrences.Count == 81);

            var expectedSummary = "A 30 min recurring event from 1/11/2029 to 7/18/2029";

            var firstOccurrence = occurrences.First();

            var expectedFirstFromDate = "01/11/2029 7:00 AM";
            var expectedFirstToDate = "01/11/2029 9:00 AM";

            Assert.IsTrue(firstOccurrence.Summary == expectedSummary);
            // No timezone expected for an all day event
            Assert.IsTrue(firstOccurrence.TimeZoneId == _weStdTimeZoneId);
            Assert.IsTrue(firstOccurrence.FromDate.ToUniversalTime().ToString(_dateFormat) == expectedFirstFromDate);
            Assert.IsTrue(firstOccurrence.ToDate.ToUniversalTime().ToString(_dateFormat) == expectedFirstToDate);

            var lastOccurrence = occurrences.Last();

            var expectedLastFromDate = "07/17/2029 6:00 AM";
            var expectedLastToDate = "07/17/2029 8:00 AM";

            Assert.IsTrue(lastOccurrence.Summary == expectedSummary);
            Assert.IsTrue(lastOccurrence.TimeZoneId == _weStdTimeZoneId);
            Assert.IsTrue(lastOccurrence.FromDate.ToUniversalTime().ToString(_dateFormat) == expectedLastFromDate);
            Assert.IsTrue(lastOccurrence.ToDate.ToUniversalTime().ToString(_dateFormat) == expectedLastToDate);
        }

        [TestMethod]
        public void Test2HourRecurrentNoEndEvent()
        {
            var occurrences = _sh.GetLabScheduleFromICalendar("ICS/1_11_2029-2hour-recurrent-noend.ics");

            Assert.IsTrue(occurrences.Count == 1724);

            var expectedSummary = "A 2 hour recurring event with no end date from 1/11/2029";

            var firstOccurrence = occurrences.First();

            var expectedFirstFromDate = "01/11/2029 7:00 AM";
            var expectedFirstToDate = "01/11/2029 9:00 AM";

            Assert.IsTrue(firstOccurrence.Summary == expectedSummary);
            // No timezone expected for an all day event
            Assert.IsTrue(firstOccurrence.TimeZoneId == _weStdTimeZoneId);
            Assert.IsTrue(firstOccurrence.FromDate.ToUniversalTime().ToString(_dateFormat) == expectedFirstFromDate);
            Assert.IsTrue(firstOccurrence.ToDate.ToUniversalTime().ToString(_dateFormat) == expectedFirstToDate);

            var lastOccurrence = occurrences.Last();

            var expectedLastFromDate = "01/13/2040 7:00 AM";
            var expectedLastToDate = "01/13/2040 9:00 AM";

            Assert.IsTrue(lastOccurrence.Summary == expectedSummary);
            Assert.IsTrue(lastOccurrence.TimeZoneId == _weStdTimeZoneId);
            Assert.IsTrue(lastOccurrence.FromDate.ToUniversalTime().ToString(_dateFormat) == expectedLastFromDate);
            Assert.IsTrue(lastOccurrence.ToDate.ToUniversalTime().ToString(_dateFormat) == expectedLastToDate);
        }

        [TestMethod]
        public void TestAllDayRecurrentEvent()
        {
            var occurrences = _sh.GetLabScheduleFromICalendar("ICS/1_11_2029-allday-recurrent.ics");

            Assert.IsTrue(occurrences.Count == 81);

            var expectedSummary = "A 2-day recurring event from 1/11/2029 to 7/18/2029";

            var firstOccurrence = occurrences.First();

            var expectedFirstFromDate = "01/10/2029 11:00 PM";
            var expectedFirstToDate = "01/12/2029 11:00 PM";

            Assert.IsTrue(firstOccurrence.Summary == expectedSummary);
            // No timezone expected for an all day event
            Assert.IsTrue(firstOccurrence.TimeZoneId == string.Empty);
            Assert.IsTrue(firstOccurrence.FromDate.ToUniversalTime().ToString(_dateFormat) == expectedFirstFromDate);
            Assert.IsTrue(firstOccurrence.ToDate.ToUniversalTime().ToString(_dateFormat) == expectedFirstToDate);

            var lastOccurrence = occurrences.Last();

            var expectedLastFromDate = "07/16/2029 10:00 PM";
            var expectedLastToDate = "07/18/2029 10:00 PM";

            Assert.IsTrue(lastOccurrence.Summary == expectedSummary);
            Assert.IsTrue(lastOccurrence.TimeZoneId == string.Empty);
            Assert.IsTrue(lastOccurrence.FromDate.ToUniversalTime().ToString(_dateFormat) == expectedLastFromDate);
            Assert.IsTrue(lastOccurrence.ToDate.ToUniversalTime().ToString(_dateFormat) == expectedLastToDate);
        }
    }
}