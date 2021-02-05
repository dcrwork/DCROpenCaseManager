using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenCaseManager.Models
{
    public class AddTestCase
    {
        public string id { get; set; }
        public string title { get; set; }
        public string desc { get; set; }
        public string validTo { get; set; }
        public string validFrom { get; set; }
        public string graphId { get; set; }
        public string roles { get; set; }
        public string delay { get; set; }
    }

    public class AddTestCaseInstance
    {
        public string id { get; set; }
        public string testCaseId { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string guid { get; set; }
    }
}