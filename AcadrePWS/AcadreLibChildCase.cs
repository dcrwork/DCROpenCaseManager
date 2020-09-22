using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcadreLib
{
    public interface AcadreLibChildCase
    {
        IEnumerable<ChildCase> SearchChildren(SearchCriterion searchCriterion);
        Child GetChildInfo(string CPR);
        Child GetChildInfo(int CaseID);
        int CreateChildJournal(string CPR, int AcadreOrgID);
        IEnumerable<ChildCase> GetChildCases(int CaseID);
        IEnumerable<JournalDocument> GetChildJournalDocuments(int CaseID);
    }
}
