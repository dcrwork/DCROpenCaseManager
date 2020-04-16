using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCaseManager.Commons
{
    public interface IMailRepository
    {
        IEnumerable<string> GetUnreadMails();

        IEnumerable<string> GetAllMails();
    }
}
