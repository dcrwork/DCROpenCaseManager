using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenCaseManager.Models
{
    public class AddInstanceModel
    {
        public string Title { get; set; }
        public int GraphId { get; set; }
        public int Responsible { get; set; }
        public List<UserRole> UserRoles { get; set; }
        public int? ChildId { get; set; }
        public string CaseNumberIdentifier { get; set; }
        public string CaseId { get; set; }
        public string CaseLink { get; set; }
    }
}