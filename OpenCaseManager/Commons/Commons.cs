using OpenCaseManager.Managers;
using OpenCaseManager.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Xml;

namespace OpenCaseManager.Commons
{
    public class Commons : ICommons
    {
        private readonly IManager _manager;
        private readonly IDataModelManager _dataModelManager;
        private readonly IDCRService _dcrService;

        public Commons(IManager manager, IDataModelManager dataModelManager, IDCRService dcrService)
        {
            _manager = manager;
            _dataModelManager = dataModelManager;
            _dcrService = dcrService;
        }

        /// <summary>
        /// Get all proceses
        /// </summary>
        public List<MajorRevision> GetProcessMajorRevisions(string graphId = "")
        {
            try
            {
                var processIdsMajorVersion = new List<MajorRevision>();
                var processed = new List<string>();

                _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.ProcessHistory.ToString());
                if (!string.IsNullOrEmpty(graphId))
                    _dataModelManager.AddFilter(DBEntityNames.ProcessHistory.GraphId.ToString(), Enums.ParameterType._int, graphId, Enums.CompareOperator.equal, Enums.LogicalOperator.and);
                _dataModelManager.AddFilter("[State]", Enums.ParameterType._int, (-1).ToString(), Enums.CompareOperator.not_equal, Enums.LogicalOperator.none);
                _dataModelManager.AddResultSet(new List<string>() { "Id",
                DBEntityNames.Process.GraphId.ToString(), DBEntityNames.Process.MajorVersionId.ToString() });
                _dataModelManager.AddOrderBy(DBEntityNames.ProcessHistory.MajorVersionId.ToString(), true);
                var processes = _manager.SelectData(_dataModelManager.DataModel);

                MajorRevision majorRevision = null;
                foreach (DataRow row in processes.Rows)
                {
                    try
                    {
                        if (!processed.Contains(row["GraphId"].ToString()))
                        {
                            var majorRevisionGraph = GetMajorVersion(row[DBEntityNames.Process.GraphId.ToString()].ToString());
                            if (majorRevisionGraph.MajorRevisionId > int.Parse(row["MajorVersionId"].ToString() == "" ? 0.ToString() :
                                row["MajorVersionId"].ToString()))
                            {
                                majorRevision = new MajorRevision()
                                {
                                    GraphId = int.Parse(row[DBEntityNames.Process.GraphId.ToString()].ToString()),
                                    MajorRevisionId = majorRevisionGraph.MajorRevisionId,
                                    MajorRevisionTitle = majorRevisionGraph.MajorRevisionTitle,
                                    MajorRevisionDate = majorRevisionGraph.MajorRevisionDate,
                                    Error = string.Empty
                                };
                                processIdsMajorVersion.Add(majorRevision);
                            }
                        }
                        processed.Add(row[DBEntityNames.Process.GraphId.ToString()].ToString());
                    }
                    catch (Exception ex)
                    {
                        majorRevision = new MajorRevision()
                        {
                            GraphId = int.Parse(row[DBEntityNames.Process.GraphId.ToString()].ToString()),
                            MajorRevisionId = 0,
                            Error = ex.Message
                        };
                        processIdsMajorVersion.Add(majorRevision);
                        processed.Add(row[DBEntityNames.Process.GraphId.ToString()].ToString());
                        Common.LogError(ex);
                    }
                }
                return processIdsMajorVersion;
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "GetProcessMajorRevisions - Failed. - graphId : " + graphId);
                Common.LogError(ex);
                throw ex;
            }
        }

        /// <summary>
        /// Add major version id to process instance
        /// </summary>
        /// <param name="graphId"></param>
        public void AddMajorVersionIdToInstance(string graphId, string instanceId)
        {
            try
            {
                var majorVersion = GetMajorVersion(graphId);
                if (majorVersion.MajorRevisionId > 0)
                {
                    _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.UPDATE, DBEntityNames.Tables.Instance.ToString());
                    _dataModelManager.AddFilter(DBEntityNames.Instance.Id.ToString(), Enums.ParameterType._int, instanceId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);
                    _dataModelManager.AddParameter("MajorVersionId", Enums.ParameterType._int, majorVersion.MajorRevisionId.ToString());
                    _manager.UpdateData(_dataModelManager.DataModel);
                }
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "AddMajorVersionIdToInstance - Failed. - graphId : " + graphId + ",instanceId : " + instanceId);
                Common.LogError(ex);
                throw ex;
            }
        }

        /// <summary>
        /// Get major version Id of a graph
        /// </summary>
        /// <param name="graphId"></param>
        /// <returns></returns>
        public MajorRevision GetMajorVersion(string graphId)
        {
            try
            {
                var response = _dcrService.GetMajorRevisions(graphId);
                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(response);

                var majorRevision = new MajorRevision()
                {
                    GraphId = int.Parse(graphId),
                    MajorRevisionId = 0,
                };

                var childs = xmlDocument.ChildNodes;
                if (childs.Count > 0)
                {
                    var majorVersionIds = new List<int>();
                    var majorVersionDate = new Dictionary<int, DateTime>();
                    var majorVersionTitle = new Dictionary<int, string>();

                    foreach (XmlNode node in childs)
                    {
                        foreach (XmlNode nodes in node)
                        {
                            majorVersionIds.Add(int.Parse(nodes.Attributes["id"].Value));
                            majorVersionTitle.Add(int.Parse(nodes.Attributes["id"].Value), nodes.Attributes["title"].Value);
                            majorVersionDate.Add(int.Parse(nodes.Attributes["id"].Value), DateTime.Parse(nodes.Attributes["date"].Value));
                        }
                    }
                    if (majorVersionIds.Count > 0)
                    {
                        majorRevision.MajorRevisionId = majorVersionIds.Max();
                        majorRevision.MajorRevisionTitle = majorVersionTitle[majorRevision.MajorRevisionId];
                        majorRevision.MajorRevisionDate = majorVersionDate[majorRevision.MajorRevisionId];
                    }
                }
                return majorRevision;
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "GetMajorVersion - Failed. - graphId : " + graphId);
                Common.LogError(ex);
                throw ex;
            }
        }

        /// <summary>
        /// Get Roles for Event Responsible
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="eventId"></param>
        /// <param name="responsibleId"></param>
        /// <returns></returns>
        public string GetResponsibleRoles(string instanceId, string eventId, string responsibleId)
        {
            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.Event.ToString());
            _dataModelManager.AddResultSet(new List<string>() { DBEntityNames.Event.Roles.ToString() });
            _dataModelManager.AddFilter(DBEntityNames.Event.InstanceId.ToString(), Enums.ParameterType._int, instanceId, Enums.CompareOperator.equal, Enums.LogicalOperator.and);
            _dataModelManager.AddFilter(DBEntityNames.Event.Id.ToString(), Enums.ParameterType._int, eventId, Enums.CompareOperator.equal, Enums.LogicalOperator.and);
            _dataModelManager.AddFilter(DBEntityNames.Event.Roles.ToString(), Enums.ParameterType._string, null, Enums.CompareOperator._null, Enums.LogicalOperator.none);
            var eventRole = _manager.SelectData(_dataModelManager.DataModel);

            if (eventRole.Rows.Count > 0)
            {            // get instance xml
                var instanceXML = Common.GetInstanceXML(instanceId, _manager, _dataModelManager);
                dynamic dcrGraph = new DCRGraph(instanceXML);
                // get pending or enabled from active repository
                string eventsXml = _dcrService.GetPendingOrEnabled(dcrGraph);

                _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SP, DBEntityNames.StoredProcedures.GetEventResponsibleRoles.ToString());
                _dataModelManager.AddParameter(DBEntityNames.GetEventResponsibleRoles.InstanceId.ToString(), Enums.ParameterType._int, instanceId);
                _dataModelManager.AddParameter(DBEntityNames.GetEventResponsibleRoles.EventId.ToString(), Enums.ParameterType._int, eventId);
                _dataModelManager.AddParameter(DBEntityNames.GetEventResponsibleRoles.ResponsibleId.ToString(), Enums.ParameterType._int, responsibleId);
                _dataModelManager.AddParameter(DBEntityNames.GetEventResponsibleRoles.EventsXML.ToString(), Enums.ParameterType._xml, eventsXml);
                var data = _manager.ExecuteStoredProcedure(_dataModelManager.DataModel);

                if (data.Rows.Count > 0)
                {
                    return data.Rows[0]["Role"].ToString();
                }
                else
                {
                    _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.Event.ToString());
                    _dataModelManager.AddResultSet(new List<string>() { DBEntityNames.Event.Roles.ToString() });
                    _dataModelManager.AddFilter(DBEntityNames.Event.InstanceId.ToString(), Enums.ParameterType._int, instanceId, Enums.CompareOperator.equal, Enums.LogicalOperator.and);
                    _dataModelManager.AddFilter(DBEntityNames.Event.Id.ToString(), Enums.ParameterType._int, eventId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);
                    eventRole = _manager.SelectData(_dataModelManager.DataModel);
                    string role = eventRole.Rows[0]["Roles"].ToString();
                    string[] roles = role.Split(',');
                    return roles[0];
                }
            }
            else
            {
                _dataModelManager.DataModel.Filters.RemoveAt(_dataModelManager.DataModel.Filters.Count - 1);
                eventRole = _manager.SelectData(_dataModelManager.DataModel);
                string role = eventRole.Rows[0]["Roles"].ToString();
                string[] roles = role.Split(',');
                return roles[0];
            }
        }
    }
}