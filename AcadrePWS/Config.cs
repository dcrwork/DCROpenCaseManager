using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcadrePWS
{
    public static class Config
    {
        public static string AcadreService
        {
            get
            {
                return ConfigurationManager.AppSettings["AcadreService"].ToString();
            }
        }
        public static string AcadreFrontEndBaseURL
        {
            get
            {
                return ConfigurationManager.AppSettings["AcadreFrontEndBaseURL"].ToString();
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
                return ConfigurationManager.AppSettings["CPRBrokerEndpointURL"].ToString();
            }
        }
        public static string CPRBrokerApplicationToken
        {
            get
            {
                return ConfigurationManager.AppSettings["CPRBrokerApplicationToken"].ToString();
            }
        }
    }
}
