using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenCaseManager.Models
{
    public class MajorRevision
    {
        public int GraphId { get; set; }

        public int MajorRevisionId { get; set; }

        public string MajorRevisionTitle { get; set; }

        public DateTime MajorRevisionDate { get; set; }

        public string Error { get; set; }
    }
}