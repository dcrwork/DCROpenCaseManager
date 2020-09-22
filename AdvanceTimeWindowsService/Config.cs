using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvanceTimeWindowsService
{
    public static class Config
    {
        public static double WaitTime
        {
            get
            {
                return double.Parse(ConfigurationManager.AppSettings["WaitTime"].ToString());
            }
        }

        public static string OCMUrl
        {
            get
            {
                return ConfigurationManager.AppSettings["OCMUrl"].ToString();
            }
        }

        public static bool LogInTxt
        {
            get
            {
                return bool.Parse(ConfigurationManager.AppSettings["LogInTxt"].ToString());
            }
        }

        public static string EventViewerSource
        {
            get
            {
                return ConfigurationManager.AppSettings["EventViewerSource"].ToString();
            }
        }

        public static string EventViewerLog
        {
            get
            {
                return ConfigurationManager.AppSettings["EventViewerLog"].ToString();
            }
        }
    }
}