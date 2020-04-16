using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenCaseManager.Models
{
    public class GlobalEvents
    {
        public string InstanceId { get; set; }
        public string Title { get; set; }
        public bool IsIncluded { get; set; }
        public bool IsEnabled { get; set; }
        public string Message { get; set; }
        public int InternalCaseId { get; set; }
        public string CaseNoForeign { get; set; }
        public string CaseLink { get; set; }
        public string Description { get; set; }
        public string EventTitle { get; set; }
        public string EventId { get; set; }
        public int GraphId { get; set; }
        public int SimulationId { get; set; }
        public int TrueEventId { get; set; }
    }
}