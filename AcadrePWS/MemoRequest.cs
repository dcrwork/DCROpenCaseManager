using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcadreLib
{
    public class MemoRequest
    {
        public string fileName;
        public string accessCode;
        public string caseFileReference;
        public string titleText;
        public string creatorReference;
        public string memoTypeReference;
        public bool isLocked;
        public byte[] fileBytes;
        public DateTime eventDate;
    }
}
