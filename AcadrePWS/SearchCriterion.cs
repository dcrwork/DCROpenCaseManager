namespace AcadreLib
{
    public class SearchCriterion
    {
        private string aCaseContent;
        private string aPrimaryContactsName;
        private string aChildCPR;
        public int AcadreOrgID { get; set; }
        public string CaseManagerInitials { get; set; }
        public string ChildCPR
        {
            get { return aChildCPR ?? "*"; }
            set { aChildCPR = value; }
        }
        public string CaseContent
        {
            get { return aCaseContent ?? ""; }
            set { aCaseContent = value; }
        }
        public string PrimaryContactsName
        {
            get { return aPrimaryContactsName ?? ""; }
            set { aPrimaryContactsName = value; }
        }
        public string KLE { get; set; }
        public bool? IsClosed { get; set; }
    }
}