using System.Configuration;

namespace OpenCaseManager.Configurations
{
    public static class Config
    {
        public static string ConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["Default"].ToString();
            }
        }
        public static string DCRActiveRepository
        {
            get
            {
                return ConfigurationManager.AppSettings["DCRActiveRepository"].ToString();
            }
        }
        public static string DCRActiveRepositoryUser
        {
            get
            {
                return ConfigurationManager.AppSettings["DCRActiveRepositoryUser"].ToString();
            }
        }
        public static string DCRActiveRepositoryUserPassword
        {
            get
            {
                return ConfigurationManager.AppSettings["DCRActiveRepositoryUserPassword"].ToString();
            }
        }
        public static int AutomaticEventsLimit
        {
            get
            {
                return int.Parse(ConfigurationManager.AppSettings["AutomaticEventsLimit"].ToString());
            }
        }
        public static string MUSGraphId
        {
            get
            {
                return ConfigurationManager.AppSettings["MUSGraphId"].ToString();
            }
        }
        public static string EmployeeView
        {
            get
            {
                return ConfigurationManager.AppSettings["EmployeeObject"].ToString();
            }
        }
        public static string PersonalFileLocation
        {
            get
            {
                return ConfigurationManager.AppSettings["PersonalFileLocation"].ToString();
            }
        }
        public static string ChildFileLocation
        {
            get
            {
                return ConfigurationManager.AppSettings["ChildFileLocation"].ToString();
            }
        }
        public static string InstanceFileLocation
        {
            get
            {
                return ConfigurationManager.AppSettings["InstanceFileLocation"].ToString();
            }
        }

        public static string JournalNoteFileLocation
        {
            get
            {
                return ConfigurationManager.AppSettings["JournalNoteFileLocation"].ToString();
            }
        }
        public static string FormInstructionHtmlLocation
        {
            get
            {
                return ConfigurationManager.AppSettings["FormInstructionHtmlLocation"].ToString();
            }
        }
        public static string MUSInstructionHtmlLocation
        {
            get
            {
                return ConfigurationManager.AppSettings["MUSInstructionHtmlLocation"].ToString();
            }
        }
        public static string HideDocumentWebpart
        {
            get
            {
                return ConfigurationManager.AppSettings["HideDocumentWebpart"].ToString();
            }
        }
        public static string MUSLeaderRole
        {
            get
            {
                return ConfigurationManager.AppSettings["MUSLeaderRole"].ToString();
            }
        }
        public static string MUSEmployeeRole
        {
            get
            {
                return ConfigurationManager.AppSettings["MUSEmployeeRole"].ToString();
            }
        }
        public static string NodeWordDocumentServer
        {
            get
            {
                return ConfigurationManager.AppSettings["NodeWordDocumentServer"].ToString();
            }
        }
        public static string DcrFormServerUrl
        {
            get
            {
                return ConfigurationManager.AppSettings["DcrFormServerUrl"].ToString();
            }
        }
        public static string ClientId
        {
            get
            {
                return ConfigurationManager.AppSettings["ClientId"].ToString();
            }
        }
        public static string Authority
        {
            get
            {
                return ConfigurationManager.AppSettings["Authority"].ToString();
            }
        }
        public static string RedirectUri
        {
            get
            {
                return ConfigurationManager.AppSettings["RedirectUri"].ToString();
            }
        }
        public static string Tenant
        {
            get
            {
                return ConfigurationManager.AppSettings["Tenant"].ToString();
            }
        }
    }
}