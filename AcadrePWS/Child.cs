using System.Collections.Generic;

namespace AcadreLib
{
    public class Child
    {

        public int CaseID; // Acadre Case ID
        public string CaseNumberIdentifier;
        public string Note;
        public SimplePerson SimpleChild;
        public IEnumerable<string> CustodyOwnersNames;
        public string SchoolName;
        public IEnumerable<SimplePerson> Mom;
        public IEnumerable<SimplePerson> Dad;
        public SimplePerson Guardian;
        public IEnumerable<SimplePerson> Siblings;
        public Child()
        {
            SimpleChild = new SimplePerson();
            Mom = new List<SimplePerson>();
            Dad = new List<SimplePerson>();
            Guardian = new SimplePerson();
        }

        public string CaseManagerInitials;
        public string CaseManagerName;
    }
}
