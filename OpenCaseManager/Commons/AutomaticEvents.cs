using OpenCaseManager.Managers;
using OpenCaseManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace OpenCaseManager.Commons
{
    public class AutomaticEvents : IAutomaticEvents
    {
        private readonly IManager _manager;
        private readonly IDataModelManager _dataModelManager;
        private readonly IDCRService _dcrService;

        public AutomaticEvents(IManager manager, IDataModelManager dataModelManager, IDCRService dcrService)
        {
            _manager = manager;
            _dataModelManager = dataModelManager;
            _dcrService = dcrService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="to"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        public void SendEmail(string to, string subject, string body)
        {
            try
            {
                var mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient(Configurations.Config.SmtpServer);

                mail.From = new MailAddress(Configurations.Config.MailUsername);
                mail.To.Add(to);
                mail.Subject = subject;
                mail.Body = body.Replace("\\\\n", "<br/>");
                mail.IsBodyHtml = true;

                SmtpServer.Port = Configurations.Config.SmtpPort;
                SmtpServer.Credentials = new System.Net.NetworkCredential(Configurations.Config.MailUsername, Configurations.Config.MailPassword);
                SmtpServer.EnableSsl = true;

                SmtpServer.Send(mail);
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "SendEmail - Failed. - to : " + to + ",subject : " + subject + ",body : " + body
                    + ",Server : " + Configurations.Config.SmtpServer + ",Port : " + Configurations.Config.SmtpPort + ",MailUsername : " + Configurations.Config.MailUsername
                    + ",MailPassword : " + Configurations.Config.MailPassword);
                Common.LogError(ex);
            }
        }

        /// <summary>
        /// Release Process
        /// </summary>
        /// <param name="instanceId"></param>
        public void ReleaseProcess(string instanceId)
        {
            try
            {
                // get values from process history
                _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.ProcessHistory.ToString());
                _dataModelManager.AddResultSet(new List<string>() { DBEntityNames.ProcessHistory.DCRXML.ToString() });
                _dataModelManager.AddFilter(DBEntityNames.ProcessHistory.InstanceId.ToString(), Enums.ParameterType._int, instanceId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);
                var data = _manager.SelectData(_dataModelManager.DataModel);

                // get phases
                var phases = _dcrService.GetPhases(data.Rows[0][DBEntityNames.ProcessHistory.DCRXML.ToString()].ToString());

                _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SP, DBEntityNames.StoredProcedures.ReleaseProcessInstance.ToString());
                _dataModelManager.AddParameter(DBEntityNames.ReleaseProcessInstance.InstanceId.ToString(), Enums.ParameterType._int, instanceId);
                _dataModelManager.AddParameter(DBEntityNames.ReleaseProcessInstance.ProcessPhaseXML.ToString(), Enums.ParameterType._xml, phases);
                _manager.ExecuteStoredProcedure(_dataModelManager.DataModel);
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "ReleaseProcess - Failed. - instance : " + instanceId);
                Common.LogError(ex);
            }
        }
    }
}