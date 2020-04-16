using System;

namespace AcadreLib
{
    public class JournalDocument
    {
        public string Type; // contains "Document" or "Memo"
        public int DocumentID; // Can be used to link to document or memo like http://esdhwebtest2.intern.syddjurs.dk:8083/Frontend/CM/MainDocument/Details?documentId=<DocumentID> or http://esdhwebtest2.intern.syddjurs.dk:8083/Frontend/CM/Memo/Details?memoId=<DocumentID>
        public string Title;
        public DateTime LastChangedDate;
        public int CaseID;
        public string CaseNumberIdentifier;
        public string DocumentMemoDescription;
    }
}