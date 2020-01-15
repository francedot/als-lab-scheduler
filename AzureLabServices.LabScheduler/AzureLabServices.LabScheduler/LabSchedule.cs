using System;
using System.Collections.Generic;
using System.Text;

namespace AzureLabServices.LabScheduler
{
    public class LabSchedule
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string TimeZoneId { get; set; }
        public string Summary { get; set; }
    }
}
