using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace AdvanceTimeWindowsService
{
    public partial class DCRAdvanceTime : ServiceBase
    {
        private Timer _serviceTimer = null;
        private EventLog eventLog;

        public DCRAdvanceTime()
        {
            InitializeComponent();
            LogSettings();
        }

        protected override void OnStart(string[] args)
        {
            _serviceTimer = new Timer();
            _serviceTimer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            _serviceTimer.Interval = Config.WaitTime;
            _serviceTimer.Enabled = true;
            InvokeAdvanceTimeService(true);
        }

        protected override void OnStop()
        {
            Log("Service stopped at " + DateTime.Now, false);
            _serviceTimer.Enabled = false;
        }

        #region private methods

        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            InvokeAdvanceTimeService(false);
        }

        private void InvokeAdvanceTimeService(bool isOnStart)
        {
            try
            {
                var response = Common.ExecuteServiceUsingWindowsLogin(Config.OCMUrl, "api/services/advanceTime", RestSharp.Method.POST);
                if (!isOnStart)
                {
                    Log("AdvanceTime recalled at " + DateTime.Now, false);
                }
                else
                {
                    Log("AdvanceTime started at " + DateTime.Now, false);
                }
            }
            catch (Exception ex)
            {
                Log(ex.Message + " - AdvanceTime failed at " + DateTime.Now, true);
            }
        }

        private void LogSettings()
        {
            if (Config.LogInTxt)
            {
                string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            else
            {
                string eventSourceName = Config.EventViewerSource;
                string logName = Config.EventViewerLog;
                if (!EventLog.SourceExists(Config.EventViewerSource))
                {
                    EventLog.CreateEventSource(Config.EventViewerSource, Config.EventViewerLog);
                }
                eventLog = new EventLog
                {
                    Source = Config.EventViewerSource,
                    Log = Config.EventViewerLog
                };
            }
        }

        private void Log(string messsage, bool isError)
        {
            if (Config.LogInTxt)
            {
                WriteToFile(messsage);
            }
            else
            {
                var eventType = EventLogEntryType.Information;
                if (isError)
                    eventType = EventLogEntryType.Error;
                WriteToEventViewer(messsage, eventType);
            }
        }

        private void WriteToFile(string message)
        {
            var fileName = DateTime.Now.ToString("dd.MM.yyyy");
            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs" + "\\" + fileName + ".txt";
            if (!File.Exists(filepath))
            {
                // Create a file to write to.   
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(message);
                }
            }
        }

        private void WriteToEventViewer(string message, EventLogEntryType eventType)
        {
            eventLog.WriteEntry(message, eventType);
        }
        #endregion
    }
}
