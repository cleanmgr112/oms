using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model
{
    public class ScheduleTaskModel
    {
        public string Name { get; set; }

        public int Minutes { get; set; }

        public string Type { get; set; }

        public bool Enabled { get; set; }

        public string Functions { get; set; }

        public bool IsStopOnError { get; set; }

        public Nullable<DateTime> LastStartTime { get; set; }

        public Nullable<DateTime> LastEndTime { get; set; }

        public Nullable<DateTime> LastSuccessTime { get; set; }
    }
}
