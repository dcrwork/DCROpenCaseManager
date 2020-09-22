using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenCaseManager.Models
{
    public class Memo
    {
        public string AccessCode { get; set; }
        public string CaseFileReferenceNumber { get; set; }
        public string FileName { get; set; }
        public string MemoTitleText { get; set; }
        public string MemoTypeReference { get; set; }
        public bool IsLocked { get; set; }
        public string NoteText { get; set; }
        public DateTime Date { get; set; }
        public string Html { get; set; }
        public string EventId { get; set; }
        public string Type { get; set; }
        public string InstanceId { get; set; }
    }
}