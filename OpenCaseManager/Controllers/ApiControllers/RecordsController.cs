using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenCaseManager.Commons;
using OpenCaseManager.Custom.Syddjurs;
using OpenCaseManager.Managers;
using OpenCaseManager.Models;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using System.Xml;

namespace OpenCaseManager.Controllers.ApiControllers
{
    [Authorize]
    [RoutePrefix("api/records")]
    public class RecordsController : ApiController
    {
        private IManager _manager;
        private IService _service;
        private IDCRService _dCRService;
        private IDataModelManager _dataModelManager;
        private IDocumentManager _documentManager;
        private IAutomaticEvents _automaticEvents;
        private ISyddjursWork _syddjursWork;
        private ICommons _commons;
        private ServicesController _servicesController;

        public RecordsController(IManager manager, IService service, IDCRService dCRService,
                                    IDataModelManager dataModelManager, IDocumentManager documentManager,
                                    ISyddjursWork syddjursWork, IAutomaticEvents automaticEvents,
                                    IMailRepository mailRepository, ICommons commons)
        {
            _manager = manager;
            _service = service;
            _dCRService = dCRService;
            _dataModelManager = dataModelManager;
            _automaticEvents = automaticEvents;
            _documentManager = documentManager;
            _syddjursWork = syddjursWork;
            _commons = commons;
            // This dependancy is only needed for CreateInstanceAPI, and should be removed if it is possible to add a call to servicesController via HTTP instead.
            _servicesController = new ServicesController(manager, service, dCRService, dataModelManager, documentManager, automaticEvents, mailRepository, commons);
        }

        // POST api/values
        [HttpPost]
        public IHttpActionResult Post(DataModel model)
        {
            try
            {
                if (model.Type == Enums.SQLOperation.SELECT.ToString())
                {
                    var output = _manager.SelectData(model);
                    return Ok(Common.ToJson(output));
                }
                return BadRequest("Only Select statement is allowed.");
            }
            catch (Exception ex)
            {
                _manager.LogSQLModel(model, ex, "Post");
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        // POST api/values
        [AllowAnonymous]
        [HttpPost]
        [Route("anonymous")]
        public IHttpActionResult PostAnonymous(DataModel model)
        {
            try
            {
                if (Enum.IsDefined(typeof(DBEntityNames.AnonymousTables), model.Entity) && model.Type == Enums.SQLOperation.SELECT.ToString())
                {
                    var output = _manager.SelectData(model);
                    return Ok(Common.ToJson(output));
                }
                return BadRequest("Only Select statement is allowed.");
            }
            catch (Exception ex)
            {
                _manager.LogSQLModel(model, ex, "Post");
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Add child 
        /// </summary>
        /// <param name="childName"></param>
        /// <param name="responsible"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("addChild")]
        // POST api/values
        public IHttpActionResult AddChildApi(AddChildModel input)
        {
            try
            {
                // add child 
                var childId = AddChild(input);
                AddChildName(input.ChildName, input.CaseNumber, childId);
                return Ok(Common.ToJson(childId));
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "AddChild - Failed. - " + Common.ToJson(input));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Return Test cases
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("getTestCases")]
        // GET api/values
        public IHttpActionResult getTestCasesApi()
        {
            try
            {
                _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SP, DBEntityNames.StoredProcedures.GetTestCases.ToString());
                return Ok(Common.ToJson(_manager.ExecuteStoredProcedure(_dataModelManager.DataModel)));
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "GetTestCase - Failed. ");
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Add Test Case
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("addTestCase")]
        // POST api/values
        public IHttpActionResult AddTestCaseApi(AddTestCase input)
        {
            try
            {
                return Ok(Common.ToJson(AddTestCase(input)));
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "AddTestCase - Failed. - " + Common.ToJson(input));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Update Test Case
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("updateTestCase")]
        //POST api/values
        public IHttpActionResult UpdateTestCaseApi(AddTestCase input)
        {
            try
            {
                UpdateTestCase(input);
                return Ok(Common.ToJson(new { }));
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "UpdateTestCase - Failed. - " + Common.ToJson(input));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Delete Test Case
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("deleteTestCase")]
        public IHttpActionResult DeleteTestCaseApi(dynamic input)
        {
            try
            {
                _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SP, DBEntityNames.StoredProcedures.DeleteTestCase.ToString());
                _dataModelManager.AddParameter(DBEntityNames.DeleteTestCase.Id.ToString(), Enums.ParameterType._int, input.ToString());

                var dataTable = _manager.ExecuteStoredProcedure(_dataModelManager.DataModel);

                return Ok(Common.ToJson(new { }));
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "DeleteTestCase - Failed. - " + Common.ToJson(input));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Add Instance
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("addInstance")]
        // POST api/values
        public IHttpActionResult AddInstanceApi(AddInstanceModel input)
        {
            try
            {
                // add Instance
                var instanceId = AddInstance(input);
                if (instanceId != "")
                {
                    return Ok(Common.ToJson(instanceId));
                }
                else
                {
                    Common.LogInfo(_manager, _dataModelManager, "AddInstance - Failed. - " + Common.ToJson(input));
                    Exception ex = new Exception("AddInstance failed");
                    Common.LogError(ex);
                    return InternalServerError(ex);
                }
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "AddInstance - Failed. - " + Common.ToJson(input));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        #region Anonymous Functions

        /// <summary>
        /// Return Delay time
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        [Route("getWarningDelay")]
        // GET api/values
        public IHttpActionResult GetWarningDelayApi(string id)
        {
            try
            {
                return Ok(Common.ToJson(GetWarningDelay(id)));
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "GetWarningDelay - Failed. ");
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Return Instance Phases
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        [Route("getInstancePhases")]
        // GET api/values
        public IHttpActionResult GetInstancePhasesApi(string id)
        {
            try
            {
                return Ok(Common.ToJson(GetInstancePhases(id)));
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "GetInstancePhases - Failed. ");
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Return Tasks
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        [Route("getTasks")]
        // GET api/values
        public IHttpActionResult GetTasksApi(string id)
        {
            try
            {
                return Ok(Common.ToJson(GetTasks(id)));
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "GetTask - Failed. ");
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Return My Test case
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        [Route("getMyTestCase")]
        // GET api/values
        public IHttpActionResult GetMyTestCaseApi(string id)
        {
            try
            {
                Guid guidOutput;
                if (!Guid.TryParse(id, out guidOutput))
                {
                    _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.TestCaseInstance.ToString());
                    _dataModelManager.AddResultSet(new List<string> { DBEntityNames.TestCaseInstance.TestCaseId.ToString()});
                    _dataModelManager.AddFilter(DBEntityNames.TestCaseInstance.InstanceId.ToString(), Enums.ParameterType._int, id, Enums.CompareOperator.equal, Enums.LogicalOperator.none);

                   id = _manager.SelectData(_dataModelManager.DataModel).Rows[0].ItemArray[0].ToString();

                }

                _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.TestCase.ToString());
                _dataModelManager.AddResultSet(new List<string> { DBEntityNames.TestCase.Title.ToString(), DBEntityNames.TestCase.ValidFrom.ToString(), DBEntityNames.TestCase.ValidTo.ToString(), DBEntityNames.TestCase.RoleToTest.ToString(), DBEntityNames.TestCase.DCRGraphId.ToString(), DBEntityNames.TestCase.Delay.ToString() });
                Guid guidResult;
                if (Guid.TryParse(id, out guidResult))
                    _dataModelManager.AddFilter(DBEntityNames.TestCase.Guid.ToString(), Enums.ParameterType._guid, id, Enums.CompareOperator.equal, Enums.LogicalOperator.none);
                else
                    _dataModelManager.AddFilter(DBEntityNames.TestCase.Id.ToString(), Enums.ParameterType._int, id, Enums.CompareOperator.equal, Enums.LogicalOperator.none);

                return Ok(Common.ToJson(_manager.SelectData(_dataModelManager.DataModel)));
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "GetMyTestCase - Failed. ");
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Launch Test Case
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [Route("launchTestCase")]
        // POST api/values
        public IHttpActionResult LaunchTestCaseApi(dynamic input)
        {
            try
            {
                var email = input["email"].ToString();
                var name = input["name"].ToString();
                var guid = input["guid"].ToString();
                return Ok(Common.ToJson(AddTestCaseInstance(new AddTestCaseInstance()
                {
                    email = email,
                    name = name,
                    guid = guid
                })));
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "AddTestCase - Failed. - " + Common.ToJson(input));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Return AI Robotic Events
        /// <param name="id"></param>
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        [Route("getAIRoboticEvents")]
        // GET api/values
        public IHttpActionResult GetAIRoboticEventsApi(string id)
        {
            try
            {
                return Ok(Common.ToJson(GetAIRoboticEvents(id)));
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "GetTestCase - Failed. ");
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        #endregion

        /// <summary>
        /// Add Instance and Initialize graph
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("createInstance")]
        // POST api/values
        public IHttpActionResult CreateInstanceApi(AddInstanceModel input)
        {
            try
            {
                // Call AddInstanceAPI
                var instanceString = AddInstance(input);

                // Call InitializeGraph via http


                var json = @"{""instanceId"":" + instanceString + @",""graphId"":" + input.GraphId + "}";
                dynamic graphInput = JObject.Parse(json);


                var result = _servicesController.InitializeGraph(graphInput, out string output);
                if (result)
                {
                    return Ok(Common.ToJson(instanceString));
                }
                else
                {
                    Common.LogInfo(_manager, _dataModelManager, "CreateInstance - InitializeGraph - Failed. - " + Common.ToJson(input));
                    Exception ex = new Exception();
                    Common.LogError(ex);
                    return InternalServerError(ex);
                }
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "CreateInstance - AddInstance - Failed. - " + Common.ToJson(input));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }




        /// <summary>
        /// Update Child
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateChild")]
        //POST api/values
        public IHttpActionResult UpdateChild(dynamic input)
        {
            var childObsBox = input["obsText"].ToString();
            var childId = input["childId"].ToString();

            UpdateChildObsBox(childId, childObsBox);
            return Ok(Common.ToJson(new { }));
        }

        /// <summary>
        /// Update ObsBox to Child
        /// </summary>
        /// <param name="input"></param>
        private void UpdateChildObsBox(string id, string input)
        {
            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.UPDATE, DBEntityNames.Tables.Adjunkt.ToString());
            _dataModelManager.AddParameter(DBEntityNames.Adjunkt.ObsBoxText.ToString(), Enums.ParameterType._string, input);
            _dataModelManager.AddFilter(DBEntityNames.Adjunkt.Id.ToString(), Enums.ParameterType._int, id, Enums.CompareOperator.equal, Enums.LogicalOperator.none);

            _manager.UpdateData(_dataModelManager.DataModel);
        }

        /// <summary>
        /// Add Process
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("addProcess")]
        // POST api/values
        public IHttpActionResult AddProcess(List<Model> input)
        {
            var countAdded = 0;
            // add Instance
            foreach (var process in input)
            {
                _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.Process.ToString());
                _dataModelManager.AddFilter(DBEntityNames.Process.GraphId.ToString(), Enums.ParameterType._int, process.GraphId.ToString(), Enums.CompareOperator.equal, Enums.LogicalOperator.none);
                _dataModelManager.AddResultSet(new List<string> { DBEntityNames.Process.Id.ToString(), DBEntityNames.Process.Status.ToString() });
                var isProcessExists = _manager.SelectData(_dataModelManager.DataModel);

                if (isProcessExists.Rows.Count > 0)
                {
                    try
                    {
                        if (isProcessExists.Rows[0]["Status"].ToString().ToLower() == false.ToString().ToLower())
                        {
                            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.UPDATE, DBEntityNames.Tables.Process.ToString());
                            _dataModelManager.AddParameter(DBEntityNames.Process.Status.ToString(), Enums.ParameterType._boolean, "true");
                            _dataModelManager.AddParameter(DBEntityNames.Process.Modified.ToString(), Enums.ParameterType._datetime, DateTime.Now.ToString());
                            _dataModelManager.AddFilter(DBEntityNames.Process.GraphId.ToString(), Enums.ParameterType._int, process.GraphId.ToString(), Enums.CompareOperator.equal, Enums.LogicalOperator.none);

                            _manager.UpdateData(_dataModelManager.DataModel);
                            countAdded++;
                        }
                    }
                    catch (Exception ex)
                    {
                        Common.LogInfo(_manager, _dataModelManager, "AddProcess - Process Status Update Failed. - " + Common.ToJson(process));
                        Common.LogError(ex);
                    }
                }
                else
                {
                    try
                    {
                        var graphXml = string.Empty;
                        graphXml = _dCRService.GetProcess(process.GraphId);

                        _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.ProcessHistory.ToString());
                        _dataModelManager.AddFilter(DBEntityNames.ProcessHistory.GraphId.ToString(), Enums.ParameterType._int, process.GraphId.ToString(), Enums.CompareOperator.equal, Enums.LogicalOperator.none);
                        _dataModelManager.AddResultSet(new List<string> { DBEntityNames.ProcessHistory.Id.ToString(), DBEntityNames.ProcessHistory.GraphId.ToString(), DBEntityNames.ProcessHistory.Status.ToString() });

                        var processHistories = _manager.SelectData(_dataModelManager.DataModel);
                        if (processHistories.Rows.Count == 0)
                        {
                            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.INSERT, DBEntityNames.Tables.ProcessHistory.ToString());
                            _dataModelManager.AddParameter(DBEntityNames.ProcessHistory.Title.ToString(), Enums.ParameterType._string, process.Title);
                            _dataModelManager.AddParameter(DBEntityNames.ProcessHistory.GraphId.ToString(), Enums.ParameterType._int, process.GraphId.ToString());
                            _dataModelManager.AddParameter(DBEntityNames.ProcessHistory.DCRXML.ToString(), Enums.ParameterType._xml, graphXml);
                            _dataModelManager.AddParameter(DBEntityNames.ProcessHistory.InstanceId.ToString(), Enums.ParameterType._int, process.InstanceId);
                            _dataModelManager.AddParameter(DBEntityNames.ProcessHistory.State.ToString(), Enums.ParameterType._int, "0");
                            _dataModelManager.AddParameter(DBEntityNames.ProcessHistory.Owner.ToString(), Enums.ParameterType._int, Common.GetResponsibleId());

                            var response = _dCRService.GetMajorRevisions(process.GraphId);
                            var xmlDocument = new XmlDocument();
                            xmlDocument.LoadXml(response);

                            var majorVersionId = 0;

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
                                    majorVersionId = majorVersionIds.Max();
                                    var title = majorVersionTitle[majorVersionId];
                                    var date = majorVersionDate[majorVersionId];


                                    _dataModelManager.AddParameter(DBEntityNames.ProcessHistory.MajorVerisonDate.ToString(), Enums.ParameterType._datetime, date.ToString());
                                    _dataModelManager.AddParameter(DBEntityNames.ProcessHistory.MajorVersionId.ToString(), Enums.ParameterType._int, majorVersionId.ToString());
                                    _dataModelManager.AddParameter(DBEntityNames.ProcessHistory.MajorVersionTitle.ToString(), Enums.ParameterType._string, title);
                                }
                            }
                            var processId = _manager.InsertData(_dataModelManager.DataModel);
                            countAdded++;
                        }
                    }
                    catch (Exception ex)
                    {
                        Common.LogInfo(_manager, _dataModelManager, "AddProcess - Add Process Failed. - " + Common.ToJson(process));
                        Common.LogError(ex);
                    }
                }
            }

