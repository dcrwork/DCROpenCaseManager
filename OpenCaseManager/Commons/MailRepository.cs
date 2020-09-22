using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MimeKit;
using OpenCaseManager.Managers;
using OpenCaseManager.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace OpenCaseManager.Commons
{
    public class MailRepository : IMailRepository
    {
        private readonly string mailServer, login, password;
        private readonly int port;
        private readonly bool ssl;
        private IManager _manager;
        private IDataModelManager _dataModelManager;
        private IDCRService _dcrService;

        public MailRepository(IManager manager, IDataModelManager dataModelManager, IDCRService dCRService)
        {
            mailServer = Configurations.Config.MailServer;
            port = Configurations.Config.MailServerPort;
            ssl = true;
            login = Configurations.Config.MailUsername;
            password = Configurations.Config.MailPassword;
            _manager = manager;
            _dataModelManager = dataModelManager;
            _dcrService = dCRService;
        }

        public IEnumerable<string> GetUnreadMails()
        {
            try
            {
                var messages = new List<string>();

                using (var client = new ImapClient())
                {
                    client.Connect(mailServer, port, ssl);

                    // Note: since we don't have an OAuth2 token, disable
                    // the XOAUTH2 authentication mechanism.
                    client.AuthenticationMechanisms.Remove("XOAUTH2");

                    client.Authenticate(login, password);

                    // The Inbox folder is always available on all IMAP servers...
                    var inbox = client.Inbox;
                    inbox.Open(FolderAccess.ReadOnly);
                    var results = inbox.Search(SearchOptions.All, SearchQuery.Not(SearchQuery.Seen));
                    foreach (var uniqueId in results.UniqueIds)
                    {
                        var message = inbox.GetMessage(uniqueId);
                        ReadMessage(message);
                        messages.Add(message.HtmlBody);

                        //Mark message as read
                        //inbox.AddFlags(uniqueId, MessageFlags.Seen, true);
                    }

                    client.Disconnect(true);
                }

                return messages;
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "GetUnreadMails - Failed.");
                Common.LogError(ex);
                throw ex;
            }
        }

        public IEnumerable<string> GetAllMails()
        {
            try
            {
                var messages = new List<string>();

                using (var client = new ImapClient())
                {
                    client.Connect(mailServer, port, ssl);

                    // Note: since we don't have an OAuth2 token, disable
                    // the XOAUTH2 authentication mechanism.
                    client.AuthenticationMechanisms.Remove("XOAUTH2");

                    client.Authenticate(login, password);

                    // The Inbox folder is always available on all IMAP servers...
                    var inbox = client.Inbox;
                    inbox.Open(FolderAccess.ReadOnly);
                    var results = inbox.Search(SearchOptions.All, SearchQuery.NotSeen);
                    foreach (var uniqueId in results.UniqueIds)
                    {
                        var message = inbox.GetMessage(uniqueId);

                        messages.Add(message.HtmlBody);

                        //Mark message as read
                        //inbox.AddFlags(uniqueId, MessageFlags.Seen, true);
                    }

                    client.Disconnect(true);
                }

                return messages;
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "GetAllMails - Failed.");
                Common.LogError(ex);
                throw ex;
            }
        }

        private void ReadMessage(MimeMessage message)
        {
            try
            {
                Guid processGuid = new Guid(message.Subject.Split(' ')[0]);

                _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.Process.ToString());
                _dataModelManager.AddResultSet(new List<string>() { "*" });
                _dataModelManager.AddFilter(DBEntityNames.Process.Guid.ToString(), Enums.ParameterType._string, processGuid.ToString(), Enums.CompareOperator.equal, Enums.LogicalOperator.none);
                var dataTable = _manager.SelectData(_dataModelManager.DataModel);

                var createInstanceModel = new AddInstanceModel()
                {
                    GraphId = int.Parse(dataTable.Rows[0]["GraphId"].ToString()),
                    Title = dataTable.Rows[0]["Title"].ToString() + processGuid.ToString(),
                    UserRoles = new List<UserRole>()
                };

                var createInstance = Common.AddInstance(createInstanceModel, _manager, _dataModelManager);

                var responsibleId = Common.GetResponsibleId();
                InitializeGraphModel model = Common.InitializeGraph(Configurations.Config.UseProcessEngine, createInstanceModel.GraphId.ToString(), createInstance, _dcrService, _manager, _dataModelManager);

                if (!string.IsNullOrEmpty(model.SimulationId))
                {
                    Common.LogInfo(_manager, _dataModelManager, "Instance Id : " + createInstance + " initialized , simId : " + model.SimulationId + " - OCMSpawnChildProcess");

                    Common.UpdateInstance(createInstance, model.SimulationId, model.InstanceXML, _manager, _dataModelManager);
                    Common.SyncEvents(createInstance, model.EventsXML, responsibleId, _manager, _dataModelManager);
                    Common.UpdateEventTypeData(createInstance, _manager, _dataModelManager);
                    //AutomaticEvents(childInstanceId, createInstanceModel.GraphId.ToString(), model.SimulationId, responsibleId);
                    Common.LogInfo(_manager, _dataModelManager, "SyncEvents called for new instance Instanc Id : " + createInstance + " - OCMSpawnChildProcess");
                };

                foreach (var attachment in message.Attachments)
                {
                    var fileName = attachment.ContentDisposition?.FileName ?? attachment.ContentType.Name;

                    using (var stream = File.Create(AppDomain.CurrentDomain.BaseDirectory + "\\tmp\\" + fileName))
                    {
                        if (attachment is MessagePart)
                        {
                            var rfc822 = (MessagePart)attachment;

                            rfc822.Message.WriteTo(stream);
                        }
                        else
                        {
                            var part = (MimePart)attachment;

                            part.Content.DecodeTo(stream);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "ReadMessage - Failed. - " + Common.ToJson(message));
                Common.LogError(ex);
                throw ex;
            }
        }
    }
}