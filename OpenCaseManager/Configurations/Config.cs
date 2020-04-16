using System;
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
        public static bool UseProcessEngine
        {
            get
            {
                return Boolean.Parse(ConfigurationManager.AppSettings["UseProcessEngine"].ToString());
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
        public static string InstanceFileLocation
        {
            get
            {
                return ConfigurationManager.AppSettings["InstanceFileLocation"].ToString();
            }
        }
        public static string DCRPortalURL
        {
            get
            {
                return ConfigurationManager.AppSettings["DCRPortalURL"].ToString();
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
                return ConfigurationManager.AppSettings["DCRFormServerUrl"].ToString();
            }
        }
        public static string DCRConverterAppUrl
        {
            get
            {
                return ConfigurationManager.AppSettings["DCRConverterAppUrl"].ToString();
            }
        }
        public static bool AlwaysLogExecutions
        {
            get
            {
                return bool.Parse(ConfigurationManager.AppSettings["AlwaysLogExecutions"].ToString());
            }
        }
        public static bool LogToAcadre
        {
            get
            {
                return bool.Parse(ConfigurationManager.AppSettings["LogToAcadre"].ToString());
            }
        }
        public static bool LogToSbSys
        {
            get
            {
                return bool.Parse(ConfigurationManager.AppSettings["LogToSbSys"].ToString());
            }
        }
        public static string AcadreService
        {
            get
            {
                return ConfigurationManager.AppSettings["AcadreService"].ToString();
            }
        }
        public static string AcadreBaseurlPWI
        {
            get
            {
                return ConfigurationManager.AppSettings["AcadreBaseurlPWI"].ToString();
            }
        }
        public static string AcadreFrontEndBaseURL
        {
            get
            {
                return ConfigurationManager.AppSettings["AcadreFrontEndBaseURL"].ToString();
            }
        }
        public static string SmtpServer
        {
            get
            {
                return ConfigurationManager.AppSettings["SmtpServer"].ToString();
            }
        }
        public static string MailServer
        {
            get
            {
                return ConfigurationManager.AppSettings["MailServer"].ToString();
            }
        }
        public static string MailUsername
        {
            get
            {
                return ConfigurationManager.AppSettings["MailUsername"].ToString();
            }
        }
        public static string MailPassword
        {
            get
            {
                return ConfigurationManager.AppSettings["MailPassword"].ToString();
            }
        }
        public static int SmtpPort
        {
            get
            {
                return int.Parse(ConfigurationManager.AppSettings["SmtpPort"].ToString());
            }
        }
        public static int MailServerPort
        {
            get
            {
                return int.Parse(ConfigurationManager.AppSettings["MailServerPort"].ToString());
            }
        }
        public static string AcadreServiceUserName
        {
            get
            {
                return ConfigurationManager.AppSettings["AcadreServiceUserName"].ToString();
            }
        }
        public static string AcadreServiceUserPassword
        {
            get
            {
                return ConfigurationManager.AppSettings["AcadreServiceUserPassword"].ToString();
            }
        }
        public static string AcadreServiceUserDomain
        {
            get
            {
                return ConfigurationManager.AppSettings["AcadreServiceUserDomain"].ToString();
            }
        }
        public static string CPRBrokerEndpointURL
        {
            get
            {
                return ConfigurationManager.AppSettings["CPRBrokerEndpointURL"].ToString();
            }
        }
        public static string CPRBrokerUserToken
        {
            get
            {
                return ConfigurationManager.AppSettings["CPRBrokerUserToken"].ToString();
            }
        }
        public static string CPRBrokerApplicationToken
        {
            get
            {
                return ConfigurationManager.AppSettings["CPRBrokerApplicationToken"].ToString();
            }
        }
        public static string ProcessGovernanceGraphId
        {
            get
            {
                return ConfigurationManager.AppSettings["ProcessGovernanceGraphId"].ToString();
            }
        }
    }
}