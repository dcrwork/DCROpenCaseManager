using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenCaseManager.Models
{
    public class Child
    {

        public int CaseID; // Acadre Case ID
        public string CaseNumberIdentifier;
        public SimplePerson SimpleChild;
        public List<string> CustodyOwnersNames;
        public string SchoolName;
        public List<SimplePerson> Mom;
        public List<SimplePerson> Dad;
        public SimplePerson Guardian;
        public List<SimplePerson> Siblings;        
        public string CustodyOwnersNamesList;

        public Child()
        {
            SimpleChild = new SimplePerson();
            Mom = new List<SimplePerson>();
            Dad = new List<SimplePerson>();
            Guardian = new SimplePerson();
        }
    }
}