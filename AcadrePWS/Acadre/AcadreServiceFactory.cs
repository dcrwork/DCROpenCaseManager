using System.Web.Services.Protocols;

namespace AcadrePWS.Acadre
{
    public static class AcadreServiceFactory
    {
        private static AcadreLib.AcadreServiceV7.CaseService7 caseService7;
        private static AcadreLib.AcadreServiceV7.ContactService7 contactService7;
        private static AcadreLib.AcadreServiceV7.MainDocumentService7 mainDocumentService7;
        private static AcadreLib.AcadreServiceV7.MemoService7 memoService7;
        private static AcadreLib.AcadreServiceV7.ConfigurationService7 configurationService7;
        private static System.Net.NetworkCredential networkCredential = new System.Net.NetworkCredential(Config.AcadreServiceUserName, Config.AcadreServiceUserPassword, Config.AcadreServiceUserDomain);

        /// <summary>
        /// Get Case Service
        /// </summary>
        /// <returns></returns>
        public static AcadreLib.AcadreServiceV7.CaseService7 GetCaseService7()
        {
            if (caseService7 == null)
            {
                caseService7 = new AcadreLib.AcadreServiceV7.CaseService7()
                {
                    Url = Config.AcadreService,
                    Credentials = networkCredential
                };
            }
            return caseService7;
        }

        /// <summary>
        /// Get contact service
        /// </summary>
        /// <returns></returns>
        public static AcadreLib.AcadreServiceV7.ContactService7 GetContactService7()
        {
            if (contactService7 == null)
            {
                contactService7 = new AcadreLib.AcadreServiceV7.ContactService7()
                {
                    Url = Config.AcadreService,
                    Credentials = networkCredential
                };
            }
            return contactService7;
        }

        /// <summary>
        /// Get Main Document
        /// </summary>
        /// <returns></returns>
        public static AcadreLib.AcadreServiceV7.MainDocumentService7 GetMainDocumentService7()
        {
            if (mainDocumentService7 == null)
            {
                mainDocumentService7 = new AcadreLib.AcadreServiceV7.MainDocumentService7()
                {
                    Url = Config.AcadreService,
                    Credentials = networkCredential
                };
            }
            return mainDocumentService7;
        }

        /// <summary>
        /// Get Memo Service
        /// </summary>
        /// <returns></returns>
        public static AcadreLib.AcadreServiceV7.MemoService7 GetMemoService7()
        {
            if (memoService7 == null)
            {
                memoService7 = new AcadreLib.AcadreServiceV7.MemoService7()
                {
                    Url = Config.AcadreService,
                    Credentials = networkCredential
                };
            }
            return memoService7;
        }

        /// <summary>
        /// Get Configuration Service
        /// </summary>
        /// <returns></returns>
        public static AcadreLib.AcadreServiceV7.ConfigurationService7 GetConfigurationService7()
        {
            if (configurationService7 == null)
            {
                configurationService7 = new AcadreLib.AcadreServiceV7.ConfigurationService7()
                {
                    Url = Config.AcadreService,
                    Credentials = networkCredential
                };
            }
            return configurationService7;
        }
    }
}