            if (countAdded > 0)
                return Ok(countAdded);
            return Conflict();
        }


        /// <summary>
        /// Get Adjunkter from database
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetAdjunkter")]
        // Post api/values
        public IHttpActionResult GetAdjunkter(AdjunktModel input)
        {
            try
            {
                var adjunktId = input.AdjunktId;

                _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.AdjunktView.ToString());
                _dataModelManager.AddFilter(DBEntityNames.Adjunkt.Responsible.ToString(), Enums.ParameterType._int, adjunktId.ToString(), Enums.CompareOperator.equal, Enums.LogicalOperator.none);
                _dataModelManager.AddResultSet(new List<string> { DBEntityNames.AdjunktView.Id.ToString(), DBEntityNames.AdjunktView.Responsible.ToString(), DBEntityNames.AdjunktView.Name.ToString(), DBEntityNames.AdjunktView.ResponsibleName.ToString() });

                var datatable = _manager.SelectData(_dataModelManager.DataModel);
                return Ok(Common.ToJson(datatable));

            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "UpdateProcess - Failed. - " + Common.ToJson(input));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get current user id
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("GetCurrentUserId")]
        public IHttpActionResult GetCurrentUserId()
        {
            try
            {

                var userName = Common.GetCurrentUserName();

                _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, "[User]");
                _dataModelManager.AddFilter(DBEntityNames.User.SamAccountName.ToString(), Enums.ParameterType._string, userName, Enums.CompareOperator.equal, Enums.LogicalOperator.none);
                _dataModelManager.AddResultSet(new List<string> { DBEntityNames.User.Id.ToString() });


                var adjunktdatatable = _manager.SelectData(_dataModelManager.DataModel);

                int userId = (int)adjunktdatatable.Rows[0].ItemArray[0];

                return Ok(Common.ToJson(userId));

            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "GetCurrentUserId - Failed. - ");
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get Adjunkter from database
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetMineAktiviteterNoInput")]
        // Post api/values
        public IHttpActionResult GetMineAktiviteterNoInput()
        {
            try
            {

                var userId = "$(loggedInUserId)";

                _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.AdjunktUser.ToString());
                _dataModelManager.AddFilter(DBEntityNames.AdjunktUser.UserId.ToString(), Enums.ParameterType._string, userId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);
                _dataModelManager.AddResultSet(new List<string> { DBEntityNames.AdjunktUser.AdjunktId.ToString(), DBEntityNames.AdjunktUser.UserId.ToString() });


                var adjunktdatatable = _manager.SelectData(_dataModelManager.DataModel);

                if (adjunktdatatable.Rows.Count < 1) return InternalServerError(new Exception("No data found for user."));


                int adjunktId = (int)adjunktdatatable.Rows[0].ItemArray[0];
                int tempUserId = (int)adjunktdatatable.Rows[0].ItemArray[1];

                var input = new MineAktiviteterModel() { AdjunktId = adjunktId, UserId = tempUserId };

                OkNegotiatedContentResult<string> res = (OkNegotiatedContentResult<string>)GetMineAktiviteter(input);
                var content = res.Content;
                return Ok(content);

            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "GetMineAktiviteter - Failed. - ");
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get Adjunkter from database
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetMineAktiviteter")]
        // Post api/values
        public IHttpActionResult GetMineAktiviteter(MineAktiviteterModel input)
        {
            try
            {
                var adjunktId = input.AdjunktId;
                var userId = input.UserId;

                _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.MineAktiviteter.ToString());
                if (adjunktId != 0) _dataModelManager.AddFilter(DBEntityNames.MineAktiviteter.ChildId.ToString(), Enums.ParameterType._int, adjunktId.ToString(), Enums.CompareOperator.equal, Enums.LogicalOperator.or);
                if (userId != 0) _dataModelManager.AddFilter(DBEntityNames.MineAktiviteter.Responsible.ToString(), Enums.ParameterType._int, userId.ToString(), Enums.CompareOperator.equal, Enums.LogicalOperator.none);
                _dataModelManager.AddResultSet(new List<string> { DBEntityNames.MineAktiviteter.Acadreorgid.ToString(), DBEntityNames.MineAktiviteter.ChildId.ToString(), DBEntityNames.MineAktiviteter.Department.ToString(), DBEntityNames.MineAktiviteter.DepartmentId.ToString(), DBEntityNames.MineAktiviteter.Description.ToString(), DBEntityNames.MineAktiviteter.Due.ToString(), DBEntityNames.MineAktiviteter.EventId.ToString(), DBEntityNames.MineAktiviteter.EventType.ToString(), DBEntityNames.MineAktiviteter.EventTypeData.ToString(), DBEntityNames.MineAktiviteter.Familieafdelingen.ToString(), DBEntityNames.MineAktiviteter.InstanceId.ToString(), DBEntityNames.MineAktiviteter.IsEnabled.ToString(), DBEntityNames.MineAktiviteter.IsExecuted.ToString(), DBEntityNames.MineAktiviteter.IsIncluded.ToString(), DBEntityNames.MineAktiviteter.IsManager.ToString(),
                    DBEntityNames.MineAktiviteter.isOpen.ToString(), DBEntityNames.MineAktiviteter.IsPending.ToString(), DBEntityNames.MineAktiviteter.ManagerId.ToString(), DBEntityNames.MineAktiviteter.Name.ToString(), DBEntityNames.MineAktiviteter.NotApplicable.ToString(), DBEntityNames.MineAktiviteter.Note.ToString(), DBEntityNames.MineAktiviteter.NoteIsHtml.ToString(), DBEntityNames.MineAktiviteter.ParentId.ToString(), DBEntityNames.MineAktiviteter.PhaseId.ToString(), DBEntityNames.MineAktiviteter.Responsible.ToString(), DBEntityNames.MineAktiviteter.Roles.ToString(), DBEntityNames.MineAktiviteter.SamAccountName.ToString(), DBEntityNames.MineAktiviteter.Title.ToString(), DBEntityNames.MineAktiviteter.UserTitle.ToString(), DBEntityNames.MineAktiviteter.InstanceTitle.ToString(), DBEntityNames.MineAktiviteter.Type.ToString(), DBEntityNames.MineAktiviteter.Modified.ToString(), DBEntityNames.MineAktiviteter.GraphId.ToString(), DBEntityNames.MineAktiviteter.SimulationId.ToString(), DBEntityNames.MineAktiviteter.TrueEventId.ToString(), DBEntityNames.MineAktiviteter.EventTitle.ToString()});
                _dataModelManager.AddOrderBy(DBEntityNames.MineAktiviteter.IsPending.ToString(), true);
                _dataModelManager.AddOrderBy(DBEntityNames.MineAktiviteter.IsEnabled.ToString(), true);
                _dataModelManager.AddOrderBy("ISNULL(Due, DateFromParts(3000, 10, 10))", false);

                var datatable = _manager.SelectData(_dataModelManager.DataModel);
                return Ok(Common.ToJson(datatable));

            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "GetMineAktiviteter - Failed. - " + Common.ToJson(input));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get AdjunktInfo from database
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetAdjunktInfo")]
        // Post api/values
        public IHttpActionResult GetAdjunktInfo(AdjunktModel input)
        {
            try
            {
                var adjunktId = input.AdjunktId;

                _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.Adjunkt.ToString());
                _dataModelManager.AddFilter(DBEntityNames.Adjunkt.Id.ToString(), Enums.ParameterType._int, adjunktId.ToString(), Enums.CompareOperator.equal, Enums.LogicalOperator.none);
                _dataModelManager.AddResultSet(new List<string> { DBEntityNames.Adjunkt.Responsible.ToString(), DBEntityNames.Adjunkt.Name.ToString(), DBEntityNames.Adjunkt.Id.ToString() });

                var datatable = _manager.SelectData(_dataModelManager.DataModel);
                return Ok(Common.ToJson(datatable));

            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "GetAdjunktInfo - Failed. - " + Common.ToJson(input));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }



        /// <summary>
        /// Add Process Revision
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddProcessRevision")]
        // POST api/values
        public IHttpActionResult AddProcessRevision(List<Model> input)
        {
            try
            {
                var countAdded = 0;
                // add Instance
                foreach (var process in input)
                {

                    // check in approval process and mark as abort
                    _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.ProcessHistory.ToString());
                    _dataModelManager.AddFilter(DBEntityNames.ProcessHistory.GraphId.ToString(), Enums.ParameterType._int, process.GraphId.ToString(), Enums.CompareOperator.equal, Enums.LogicalOperator.and);
                    _dataModelManager.AddFilter(DBEntityNames.ProcessHistory.State.ToString(), Enums.ParameterType._int, 0.ToString(), Enums.CompareOperator.equal, Enums.LogicalOperator.none);
                    _dataModelManager.AddResultSet(new List<string> { DBEntityNames.ProcessHistory.Id.ToString(), DBEntityNames.ProcessHistory.InstanceId.ToString() });
                    var processes = _manager.SelectData(_dataModelManager.DataModel);

                    if (processes.Rows.Count > 0)
                    {
                        foreach (DataRow row in processes.Rows)
                        {
                            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.UPDATE, DBEntityNames.Tables.ProcessHistory.ToString());
                            _dataModelManager.AddParameter(DBEntityNames.ProcessHistory.State.ToString(), Enums.ParameterType._int, (-1).ToString());
                            _dataModelManager.AddFilter(DBEntityNames.ProcessHistory.Id.ToString(), Enums.ParameterType._int, row["Id"].ToString(), Enums.CompareOperator.equal, Enums.LogicalOperator.none);
                            _manager.UpdateData(_dataModelManager.DataModel);

                            // get instance details
                            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.Instance.ToString());
                            _dataModelManager.AddFilter(DBEntityNames.Instance.Id.ToString(), Enums.ParameterType._int, row["InstanceId"].ToString(), Enums.CompareOperator.equal, Enums.LogicalOperator.none);
                            _dataModelManager.AddResultSet(new List<string> { DBEntityNames.Instance.SimulationId.ToString(), DBEntityNames.Instance.GraphId.ToString() });
                            var instance = _manager.SelectData(_dataModelManager.DataModel);

                            // get event details
                            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.Event.ToString());
                            _dataModelManager.AddFilter(DBEntityNames.Event.EventId.ToString(), Enums.ParameterType._string, "AbortApproval", Enums.CompareOperator.equal, Enums.LogicalOperator.and);
                            _dataModelManager.AddFilter(DBEntityNames.Event.InstanceId.ToString(), Enums.ParameterType._int, row["InstanceId"].ToString(), Enums.CompareOperator.equal, Enums.LogicalOperator.none);
                            _dataModelManager.AddResultSet(new List<string> { DBEntityNames.Event.Id.ToString() });
                            var events = _manager.SelectData(_dataModelManager.DataModel);

                            // invoke AbortApproval in process instance
                            var obj = new
                            {
                                graphId = instance.Rows[0]["GraphId"].ToString(),
                                simulationId = instance.Rows[0]["SimulationId"].ToString(),
                                instanceId = row["InstanceId"].ToString(),
                                eventId = "AbortApproval",
                                trueEventId = events.Rows[0]["Id"].ToString()
                            };

                            var serviceModel = new ServiceModel()
                            {
                                BaseUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority),
                                Body = "",
                                MethodType = RestSharp.Method.POST,
                                Url = "api/services/ExecuteEvent"
                            };
                            var req = _service.GetRequest(serviceModel);
                            req.AddParameter("application/json", SimpleJson.SerializeObject(obj), ParameterType.RequestBody);

                            var client = new RestClient
                            {
                                BaseUrl = new Uri(serviceModel.BaseUrl)
                            };
                            client.Authenticator = new NtlmAuthenticator();
                            IRestResponse res = client.Execute(req);
                            if (res.StatusCode >= System.Net.HttpStatusCode.BadRequest)
                            {
                                throw new Exception(res.Content);
                            }

                        }
                    }

                    var graphXml = string.Empty;
                    graphXml = _dCRService.GetProcess(process.GraphId);

                    _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.INSERT, DBEntityNames.Tables.ProcessHistory.ToString());
                    _dataModelManager.AddParameter(DBEntityNames.ProcessHistory.Title.ToString(), Enums.ParameterType._string, process.Title);
                    _dataModelManager.AddParameter(DBEntityNames.ProcessHistory.GraphId.ToString(), Enums.ParameterType._int, process.GraphId.ToString());
                    _dataModelManager.AddParameter(DBEntityNames.ProcessHistory.DCRXML.ToString(), Enums.ParameterType._xml, graphXml);
                    _dataModelManager.AddParameter(DBEntityNames.ProcessHistory.InstanceId.ToString(), Enums.ParameterType._int, process.InstanceId);
                    _dataModelManager.AddParameter(DBEntityNames.ProcessHistory.State.ToString(), Enums.ParameterType._int, "0");
                    _dataModelManager.AddParameter(DBEntityNames.ProcessHistory.Owner.ToString(), Enums.ParameterType._int, Common.GetResponsibleId());

                    var response = _dCRService.GetMajorRevisions(process.GraphId);
                    var xmlDocument = new XmlDocument();
                    xmlDocument.LoadXml(response);

                    var majorVersionId = 0;

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
                            majorVersionId = majorVersionIds.Max();
                            var title = majorVersionTitle[majorVersionId];
                            var date = majorVersionDate[majorVersionId];


                            _dataModelManager.AddParameter(DBEntityNames.ProcessHistory.MajorVerisonDate.ToString(), Enums.ParameterType._datetime, date.ToString());
                            _dataModelManager.AddParameter(DBEntityNames.ProcessHistory.MajorVersionId.ToString(), Enums.ParameterType._int, majorVersionId.ToString());
                            _dataModelManager.AddParameter(DBEntityNames.ProcessHistory.MajorVersionTitle.ToString(), Enums.ParameterType._string, title);
                        }
                    }
                    try
                    {
                        var processId = _manager.InsertData(_dataModelManager.DataModel);
                        countAdded++;
                    }
                    catch (Exception ex)
                    {
                        Common.LogInfo(_manager, _dataModelManager, "AddProcessRevision - Failed. - " + Common.ToJson(process));
                        Common.LogError(ex);
                        return InternalServerError(ex);
                    }
                }

                if (countAdded > 0)
                    return Ok(countAdded);
                else
                    return BadRequest("No Revision is added");
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "AddProcessRevision - Failed. " + Common.ToJson(input));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Update Process
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateProcess")]
        // POST api/values
        public IHttpActionResult UpdateProcess(dynamic input)
        {
            try
            {
                var graphId = input["graphId"].ToString();
                var processTitle = input["processTitle"].ToString();
                var processStatus = input["processStatus"].ToString();
                var showOnFronPage = input["showOnFronPage"].ToString();

                _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.UPDATE, DBEntityNames.Tables.Process.ToString());
                _dataModelManager.AddParameter(DBEntityNames.Process.Title.ToString(), Enums.ParameterType._string, processTitle);
                _dataModelManager.AddParameter(DBEntityNames.Process.Status.ToString(), Enums.ParameterType._boolean, processStatus);
                _dataModelManager.AddParameter(DBEntityNames.Process.OnFrontPage.ToString(), Enums.ParameterType._boolean, showOnFronPage);
                _dataModelManager.AddParameter(DBEntityNames.Process.Modified.ToString(), Enums.ParameterType._datetime, DateTime.Now.ToString());
                _dataModelManager.AddFilter(DBEntityNames.Process.GraphId.ToString(), Enums.ParameterType._int, graphId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);

                _manager.UpdateData(_dataModelManager.DataModel);
                return Ok(Common.ToJson(new { }));
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "UpdateProcess - Failed. - " + Common.ToJson(input));
                Common.LogError(ex);
                return InternalServerError(ex);
            }

        }

        /// <summary>
        /// Update Process
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateProcessFromDCR")]
        // POST api/values
        public IHttpActionResult UpdateProcessFromDCR(dynamic input)
        {
            try
            {
                var processId = input["processId"].ToString();
                var graphId = input["graphId"].ToString();

                UpdateProcessAndPhases(processId, graphId);
                return Ok(Common.ToJson(new { }));
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "UpdateProcessFromDCR - Failed. - " + Common.ToJson(input));
                Common.LogError(ex);
                return InternalServerError(ex);
            }

        }

        /// <summary>
        /// Add a form
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddForm")]
        public IHttpActionResult AddForm(dynamic input)
        {
            try
            {
                var isFromTemplate = Boolean.Parse(input["isFromTemplate"].ToString());
                var templateFormId = input["templateFormId"].ToString();

                // new form data model
                _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.INSERT, DBEntityNames.Tables.Form.ToString());
                _dataModelManager.AddParameter(DBEntityNames.Form.Title.ToString(), Enums.ParameterType._string, "Untitled");
                _dataModelManager.AddParameter(DBEntityNames.Form.IsTemplate.ToString(), Enums.ParameterType._boolean, bool.FalseString);
                _dataModelManager.AddParameter(DBEntityNames.Form.UserId.ToString(), Enums.ParameterType._int, Common.GetResponsibleId());
                if (isFromTemplate)
                {
                    _dataModelManager.AddParameter(DBEntityNames.Form.FormTemplateId.ToString(), Enums.ParameterType._string, templateFormId);
                }

                var dataTable = _manager.InsertData(_dataModelManager.DataModel);
                var formId = 0;
                if (dataTable.Rows.Count > 0)
                    formId = int.Parse(dataTable.Rows[0][DBEntityNames.Form.Id.ToString()].ToString());

                if (isFromTemplate)
                {
                    _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SP, DBEntityNames.StoredProcedures.CopyFormFromTemplate.ToString());
                    _dataModelManager.AddParameter(DBEntityNames.CopyFormFromTemplate.FormId.ToString(), Enums.ParameterType._int, formId.ToString());
                    _dataModelManager.AddParameter(DBEntityNames.CopyFormFromTemplate.TemplateId.ToString(), Enums.ParameterType._int, templateFormId);

                    _manager.ExecuteStoredProcedure(_dataModelManager.DataModel);
                }

                return Ok(Common.ToJson(formId));
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "AddForm - Failed. - " + Common.ToJson(input));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Update form title/IsTemplate
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateForm")]
        public IHttpActionResult UpdateForm(dynamic input)
        {
            try
            {
                var isTemplate = input["isTemplate"].ToString();
                var title = input["title"].ToString();
                var formId = input["id"].ToString();

                _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.UPDATE, DBEntityNames.Tables.Form.ToString());
                _dataModelManager.AddParameter(DBEntityNames.Form.Title.ToString(), Enums.ParameterType._string, title);
                _dataModelManager.AddParameter(DBEntityNames.Form.IsTemplate.ToString(), Enums.ParameterType._boolean, isTemplate);
                _dataModelManager.AddFilter(DBEntityNames.Form.Id.ToString(), Enums.ParameterType._int, formId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);

                var dataTable = _manager.UpdateData(_dataModelManager.DataModel);

                return Ok(Common.ToJson(formId));
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "UpdateForm - Failed. - " + Common.ToJson(input));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Add a group/question
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddQuestion")]
        public IHttpActionResult AddQuestion(dynamic input)
        {
            try
            {
                var formId = input["formId"].ToString();
                var itemText = input["itemText"].ToString();
                var sequenceNumber = input["sequenceNumber"].ToString();
                var itemId = input["itemId"];
                var isGroup = input["isGroup"].ToString();

                _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.INSERT, DBEntityNames.Tables.FormItem.ToString());
                _dataModelManager.AddParameter(DBEntityNames.FormItem.FormId.ToString(), Enums.ParameterType._int, formId);
                _dataModelManager.AddParameter(DBEntityNames.FormItem.IsGroup.ToString(), Enums.ParameterType._boolean, isGroup);
                _dataModelManager.AddParameter(DBEntityNames.FormItem.SequenceNumber.ToString(), Enums.ParameterType._int, sequenceNumber);
                _dataModelManager.AddParameter(DBEntityNames.FormItem.ItemText.ToString(), Enums.ParameterType._string, itemText);
                if (itemId != null)
                {
                    _dataModelManager.AddParameter(DBEntityNames.FormItem.ItemId.ToString(), Enums.ParameterType._int, itemId.ToString());
                }

                var dataTable = _manager.InsertData(_dataModelManager.DataModel);

                var questionId = 0;
                if (dataTable.Rows.Count > 0)
                    questionId = int.Parse(dataTable.Rows[0][DBEntityNames.FormItem.Id.ToString()].ToString());

                return Ok(Common.ToJson(questionId));
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "AddQuestion - Failed. - " + Common.ToJson(input));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Set sequence number of group/question
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("SetQuestionSequence")]
        public IHttpActionResult SetQuestionSequence(dynamic input)
        {
            try
            {
                var itemId = input["itemId"].ToString();
                var parentId = input["targetId"].ToString();
                var position = input["position"].ToString();

                _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SP, DBEntityNames.StoredProcedures.SetFormItemSequence.ToString());
                _dataModelManager.AddParameter(DBEntityNames.SetFormItemSequence.Source.ToString(), Enums.ParameterType._int, itemId);
                _dataModelManager.AddParameter(DBEntityNames.SetFormItemSequence.Target.ToString(), Enums.ParameterType._int, parentId);
                _dataModelManager.AddParameter(DBEntityNames.SetFormItemSequence.Position.ToString(), Enums.ParameterType._int, position);

                var dataTable = _manager.ExecuteStoredProcedure(_dataModelManager.DataModel);

                return Ok(Common.ToJson(new { }));
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "SetQuestionSequence - Failed. - " + Common.ToJson(input));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Delete a question
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("DeleteQuestion")]
        public IHttpActionResult DeleteQuestion(dynamic input)
        {
            try
            {
                var itemId = input["itemId"].ToString();

                _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SP, DBEntityNames.StoredProcedures.DeleteFormItem.ToString());
                _dataModelManager.AddParameter(DBEntityNames.DeleteFormItem.FormItemId.ToString(), Enums.ParameterType._int, itemId);

                var dataTable = _manager.ExecuteStoredProcedure(_dataModelManager.DataModel);

                return Ok(Common.ToJson(new { }));
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "DeleteQuestion - Failed. - " + Common.ToJson(input));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Update a question
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateQuestion")]
        public IHttpActionResult UpdateQuestion(dynamic input)
        {
            try
            {
                var itemId = input["itemId"].ToString();
                var itemText = input["itemText"].ToString();

                _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.UPDATE, DBEntityNames.Tables.FormItem.ToString());
                _dataModelManager.AddParameter(DBEntityNames.FormItem.ItemText.ToString(), Enums.ParameterType._string, itemText);
                _dataModelManager.AddFilter(DBEntityNames.FormItem.Id.ToString(), Enums.ParameterType._int, itemId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);

                var dataTable = _manager.UpdateData(_dataModelManager.DataModel);

                return Ok(Common.ToJson(new { }));
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "UpdateQuestion - Failed. - " + Common.ToJson(input));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Add custom values for an instance
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddInstanceCustomAttributes")]
        public IHttpActionResult AddInstanceCustomAttributes(dynamic input)
        {
            try
            {
                var instanceId = input["instanceId"].ToString();
                var employee = input["employee"].ToString();

                _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.INSERT, DBEntityNames.Tables.InstanceExtension.ToString());
                _dataModelManager.AddParameter(DBEntityNames.InstanceExtension.InstanceId.ToString(), Enums.ParameterType._int, instanceId);
                _dataModelManager.AddParameter(DBEntityNames.InstanceExtension.Employee.ToString(), Enums.ParameterType._string, employee);
                _dataModelManager.AddParameter(DBEntityNames.InstanceExtension.Year.ToString(), Enums.ParameterType._int, DateTime.Now.Year.ToString());

                var dataTable = _manager.InsertData(_dataModelManager.DataModel);

                return Ok(Common.ToJson(new { }));
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "AddInstanceCustomAttributes - Failed. - " + Common.ToJson(input));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Add Document
        /// </summary>
        /// <returns></returns>
        [Route("AddDocument")]
        [HttpPost]
        public IHttpActionResult AddDocument()
        {
            try
            {
                var request = HttpContext.Current.Request;
                var givenFileName = request.Headers["givenFileName"];
                var fileName = request.Headers["filename"];
                var fileType = request.Headers["type"];
                var instanceId = request.Headers["instanceId"];
                var childId = string.Empty;
                try
                {
                    childId = request.Headers["childId"];
                }
                catch (Exception) { }
                var eventId = string.Empty;
                try
                {
                    eventId = request.Headers["eventId"];
                }
                catch (Exception) { }
                var eventTime = DateTime.Now;
                try
                {
                    eventTime = request.Headers["eventTime"].parseDanishDateToDate();
                }
                catch (Exception) { }
                var isDraft = false;
                try
                {
                    isDraft = Convert.ToBoolean(request.Headers["isDraft"]);
                }
                catch (Exception) { }
                var filePath = string.Empty;
                var documentId = string.Empty;
                if (fileType == "JournalNoteBig")
                {
                    var addedDocument = _documentManager.AddDocument(instanceId, fileType, givenFileName, fileName, eventId, isDraft, childId, eventTime, _manager, _dataModelManager);
                    filePath = addedDocument.Item1;
                    documentId = addedDocument.Item2;
                }
                else filePath = _documentManager.AddDocument(instanceId, fileType, givenFileName, fileName, eventId, _manager, _dataModelManager);
                try
                {
                    using (var fs = new FileStream(filePath, FileMode.Create))
                    {
                        request.InputStream.CopyTo(fs);
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                var docId = 0;
                var parseResult = int.TryParse(documentId, out docId);

                return Ok(Common.ToJson(docId == 0 ? "" : docId.ToString()));
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "AddDocument - Failed. - " + Common.ToJson(Request.Headers));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Update Document
        /// </summary>
        /// <returns></returns>
        [Route("UpdateDocument")]
        [HttpPost]
        public IHttpActionResult UpdateDocument()
        {
            try
            {
                var request = HttpContext.Current.Request;
                var id = request.Headers["id"];
                var givenFileName = request.Headers["givenFileName"];
                var fileType = request.Headers["type"];
                var instanceId = request.Headers["instanceId"];
                var isNewFileAdded = bool.Parse(request.Headers["isNewFileAdded"]);
                var fileLink = string.Empty;
                var eventTime = DateTime.Now;
                try
                {
                    eventTime = Convert.ToDateTime(request.Headers["eventTime"]);
                }
                catch (Exception) { }
                var isDraft = false;
                try
                {
                    isDraft = Convert.ToBoolean(request.Headers["isDraft"]);
                }
                catch (Exception) { }
                if (isNewFileAdded)
                {
                    DeleteFileFromFileSystem(id, fileType, instanceId);

                    var fileName = request.Headers["filename"];
                    string ext = Path.GetExtension(fileName);
                    fileLink = DateTime.Now.ToFileTime() + ext;
                    var filePath = string.Empty;

                    switch (fileType)
                    {
                        case "PersonalDocument":
                            var directoryInfo = new DirectoryInfo(Configurations.Config.PersonalFileLocation);
                            if (!directoryInfo.Exists)
                            {
                                directoryInfo.Create();
                            }
                            var currentUser = Common.GetCurrentUserName();
                            directoryInfo = new DirectoryInfo(Configurations.Config.PersonalFileLocation + "\\" + currentUser);
                            if (!directoryInfo.Exists)
                            {
                                directoryInfo.Create();
                            }
                            filePath = directoryInfo.FullName;
                            break;
                        case "InstanceDocument":
                            directoryInfo = new DirectoryInfo(Configurations.Config.InstanceFileLocation);
                            if (!directoryInfo.Exists)
                            {
                                directoryInfo.Create();
                            }
                            directoryInfo = new DirectoryInfo(Configurations.Config.InstanceFileLocation + "\\" + instanceId);
                            if (!directoryInfo.Exists)
                            {
                                directoryInfo.Create();
                            }
                            filePath = directoryInfo.FullName;
                            break;
                        case "JournalNoteImportant":
                            directoryInfo = new DirectoryInfo(Configurations.Config.JournalNoteFileLocation);
                            if (!directoryInfo.Exists)
                            {
                                directoryInfo.Create();
                            }
                            directoryInfo = new DirectoryInfo(Configurations.Config.JournalNoteFileLocation + "\\" + instanceId);
                            if (!directoryInfo.Exists)
                            {
                                directoryInfo.Create();
                            }
                            filePath = directoryInfo.FullName;
                            break;
                        case "JournalNoteLittle": //Should only temporarily be allowed to be edited
                            directoryInfo = new DirectoryInfo(Configurations.Config.JournalNoteFileLocation);
                            if (!directoryInfo.Exists)
                            {
                                directoryInfo.Create();
                            }
                            directoryInfo = new DirectoryInfo(Configurations.Config.JournalNoteFileLocation + "\\" + instanceId);
                            if (!directoryInfo.Exists)
                            {
                                directoryInfo.Create();
                            }
                            filePath = directoryInfo.FullName;
                            break;
                        case "JournalNoteBig": //Should only temporarily be allowed to be edited
                            directoryInfo = new DirectoryInfo(Configurations.Config.JournalNoteFileLocation);
                            if (!directoryInfo.Exists)
                            {
                                directoryInfo.Create();
                            }
                            directoryInfo = new DirectoryInfo(Configurations.Config.JournalNoteFileLocation + "\\" + instanceId);
                            if (!directoryInfo.Exists)
                            {
                                directoryInfo.Create();
                            }
                            filePath = directoryInfo.FullName;
                            break;
                    }
                    filePath = filePath + "\\" + fileLink;

                    try
                    {
                        using (var fs = new FileStream(filePath, FileMode.Create))
                        {
                            request.InputStream.CopyTo(fs);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                UpdateDocument(id, givenFileName, fileLink, isDraft.ToString());
                if (fileType == "JournalNoteBig") UpdateJournalHistoryDocument(id, givenFileName, eventTime);

                return Ok(Common.ToJson(new { }));
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "UpdateDocument - Failed. - " + Common.ToJson(Request.Headers));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Delete Document
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Route("DeleteDocument")]
        [HttpPost]
        public IHttpActionResult DeleteDocument(dynamic input)
        {
            try
            {
                var id = input["Id"].ToString();
                var type = input["Type"].ToString();
                var instanceId = input["InstanceId"].ToString();

                // delete document from filesystem
                var isDeleted = DeleteFileFromFileSystem(id, type, instanceId);
                if (isDeleted)
                {
                    // delete document from DB
                    DeleteDocument(id);
                }
                return Ok(Common.ToJson(new { }));
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "DeleteDocument - Failed. - " + Common.ToJson(input));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get Document URL
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Route("GetDocumentsUrl")]
        [HttpPost]
        public IHttpActionResult GetDocumentsUrl(dynamic input)
        {
            try
            {
                var type = input["Type"].ToString();
                var instanceId = input["InstanceId"].ToString();
                var documentsUrl = CopyToTempFolder(type, instanceId);

                return Ok(Common.ToJson(documentsUrl));
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "GetDocumentsUrl - Failed. - " + Common.ToJson(input));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get Document URL
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Route("CleanUpTempDocuments")]
        [HttpPost]
        public IHttpActionResult CleanUpTempDocuments(dynamic input)
        {
            try
            {
                var urls = new List<string>();
                for (var i = 0; i < input["docsUrl"].Count; i++)
                {
                    urls.Add(AppDomain.CurrentDomain.BaseDirectory + "tmp" + input["docsUrl"][i].ToString().Split(new string[] { "tmp" }, StringSplitOptions.None)[1]);
                }

                foreach (var url in urls)
                {
                    var fileInfo = new FileInfo(url);
                    if (fileInfo.Exists)
                    {
                        fileInfo.Directory.Delete(true);
                    }
                }
                return Ok(Common.ToJson(new { }));
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "CleanUpTempDocuments - Failed. - " + Common.ToJson(input));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Replace values in event type parameters
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Route("ReplaceEventTypeParamsKeys")]
        [HttpPost]
        public IHttpActionResult ReplaceEventTypeParamsKeys(dynamic input)
        {
            try
            {
                var eventTypeValue = input["eventTypeValue"].ToString();
                var instanceId = input["instanceId"].ToString();
                var actualValue = Common.ReplaceEventTypeKeyValues(eventTypeValue, instanceId, _manager, _dataModelManager);
                return Ok(actualValue);
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "ReplaceEventTypeParamsKeys - Failed. - " + Common.ToJson(input));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Upload form to personal folder
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Route("UploadFormToPersonalFolder")]
        [HttpPost]
        public IHttpActionResult UploadFormToPersonalFolder(dynamic input)
        {
            try
            {
                var formId = input["formId"].ToString();
                var formName = input["formName"].ToString();
                var documentType = input["type"].ToString();

                var givenFileName = String.Format("{0:yyyyMMdd}", DateTime.Now) + "_" + formName;
                var fileName = givenFileName;
                byte[] data = { };

                if (documentType == "word")
                {
                    fileName += ".docx";

                    var html = Common.GetFormHtml(formId, _manager, _dataModelManager);
                    var path = Common.GetFormWordPath(html, _service);
                    var formDocumentPath = JsonConvert.DeserializeObject<dynamic>(path);
                    data = File.ReadAllBytes(formDocumentPath.success.ToString());
                }
                else if (documentType == "pdf")
                {
                    fileName += ".pdf";
                    data = Common.GetFormData(formId, _manager, _dataModelManager);
                }

                var filePath = _documentManager.AddDocument(string.Empty, "Personal", givenFileName, fileName, string.Empty, _manager, _dataModelManager);
                Common.SaveBytesToFile(filePath, data);

                return Ok(Common.ToJson(new { }));
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "UploadFormToPersonalFolder - Failed. - " + Common.ToJson(input));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Replace values in event type parameters
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("LogJsError")]
        [HttpPost]
        public IHttpActionResult LogJsError(dynamic input)
        {
            try
            {
                var message = input["message"].ToString();
                var source = input["source"].ToString();
                var stack = input["stack"].ToString();

                _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.INSERT, DBEntityNames.Tables.Log.ToString());
                _dataModelManager.AddParameter(DBEntityNames.Log.Level.ToString(), Enums.ParameterType._string, "JsError");
                _dataModelManager.AddParameter(DBEntityNames.Log.UserName.ToString(), Enums.ParameterType._string, Common.GetCurrentUserName());
                _dataModelManager.AddParameter(DBEntityNames.Log.ServerName.ToString(), Enums.ParameterType._string, Request.RequestUri.Host);
                _dataModelManager.AddParameter(DBEntityNames.Log.Port.ToString(), Enums.ParameterType._string, Request.RequestUri.Port.ToString());
                _dataModelManager.AddParameter(DBEntityNames.Log.Url.ToString(), Enums.ParameterType._string, source);
                _dataModelManager.AddParameter(DBEntityNames.Log.Https.ToString(), Enums.ParameterType._boolean, Common.IsHttps().ToString());
                _dataModelManager.AddParameter(DBEntityNames.Log.Message.ToString(), Enums.ParameterType._string, message);
                _dataModelManager.AddParameter(DBEntityNames.Log.Exception.ToString(), Enums.ParameterType._string, stack);
                _manager.InsertData(_dataModelManager.DataModel);

                return Ok(Common.ToJson(new { }));
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "LogJsError - Failed. - " + Common.ToJson(input));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Add MUS Role
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Route("AddMUSRole")]
        [HttpPost]
        public IHttpActionResult AddMUSRole(dynamic input)
        {
            try
            {
                var instanceId = input["instanceId"].ToString();
                var employee = input["employee"].ToString();
                var roles = new List<string>();
                for (var i = 0; i < input["roles"].Count; i++)
                {
                    roles.Add(input["roles"][i].ToString());
                }
                var userRoles = new List<UserRole>();
                foreach (var role in roles)
                {
                    if (role == Configurations.Config.MUSLeaderRole)
                    {
                        userRoles.Add(new UserRole()
                        {
                            RoleId = role,
                            UserId = int.Parse(Common.GetResponsibleId())
                        });
                    }
                    else if (role == Configurations.Config.MUSEmployeeRole)
                    {
                        userRoles.Add(new UserRole()
                        {
                            RoleId = role,
                            UserId = int.Parse(Common.GetResponsibleFullDetails(_manager, _dataModelManager, employee).Rows[0]["Id"].ToString())
                        });
                    }
                    else
                    {
                        userRoles.Add(new UserRole()
                        {
                            RoleId = role,
                            UserId = int.Parse(Common.GetResponsibleId())
                        });
                    }
                }
                Common.AddInstanceRoles(userRoles, instanceId, _manager, _dataModelManager);
                return Ok(Common.ToJson(new { }));
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "AddMUSRole - Failed. - " + Common.ToJson(input));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Update Comment for Tasks With Note
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("SetTasksWNoteComment")]
        public IHttpActionResult SetTasksWNoteComment(dynamic input)
        {
            try
            {
                var eventId = input["eventId"].ToString();
                var instanceId = input["instanceId"].ToString();
                var note = input["note"].ToString();
                var isHtml = input["isHtml"].ToString();

                if (eventId.ToLower().StartsWith("global".ToLower()))
                {
                    var childId = Common.GetInstanceChildId(_manager, _dataModelManager, instanceId);
                    if (childId > 0)
                    {
                        _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SP, DBEntityNames.StoredProcedures.GetGlobalEvents.ToString());
                        _dataModelManager.AddParameter(DBEntityNames.GetGlobalEvents.ChildId.ToString(), Enums.ParameterType._int, childId.ToString());
                        _dataModelManager.AddParameter(DBEntityNames.GetGlobalEvents.EventId.ToString(), Enums.ParameterType._string, eventId);
                        var globalEvents = _manager.ExecuteStoredProcedure(_dataModelManager.DataModel);

                        foreach (DataRow globalEvent in globalEvents.Rows)
                        {
                            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.UPDATE, DBEntityNames.Tables.Event.ToString());
                            _dataModelManager.AddParameter(DBEntityNames.Event.Note.ToString(), Enums.ParameterType._string, note);
                            _dataModelManager.AddParameter(DBEntityNames.Event.NoteIsHtml.ToString(), Enums.ParameterType._boolean, isHtml);
                            _dataModelManager.AddFilter(DBEntityNames.Event.EventId.ToString(), Enums.ParameterType._string, eventId, Enums.CompareOperator.equal, Enums.LogicalOperator.and);
                            _dataModelManager.AddFilter(DBEntityNames.Event.InstanceId.ToString(), Enums.ParameterType._int, globalEvent["InstanceId"].ToString(), Enums.CompareOperator.equal, Enums.LogicalOperator.none);

                            var dataTable = _manager.UpdateData(_dataModelManager.DataModel);
                        }
                    }
                }
                else
                {
                    _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.UPDATE, DBEntityNames.Tables.Event.ToString());
                    _dataModelManager.AddParameter(DBEntityNames.Event.Note.ToString(), Enums.ParameterType._string, note);
                    _dataModelManager.AddParameter(DBEntityNames.Event.NoteIsHtml.ToString(), Enums.ParameterType._boolean, isHtml);
                    _dataModelManager.AddFilter(DBEntityNames.Event.EventId.ToString(), Enums.ParameterType._string, eventId, Enums.CompareOperator.equal, Enums.LogicalOperator.and);
                    _dataModelManager.AddFilter(DBEntityNames.Event.InstanceId.ToString(), Enums.ParameterType._int, instanceId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);

                    _manager.UpdateData(_dataModelManager.DataModel);
                }

                return Ok(Common.ToJson(new { }));
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "SetTasksWNoteComment - Failed. - " + Common.ToJson(input));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get Menu Items
        /// </summary>
        /// <returns></returns>        
        [HttpGet]
        [Route("GetMenuItems")]
        public IHttpActionResult GetMenuItems()
        {
            try
            {
                _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SP, DBEntityNames.StoredProcedures.GetMenuItems.ToString());
                _dataModelManager.AddParameter(DBEntityNames.Form.UserId.ToString(), Enums.ParameterType._int, Common.GetResponsibleId());

                var datatable = _manager.ExecuteStoredProcedure(_dataModelManager.DataModel);
                return Ok(Common.ToJson(datatable));
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "GetMenuItems - Failed.");
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Update Responsible
        /// </summary>
        /// <returns></returns>        
        [HttpPost]
        [Route("UpdateResponsible")]
        public IHttpActionResult UpdateResponsible(dynamic input)
        {

            try
            {
                string instanceId = input["instanceId"].ToString();
                string eventId = input["trueEventId"] == null ? "" : input["trueEventId"].ToString();
                string childId = input["childId"] == null ? "" : input["childId"].ToString();

                string responsible = input["responsible"].ToString();
                string changeResponsibleFor = input["changeFor"].ToString();
                string oldResponsibleSamAccountName = input["oldResponsible"].ToString();

                var newResponsible = Common.GetUserName(_manager, _dataModelManager, responsible);
                var internalCaseId = string.Empty;
                switch (changeResponsibleFor)
                {
                    case "activity":
                        _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.UPDATE, DBEntityNames.Tables.Event.ToString());
                        _dataModelManager.AddParameter(DBEntityNames.Event.Responsible.ToString(), Enums.ParameterType._int, responsible);
                        _dataModelManager.AddFilter(DBEntityNames.Event.Id.ToString(), Enums.ParameterType._int, eventId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);

                        _manager.UpdateData(_dataModelManager.DataModel);
                        return Ok(Common.ToJson(new object { }));
                    case "instance":
                        _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.UPDATE, DBEntityNames.Tables.Instance.ToString());
                        _dataModelManager.AddParameter(DBEntityNames.Instance.Responsible.ToString(), Enums.ParameterType._int, responsible);
                        _dataModelManager.AddFilter(DBEntityNames.Instance.Id.ToString(), Enums.ParameterType._int, instanceId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);

                        _manager.UpdateData(_dataModelManager.DataModel);
                        return Ok(Common.ToJson(new object { }));
                    case "child":
                        _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.UPDATE, DBEntityNames.Tables.Adjunkt.ToString());
                        _dataModelManager.AddParameter(DBEntityNames.Adjunkt.Responsible.ToString(), Enums.ParameterType._int, responsible);
                        _dataModelManager.AddFilter(DBEntityNames.Adjunkt.Id.ToString(), Enums.ParameterType._int, childId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);

                        _manager.UpdateData(_dataModelManager.DataModel);
                        return Ok(Common.ToJson(new object { }));
                    default:
                        return BadRequest();
                }

            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "GetMenuItems - Failed.");
                Common.LogError(ex);
                return InternalServerError(ex);
            }


        }




        /// <summary>
        /// Update Responsible
        /// </summary>
        /// <returns></returns>        
        [HttpPost]
        [Route("UpdateResponsibleAcadre")]
        public IHttpActionResult UpdateResponsibleAcadre(dynamic input)
        {
            try
            {
                string instanceId = input["instanceId"].ToString();
                string eventId = input["trueEventId"] == null ? "" : input["trueEventId"].ToString();
                string childId = input["childId"] == null ? "" : input["childId"].ToString();

                string responsible = input["responsible"].ToString();
                string changeResponsibleFor = input["changeFor"].ToString();
                string oldResponsibleSamAccountName = input["oldResponsible"].ToString();

                var newResponsible = Common.GetUserName(_manager, _dataModelManager, responsible);
                var internalCaseId = string.Empty;
                DataTable instanceDetails = new DataTable();

                switch (changeResponsibleFor)
                {
                    case "activity":
                        // get instance details
                        childId = Common.GetInstanceChildId(_manager, _dataModelManager, instanceId).ToString();
                        instanceDetails = Common.GetInstanceDetails(_manager, _dataModelManager, instanceId);
                        internalCaseId = instanceDetails.Rows[0][DBEntityNames.Instance.InternalCaseID.ToString()].ToString();
                        break;
                    case "instance":
                        // get instance details
                        instanceDetails = Common.GetInstanceDetails(_manager, _dataModelManager, instanceId);
                        var oldResponsibleId = instanceDetails.Rows[0][DBEntityNames.Instance.Responsible.ToString()].ToString();
                        oldResponsibleSamAccountName = Common.GetUserName(_manager, _dataModelManager, oldResponsibleId);
                        internalCaseId = instanceDetails.Rows[0][DBEntityNames.Instance.InternalCaseID.ToString()].ToString();
                        if (string.IsNullOrWhiteSpace(childId))
                        {
                            childId = Common.GetInstanceChildId(_manager, _dataModelManager, instanceId).ToString();
                        }
                        Common.UpdateInstanceResponsible(instanceId, internalCaseId, oldResponsibleSamAccountName, newResponsible, _manager, _dataModelManager);
                        break;
                    case "child":
                        Common.UpdateChildResponsible(childId, oldResponsibleSamAccountName, newResponsible, _manager, _dataModelManager);
                        break;
                }

                Common.LogInfo(_manager, _dataModelManager, "ChangeResponsibleOfChild - childId : " + childId + ",instanceId : " + instanceId + ",eventId : " + eventId + ",FromInitials : " + oldResponsibleSamAccountName + ",ToInitials : " + newResponsible);

                _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SP, DBEntityNames.StoredProcedures.ChangeResponsibleOfChild.ToString());
                _dataModelManager.AddParameter(DBEntityNames.ChangeResponsibleOfChild.ChildId.ToString(), Enums.ParameterType._int, childId.ToString());
                if (!string.IsNullOrWhiteSpace(instanceId))
                    _dataModelManager.AddParameter(DBEntityNames.ChangeResponsibleOfChild.InstanceId.ToString(), Enums.ParameterType._int, instanceId.ToString());
                if (!string.IsNullOrWhiteSpace(eventId))
                    _dataModelManager.AddParameter(DBEntityNames.ChangeResponsibleOfChild.EventId.ToString(), Enums.ParameterType._int, eventId.ToString());
                _dataModelManager.AddParameter(DBEntityNames.ChangeResponsibleOfChild.FromInitials.ToString(), Enums.ParameterType._string, oldResponsibleSamAccountName);
                _dataModelManager.AddParameter(DBEntityNames.ChangeResponsibleOfChild.ToInitials.ToString(), Enums.ParameterType._string, newResponsible);
                var dataTable = _manager.ExecuteStoredProcedure(_dataModelManager.DataModel);

                if (dataTable.Rows.Count > 0)
                {
                    if (!string.IsNullOrWhiteSpace(dataTable.Rows[0]["Message"].ToString()) && (!string.IsNullOrWhiteSpace(internalCaseId) || !string.IsNullOrWhiteSpace(childId)))
                    {
                        var accessCode = "BO";
                        var caseFileReference = childId;
                        var creator = Common.GetCurrentUserName();
                        var fileName = DateTime.Now.ToFileTime() + ".rtf";
                        var memoTitleText = string.IsNullOrWhiteSpace(dataTable.Rows[0]["Title"].ToString()) ? "Ændr ansvarlig" : dataTable.Rows[0]["Title"].ToString();
                        var memoTypeReference = "2";
                        var memoIsLocked = true;
                        var fileBytes = Common.GetRTFDocument(dataTable.Rows[0]["Message"].ToString(), _manager, _dataModelManager, instanceId, string.Empty, changeResponsibleFor == "child" ? childId : string.Empty);
                        var date = DateTime.UtcNow;

                        var parameters = new Dictionary<string, dynamic>
                            {
                                { "accessCode", accessCode },
                                { "caseFileReference", caseFileReference },
                                { "creator", creator },
                                { "fileName", fileName },
                                { "memoTitleText", memoTitleText },
                                { "memoTypeReference", memoTypeReference },
                                { "memoIsLocked", memoIsLocked },
                                { "date", date },
                                { "xmlBinary", Encoding.ASCII.GetString(fileBytes) }
                            };

                        try
                        {
                            Common.SaveEventTypeDataParamertes(instanceId, parameters, "CreateMemoAcadre", null, _manager, _dataModelManager);

                            AcadrePWS.CaseManagement.ActingFor(Common.GetCurrentUserName());
                            AcadrePWS.CaseManagement.CreateMemo(fileName, accessCode, caseFileReference, memoTitleText, creator, memoTypeReference, memoIsLocked, fileBytes, date);
                        }
                        catch (Exception ex)
                        {
                            Common.SaveEventTypeDataParamertes(instanceId, parameters, "CreateMemoAcadre", ex, _manager, _dataModelManager);
                            Common.LogError(ex);
                        }
                    }
                    else
                    {
                        Common.LogInfo(_manager, _dataModelManager, "ChangeResponsible CreateJournalAcadre - Message, InternalCaseId or JournalCaseId is empty");
                    }
                }
                return Ok(Common.ToJson(new object { }));
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "UpdateResponsible - Failed. - " + Common.ToJson(input));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Update Instance Title
        /// </summary>
        /// <returns></returns>        
        [HttpPost]
        [Route("UpdateInstanceTitle")]
        public IHttpActionResult UpdateInstanceTitle(dynamic input)
        {
            try
            {
                string instanceTitle = input["title"].ToString();
                string instanceId = input["instanceId"].ToString();

                _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.UPDATE, DBEntityNames.Tables.Instance.ToString());
                _dataModelManager.AddParameter(DBEntityNames.Instance.Title.ToString(), Enums.ParameterType._string, instanceTitle);
                _dataModelManager.AddFilter(DBEntityNames.Instance.Id.ToString(), Enums.ParameterType._int, instanceId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);
                _manager.UpdateData(_dataModelManager.DataModel);

                _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.Instance.ToString());
                _dataModelManager.AddResultSet(new List<string>() { DBEntityNames.Instance.InternalCaseID.ToString() });
                _dataModelManager.AddFilter(DBEntityNames.Instance.Id.ToString(), Enums.ParameterType._int, instanceId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);
                var data = _manager.SelectData(_dataModelManager.DataModel);

                if (data.Rows.Count > 0 && data.Rows[0][DBEntityNames.Instance.InternalCaseID.ToString()] != null && data.Rows[0][DBEntityNames.Instance.InternalCaseID.ToString()].ToString() != string.Empty)
                    _syddjursWork.UpdateCaseContent(int.Parse(data.Rows[0][DBEntityNames.Instance.InternalCaseID.ToString()].ToString()), instanceTitle);

                return Ok(Common.ToJson(new object { }));
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "UpdateInstanceTitle - Failed. - " + Common.ToJson(input));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Copy all files to a temp folder
        /// </summary>
        /// <param name="type"></param>
        /// <param name="instanceId"></param>
        /// <returns></returns>
        private List<string> CopyToTempFolder(string type, string instanceId)
        {
            try
            {
                var documentUrl = new List<string>();
                var selectedDocuments = SelectPersonDocumentByPerson(new List<string> { "Title", "Link" });
                if (selectedDocuments.Rows.Count > 0)
                {
                    var dirPath = "tmp\\" + DateTime.Now.ToFileTime();

                    foreach (DataRow document in selectedDocuments.Rows)
                    {
                        var path = string.Empty;
                        // delete document from file system
                        switch (type)
                        {
                            case "PersonalDocument":
                                var currentUser = Common.GetCurrentUserName();
                                path = Configurations.Config.PersonalFileLocation + "\\" + currentUser + "\\" + document["Link"].ToString();
                                break;
                            case "InstanceDocument":
                                path = Configurations.Config.InstanceFileLocation + "\\" + instanceId + "\\" + document["Link"].ToString();
                                break;
                        }

                        var fileInfo = new FileInfo(path);
                        if (fileInfo.Exists)
                        {
                            try
                            {
                                var destFilePath = dirPath + "\\" + document["Title"].ToString() + Path.GetExtension(path);
                                var destPath = AppDomain.CurrentDomain.BaseDirectory + destFilePath;
                                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + dirPath);

                                var destFileInfor = new FileInfo(destPath);
                                if (destFileInfor.Exists)
                                {
                                    destFileInfor.Delete();
                                }
                                fileInfo.CopyTo(destPath);
                                documentUrl.Add(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + "/" + destFilePath);
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                        }
                    }
                }
                return documentUrl;
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "CopyToTempFolder - Failed. - instanceId : " + instanceId + ", type : " + type);
                Common.LogError(ex);
                throw ex;
            }
        }

        /// <summary>
        /// Add Child 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string AddChild(AddChildModel model)
        {
            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.INSERT, DBEntityNames.Tables.Child.ToString());
            _dataModelManager.AddParameter(DBEntityNames.Child.Responsible.ToString(), Enums.ParameterType._int, Common.GetResponsibleId());
            // Returns child id
            return _manager.InsertData(_dataModelManager.DataModel).Rows[0][DBEntityNames.Child.Id.ToString()].ToString();
        }
        

        /// <summary>
        /// Add Child 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string AddTestCase(AddTestCase model)
        {
            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.INSERT, DBEntityNames.Tables.TestCase.ToString());
            _dataModelManager.AddParameter(DBEntityNames.TestCase.Created.ToString(), Enums.ParameterType._datetime, DateTime.Now.ToString());
            _dataModelManager.AddParameter(DBEntityNames.TestCase.CreatedBy.ToString(), Enums.ParameterType._int, Common.GetResponsibleId().ToString());
            _dataModelManager.AddParameter(DBEntityNames.TestCase.Title.ToString(), Enums.ParameterType._string, model.title);
            _dataModelManager.AddParameter(DBEntityNames.TestCase.Description.ToString(), Enums.ParameterType._string, model.desc);
            _dataModelManager.AddParameter(DBEntityNames.TestCase.Guid.ToString(), Enums.ParameterType._string, Guid.NewGuid().ToString());
            _dataModelManager.AddParameter(DBEntityNames.TestCase.ValidFrom.ToString(), Enums.ParameterType._datetime, model.validFrom);
            _dataModelManager.AddParameter(DBEntityNames.TestCase.ValidTo.ToString(), Enums.ParameterType._datetime, model.validTo);
            _dataModelManager.AddParameter(DBEntityNames.TestCase.DCRGraphId.ToString(), Enums.ParameterType._int, model.graphId);
            _dataModelManager.AddParameter(DBEntityNames.TestCase.Delay.ToString(), Enums.ParameterType._int, model.delay);
            _dataModelManager.AddParameter(DBEntityNames.TestCase.RoleToTest.ToString(), Enums.ParameterType._string, model.roles);
            _dataModelManager.AddParameter(DBEntityNames.TestCase.Page.ToString(), Enums.ParameterType._int, Convert.ToString(0));
            // Returns child id
            return _manager.InsertData(_dataModelManager.DataModel).Rows[0][DBEntityNames.TestCase.Id.ToString()].ToString();
        }

        /// <summary>
        /// Add Child 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string AddTestCaseInstance(AddTestCaseInstance model)
        {
            var testCaseId = new DataTable();
            var instanceId = string.Empty;
            if (model.guid != null)
            {
                _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.TestCase.ToString());
                _dataModelManager.AddResultSet(new List<string> { DBEntityNames.TestCase.Id.ToString(), DBEntityNames.TestCase.RoleToTest.ToString(), DBEntityNames.TestCase.DCRGraphId.ToString(), DBEntityNames.TestCase.Title.ToString(), DBEntityNames.TestCase.CreatedBy.ToString() });
                _dataModelManager.AddFilter(DBEntityNames.TestCase.Guid.ToString(), Enums.ParameterType._guid, model.guid.ToString(), Enums.CompareOperator.equal, Enums.LogicalOperator.none);

                testCaseId =_manager.SelectData(_dataModelManager.DataModel);

                var responsible = int.Parse(Common.GetResponsibleId());
                var roles = testCaseId.Rows[0]["RoleToTest"].ToString().Split(',');
                var rolesList = new List<UserRole>();
                foreach (var item in roles)
                {
                    var userRole = new UserRole()
                    {
                        RoleId = item,
                        UserId = responsible
                    };
                    rolesList.Add(userRole);
                }
                var graphId = (int)testCaseId.Rows[0]["DCRGraphId"];
                instanceId = AddInstance(new AddInstanceModel()
                {
                    Title = testCaseId.Rows[0]["Title"].ToString(),
                    GraphId = graphId,
                    Responsible = (int)testCaseId.Rows[0]["CreatedBy"],
                    UserRoles = rolesList
                });

                var json = @"{""instanceId"":" + instanceId + @",""graphId"":" + graphId + "}";
                dynamic graphInput = JObject.Parse(json);


                var result = _servicesController.InitializeGraph(graphInput, out string output);
                if (!result)
                {
                    Common.LogInfo(_manager, _dataModelManager, "AddTestCaseInstance - InitializeGraph - Failed. - " + Common.ToJson(model));
                    Exception ex = new Exception();
                    Common.LogError(ex);
                }
            }

            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.INSERT, DBEntityNames.Tables.TestCaseInstance.ToString());
            _dataModelManager.AddParameter(DBEntityNames.TestCaseInstance.Created.ToString(), Enums.ParameterType._datetime, DateTime.Now.ToString());
            _dataModelManager.AddParameter(DBEntityNames.TestCaseInstance.Email.ToString(), Enums.ParameterType._string, model.email);
            _dataModelManager.AddParameter(DBEntityNames.TestCaseInstance.Name.ToString(), Enums.ParameterType._string, model.name);
            _dataModelManager.AddParameter(DBEntityNames.TestCaseInstance.TestCaseId.ToString(), Enums.ParameterType._int, testCaseId.Rows[0]["Id"].ToString());
            _dataModelManager.AddParameter(DBEntityNames.TestCaseInstance.InstanceId.ToString(), Enums.ParameterType._int, instanceId);
           
            _manager.InsertData(_dataModelManager.DataModel).Rows[0][DBEntityNames.TestCaseInstance.Id.ToString()].ToString();
            // Returns Instace id
            return instanceId;
        }

        /// <summary>
        /// Update a document
        /// </summary>
        /// <param name="documentName"></param>
        /// <param name="type"></param>
        /// <param name="link"></param>
        private void UpdateTestCase(AddTestCase model)
        {
            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.UPDATE, DBEntityNames.Tables.TestCase.ToString());
            _dataModelManager.AddParameter(DBEntityNames.TestCase.Modified.ToString(), Enums.ParameterType._datetime, DateTime.Now.ToString());
            _dataModelManager.AddParameter(DBEntityNames.TestCase.ModifiedBy.ToString(), Enums.ParameterType._int, Common.GetResponsibleId().ToString());
            if (!string.IsNullOrEmpty(model.title))
                _dataModelManager.AddParameter(DBEntityNames.TestCase.Title.ToString(), Enums.ParameterType._string, model.title);
            if (!string.IsNullOrEmpty(model.desc))
                _dataModelManager.AddParameter(DBEntityNames.TestCase.Description.ToString(), Enums.ParameterType._string, model.desc);
            if (!string.IsNullOrEmpty(model.validFrom))
                _dataModelManager.AddParameter(DBEntityNames.TestCase.ValidFrom.ToString(), Enums.ParameterType._datetime, model.validFrom);
            if (!string.IsNullOrEmpty(model.validTo))
                _dataModelManager.AddParameter(DBEntityNames.TestCase.ValidTo.ToString(), Enums.ParameterType._datetime, model.validTo);
            if (!string.IsNullOrEmpty(model.graphId))
                _dataModelManager.AddParameter(DBEntityNames.TestCase.DCRGraphId.ToString(), Enums.ParameterType._int, model.graphId);
            if (!string.IsNullOrEmpty(model.delay))
                _dataModelManager.AddParameter(DBEntityNames.TestCase.Delay.ToString(), Enums.ParameterType._int, model.delay);
            if (!string.IsNullOrEmpty(model.roles))
                _dataModelManager.AddParameter(DBEntityNames.TestCase.RoleToTest.ToString(), Enums.ParameterType._string, model.roles);

            _dataModelManager.AddFilter(DBEntityNames.TestCase.Id.ToString(), Enums.ParameterType._int, model.id, Enums.CompareOperator.equal, Enums.LogicalOperator.none);
            _manager.UpdateData(_dataModelManager.DataModel);
        }

        
        /// <summary>
        /// Return Instance Phases
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private DataTable GetInstancePhases(string id)
        {
            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.InstancePhases.ToString());
            _dataModelManager.AddResultSet(new List<string> { DBEntityNames.InstancePhases.Title.ToString(), DBEntityNames.InstancePhases.PhaseId.ToString(), DBEntityNames.InstancePhases.CurrentPhase.ToString() });
            _dataModelManager.AddFilter(DBEntityNames.InstancePhases.InstanceId.ToString(), Enums.ParameterType._int, id, Enums.CompareOperator.equal, Enums.LogicalOperator.none);
            _dataModelManager.AddOrderBy(DBEntityNames.InstancePhases.SequenceNumber.ToString(), false);

            return _manager.SelectData(_dataModelManager.DataModel);
        }

        /// <summary>
        /// Return Instance Phases
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private DataTable GetWarningDelay(string id)
        {
            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.Instance.ToString());
            _dataModelManager.AddResultSet(new List<string> { DBEntityNames.Instance.Id.ToString(), DBEntityNames.Instance.NextDelay.ToString(), Convert.ToString("Datediff(millisecond, getUTCDate(), " + DBEntityNames.Instance.NextDelay.ToString() + ") as DIFF"), "getUTCDate() as UTC" });
            _dataModelManager.AddFilter(DBEntityNames.Instance.Id.ToString(), Enums.ParameterType._int, id, Enums.CompareOperator.equal, Enums.LogicalOperator.and);

            return _manager.SelectData(_dataModelManager.DataModel);
        }

        /// <summary> 
        /// Return AI Robotic Events
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private DataTable GetAIRoboticEvents(string id)
        {
            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.InstanceAIRoboticEvents.ToString());
            _dataModelManager.AddResultSet(new List<string> { DBEntityNames.InstanceAutomaticEvents.EventId.ToString(), DBEntityNames.InstanceAutomaticEvents.EventTitle.ToString(), DBEntityNames.InstanceAutomaticEvents.EventOpen.ToString(), DBEntityNames.InstanceAutomaticEvents.IsEnabled.ToString(), DBEntityNames.InstanceAutomaticEvents.IsIncluded.ToString(), DBEntityNames.InstanceAutomaticEvents.IsExecuted.ToString(), DBEntityNames.InstanceAutomaticEvents.EventType.ToString(), DBEntityNames.InstanceAutomaticEvents.InstanceId.ToString(), DBEntityNames.InstanceAutomaticEvents.Responsible.ToString(), DBEntityNames.InstanceAutomaticEvents.EventTypeData.ToString(), DBEntityNames.InstanceAutomaticEvents.Modified.ToString(), DBEntityNames.InstanceAutomaticEvents.SimulationId.ToString(), DBEntityNames.InstanceAutomaticEvents.GraphId.ToString(), DBEntityNames.InstanceAutomaticEvents.TrueEventId.ToString(), DBEntityNames.InstanceAutomaticEvents.Description.ToString() });
            _dataModelManager.AddFilter(DBEntityNames.InstanceAutomaticEvents.InstanceId.ToString(), Enums.ParameterType._int, id, Enums.CompareOperator.equal, Enums.LogicalOperator.none);

            return _manager.SelectData(_dataModelManager.DataModel);
        }

        /// <summary>
        /// Add instance
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private string AddInstance(AddInstanceModel input)
        {
            try
            {
                // add Instance
                var instanceId = Common.AddInstance(input, _manager, _dataModelManager);
                if (input.ChildId != null)
                {
                    if (!string.IsNullOrEmpty(input.CaseId))
                        LinkCaseToInstance(input.CaseId, input.CaseNumberIdentifier, instanceId);
                    ConnectInstanceToChild(instanceId, input.ChildId);
                }
                return instanceId;
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "AddInstance - Failed. - " + Common.ToJson(input));
                Common.LogError(ex);
                return "";
            }
        }

        /// <summary>
        /// Return Tasks
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private DataTable GetTasks(string id)
        {
            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SP, Convert.ToString(DBEntityNames.StoredProcedures.InstanceTasksAI.ToString()));
            _dataModelManager.AddParameter(DBEntityNames.Event.Responsible.ToString(), Enums.ParameterType._int, Common.GetResponsibleId());
            _dataModelManager.AddParameter(DBEntityNames.Event.InstanceId.ToString(), Enums.ParameterType._int, id);
            return _manager.ExecuteStoredProcedure(_dataModelManager.DataModel);
        }

        /// <summary>
        /// Add Child Name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        private void AddChildName(string name, string caseNumber, string id)
        {
            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.INSERT, DBEntityNames.Tables.StamdataChild.ToString());
            _dataModelManager.AddParameter(DBEntityNames.StamdataChild.ChildId.ToString(), Enums.ParameterType._string, id);
            _dataModelManager.AddParameter(DBEntityNames.StamdataChild.Navn.ToString(), Enums.ParameterType._string, name);
            _dataModelManager.AddParameter(DBEntityNames.StamdataChild.Sagsnummer.ToString(), Enums.ParameterType._string, caseNumber);
            _manager.InsertData(_dataModelManager.DataModel).Rows[0][DBEntityNames.Child.Id.ToString()].ToString();
        }

        /// <summary>
        /// Update instance after event Log
        /// </summary>
        /// <param name="instanceId"></param>
        private void UpdateEventLogInstance(string instanceId, string xml)
        {
            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SP, DBEntityNames.StoredProcedures.UpdateEventLogInstance.ToString());
            _dataModelManager.AddParameter(DBEntityNames.UpdateEventLogInstance.instanceId.ToString(), Enums.ParameterType._int, instanceId);
            _dataModelManager.AddParameter(DBEntityNames.UpdateEventLogInstance.xml.ToString(), Enums.ParameterType._xml, xml);

            _manager.ExecuteStoredProcedure(_dataModelManager.DataModel);
        }

        /// <summary>
        /// Update process and phases
        /// </summary>
        /// <param name="processId"></param>
        /// <param name="graphId"></param>
        private void UpdateProcessAndPhases(string processId, string graphId, string title = "")
        {
            var graphXml = _dCRService.GetProcess(graphId);

            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.UPDATE, DBEntityNames.Tables.Process.ToString());
            _dataModelManager.AddParameter(DBEntityNames.Process.DCRXML.ToString(), Enums.ParameterType._xml, graphXml);
            _dataModelManager.AddParameter(DBEntityNames.Process.Modified.ToString(), Enums.ParameterType._datetime, DateTime.Now.ToString());
            _dataModelManager.AddParameter(DBEntityNames.Process.Status.ToString(), Enums.ParameterType._boolean, Boolean.TrueString);
            if (!string.IsNullOrEmpty(title))
            {
                _dataModelManager.AddParameter(DBEntityNames.Process.Title.ToString(), Enums.ParameterType._string, title);
            }
            _dataModelManager.AddFilter(DBEntityNames.Process.GraphId.ToString(), Enums.ParameterType._int, graphId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);

            _manager.UpdateData(_dataModelManager.DataModel);

            var phases = _dCRService.GetPhases(graphXml);

            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SP, DBEntityNames.StoredProcedures.AddProcessPhases.ToString());
            _dataModelManager.AddParameter(DBEntityNames.AddProcessPhases.ProcessId.ToString(), Enums.ParameterType._int, processId);
            _dataModelManager.AddParameter(DBEntityNames.AddProcessPhases.PhaseXml.ToString(), Enums.ParameterType._xml, phases);

            _manager.ExecuteStoredProcedure(_dataModelManager.DataModel);
        }

        /// <summary>
        /// Update a document
        /// </summary>
        /// <param name="documentName"></param>
        /// <param name="type"></param>
        /// <param name="link"></param>
        private void UpdateDocument(string id, string documentName, string link, string isDraft)
        {
            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.UPDATE, DBEntityNames.Tables.Document.ToString());
            _dataModelManager.AddParameter(DBEntityNames.Document.Title.ToString(), Enums.ParameterType._string, documentName);
            if (!string.IsNullOrEmpty(link))
            {
                _dataModelManager.AddParameter(DBEntityNames.Document.Link.ToString(), Enums.ParameterType._string, link);
            }
            if (!string.IsNullOrEmpty(isDraft))
            {
                _dataModelManager.AddParameter(DBEntityNames.Document.IsDraft.ToString(), Enums.ParameterType._boolean, isDraft);
            }
            _dataModelManager.AddFilter(DBEntityNames.Document.Id.ToString(), Enums.ParameterType._int, id, Enums.CompareOperator.equal, Enums.LogicalOperator.none);
            _manager.UpdateData(_dataModelManager.DataModel);
        }

        /// <summary>
        /// Update Journal History
        /// </summary>
        /// <param name="id"></param>
        /// <param name="documentName"></param>
        /// <param name="eventTime"></param>
        private void UpdateJournalHistoryDocument(string id, string documentName, DateTime eventTime)
        {
            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.UPDATE, DBEntityNames.Tables.JournalHistory.ToString());
            _dataModelManager.AddParameter(DBEntityNames.JournalHistory.Title.ToString(), Enums.ParameterType._string, documentName);
            _dataModelManager.AddParameter(DBEntityNames.JournalHistory.EventDate.ToString(), Enums.ParameterType._datetime, eventTime.ToString());
            _dataModelManager.AddFilter(DBEntityNames.JournalHistory.DocumentId.ToString(), Enums.ParameterType._int, id, Enums.CompareOperator.equal, Enums.LogicalOperator.none);
            _manager.UpdateData(_dataModelManager.DataModel);
        }

        /// <summary>
        /// Select a document using Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="resultSet"></param>
        /// <returns></returns>
        private DataTable SelectDocumentById(string id, List<string> resultSet)
        {
            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.Document.ToString());
            _dataModelManager.AddResultSet(resultSet);
            _dataModelManager.AddFilter(DBEntityNames.Document.Id.ToString(), Enums.ParameterType._int, id, Enums.CompareOperator.equal, Enums.LogicalOperator.none);

            return _manager.SelectData(_dataModelManager.DataModel);
        }

        /// <summary>
        /// Select a document using Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="resultSet"></param>
        /// <returns></returns>
        private DataTable SelectPersonDocumentByPerson(List<string> resultSet)
        {
            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.Document.ToString());
            _dataModelManager.AddResultSet(resultSet);
            _dataModelManager.AddFilter(DBEntityNames.Document.Type.ToString(), Enums.ParameterType._string, "Personal", Enums.CompareOperator.equal, Enums.LogicalOperator.and);
            _dataModelManager.AddFilter(DBEntityNames.Document.Responsible.ToString(), Enums.ParameterType._string, Common.GetCurrentUserName(), Enums.CompareOperator.equal, Enums.LogicalOperator.and);
            _dataModelManager.AddFilter(DBEntityNames.Document.IsActive.ToString(), Enums.ParameterType._boolean, bool.TrueString, Enums.CompareOperator.equal, Enums.LogicalOperator.none);

            return _manager.SelectData(_dataModelManager.DataModel);
        }

        /// <summary>
        /// Delete a document using Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private void DeleteDocument(string id)
        {
            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.UPDATE, DBEntityNames.Tables.Document.ToString());
            _dataModelManager.AddParameter(DBEntityNames.Document.IsActive.ToString(), Enums.ParameterType._boolean, bool.FalseString);
            _dataModelManager.AddFilter(DBEntityNames.Document.Id.ToString(), Enums.ParameterType._int, id, Enums.CompareOperator.equal, Enums.LogicalOperator.none);
            _manager.UpdateData(_dataModelManager.DataModel);
        }

        /// <summary>
        /// Delete a file from file system
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        private bool DeleteFileFromFileSystem(string id, string type, string instanceId)
        {
            // select document from Db
            DataTable document = SelectDocumentById(id, new List<string>() { DBEntityNames.Document.Link.ToString() });
            if (document.Rows.Count > 0)
            {
                var path = string.Empty;
                // delete document from file system
                switch (type)
                {
                    case "PersonalDocument":
                        var currentUser = Common.GetCurrentUserName();
                        path = Configurations.Config.PersonalFileLocation + "\\" + currentUser + "\\" + document.Rows[0]["Link"].ToString();
                        break;
                    case "InstanceDocument":
                        path = Configurations.Config.InstanceFileLocation + "\\" + instanceId + "\\" + document.Rows[0]["Link"].ToString();
                        break;
                    case "JournalNoteBig":
                        path = Configurations.Config.JournalNoteFileLocation + "\\" + instanceId + "\\" + document.Rows[0]["Link"].ToString();
                        break;
                }

                var fileInfo = new FileInfo(path);
                if (fileInfo.Exists)
                {
                    try
                    {
                        fileInfo.Delete();
                    }
                    catch (Exception ex)
                    {
                        Common.LogInfo(_manager, _dataModelManager, "DeleteFileFromFileSystem - Failed. - instanceId : " + instanceId + ", type : " + type + ", documentid : " + id);
                        Common.LogError(ex);
                        throw ex;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Connect an instance to a child
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="childId"></param>
        private void ConnectInstanceToChild(string instanceId, int? childId)
        {
            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.INSERT, DBEntityNames.Tables.InstanceExtension.ToString());
            _dataModelManager.AddParameter(DBEntityNames.InstanceExtension.InstanceId.ToString(), Enums.ParameterType._int, instanceId);
            _dataModelManager.AddParameter(DBEntityNames.InstanceExtension.ChildId.ToString(), Enums.ParameterType._int, childId.ToString());

            _manager.InsertData(_dataModelManager.DataModel);
        }

        /// <summary>
        /// Link Acadre to Instance
        /// </summary>
        /// <param name="childId"></param>
        /// <param name="caseNumberIdentifier"></param>
        private void LinkCaseToInstance(string caseId, string caseNumberIdentifier, string instanceId)
        {
            Common.LogInfo(_manager, _dataModelManager, "GetCaseURL(" + caseId + " )");
            string caseLink = Common.GetCaseLink(caseId);
            if (string.IsNullOrWhiteSpace(caseNumberIdentifier))
            {
                Common.LogInfo(_manager, _dataModelManager, "GetCaseNumber(" + caseId + " )");
                caseNumberIdentifier = Common.GetCaseIdForeign(caseId);
            }

            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.UPDATE, DBEntityNames.Tables.Instance.ToString());
            _dataModelManager.AddParameter(DBEntityNames.Instance.InternalCaseID.ToString(), Enums.ParameterType._int, caseId);
            _dataModelManager.AddParameter(DBEntityNames.Instance.CaseNoForeign.ToString(), Enums.ParameterType._string, caseNumberIdentifier);
            _dataModelManager.AddParameter(DBEntityNames.Instance.CaseLink.ToString(), Enums.ParameterType._string, caseLink);
            _dataModelManager.AddFilter(DBEntityNames.Instance.Id.ToString(), Enums.ParameterType._int, instanceId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);
            _manager.UpdateData(_dataModelManager.DataModel);
        }
    }
}