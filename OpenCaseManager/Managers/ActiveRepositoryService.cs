using OpenCaseManager.Models;
using RestSharp;
using System.Linq;
using System.Net;

namespace OpenCaseManager.Managers
{
    public class ActiveRepositoryService : IActiveRepositoryService
    {
        private IService _service;

        public ActiveRepositoryService(IService service)
        {
            _service = service;
        }

        /// <summary>
        /// Initialize a graph
        /// </summary>
        /// <param name="graphId"></param>
        /// <returns></returns>
        public string InitializeGraph(string graphId)
        {

            var url = string.Format("/api/graphs/{0}/sims", graphId);
            var serviceModel = GetServiceModel(url, Method.POST);

            var request = _service.GetRequest(serviceModel);
            var response = _service.GetResponse(serviceModel, request);
            var simulationId = response.Headers.ToList().Find(x => x.Name == "X-DCR-simulation-ID").Value.ToString();
            return simulationId;
        }

        /// <summary>
        /// Get Pending or Enabled Events
        /// </summary>
        /// <param name="graphId"></param>
        /// <param name="simulationId"></param>
        /// <returns></returns>
        public string GetPendingOrEnabled(string graphId, string simulationId)
        {
            var url = string.Format("api/graphs/{0}/sims/{1}/events?filter=enabledorpending ", graphId, simulationId);
            var serviceModel = GetServiceModel(url, Method.GET);

            var request = _service.GetRequest(serviceModel);
            var response = _service.GetResponse(serviceModel, request);
            var content = response.Content;
            content = content.Replace("\\r\\n", "").Replace("\\\"", "\"").Replace("\"<events", "<events").Replace("</events>\"", "</events>");
            return content;
        }

        /// <summary>
        /// Execute an event
        /// </summary>
        /// <param name="graphId"></param>
        /// <param name="simulationId"></param>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public string ExecuteEvent(string graphId, string simulationId, string eventId)
        {
            var url = string.Format("api/graphs/{0}/sims/{1}/events/{2} ", graphId, simulationId, eventId);
            var serviceModel = GetServiceModel(url, Method.POST);

            var request = _service.GetRequest(serviceModel);
            var response = _service.GetResponse(serviceModel, request);

            var content = response.Content;
            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return string.Empty;
            }
            return string.Empty;
        }

        /// <summary>
        /// Execute an event
        /// </summary>
        /// <param name="graphId"></param>
        /// <param name="simulationId"></param>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public string ExecuteEventWithTime(string graphId, string simulationId, string eventId, string time)
        {
            var url = string.Format("api/graphs/{0}/sims/{1}/events/{2} ", graphId, simulationId, eventId);
            string body = "<absoluteTime>" + time + "</absoluteTime>";
            var serviceModel = GetServiceModel(url, Method.POST, body);

            var request = _service.GetRequest(serviceModel);
            var obj = new { time = serviceModel.Body };
            request.AddParameter("application/json", SimpleJson.SerializeObject(obj), ParameterType.RequestBody);
            var response = _service.GetResponse(serviceModel, request);

            var content = response.Content;
            return content;
        }

        /// <summary>
        /// Get Process Roles
        /// </summary>
        /// <param name="graphId"></param>
        /// <returns></returns>
        public string GetProcessRoles(string graphId)
        {
            var url = string.Format("api/graphs/{0}?filter=roles ", graphId);
            var serviceModel = GetServiceModel(url, Method.GET);

            var request = _service.GetRequest(serviceModel);
            var response = _service.GetResponse(serviceModel, request);

            var content = response.Content;
            return content;
        }

        /// <summary>
        /// Get Process Phases
        /// </summary>
        /// <param name="graphId"></param>
        /// <returns></returns>
        public string GetProcessPhases(string graphId)
        {
            var url = string.Format("api/graphs/{0}?filter=phases ", graphId);
            var serviceModel = GetServiceModel(url, Method.GET);

            var request = _service.GetRequest(serviceModel);
            var response = _service.GetResponse(serviceModel, request);
            var content = response.Content;
            return content;
        }

        /// <summary>
        /// Search Processes
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public string SearchProcess(string title)
        {
            var url = string.Format("/api/graphs" + title);
            var serviceModel = GetServiceModel(url, Method.GET);

            var request = _service.GetRequest(serviceModel);
            var response = _service.GetResponse(serviceModel, request);
            var content = response.Content;
            content = content.Replace("\\r\\n", "").Replace("\\\"", "\"").Replace("\"<graphs", "<graphs").Replace("</graphs>\"", "</graphs>");
            return content;
        }

        /// <summary>
        /// Get graph details
        /// </summary>
        /// <param name="graphId"></param>
        /// <returns></returns>
        public string GetProcess(string graphId)
        {
            var url = string.Format("/api/graphs/{0}?filter=refer", graphId);
            var serviceModel = GetServiceModel(url, Method.GET);

            var request = _service.GetRequest(serviceModel);
            var response = _service.GetResponse(serviceModel, request);
            var content = response.Content;
            content = content.Replace("\\r\\n", "").Replace("\\\"", "\"").Replace("\"<graphs", "<graphs").Replace("</graphs>\"", "</graphs>");
            return content;
        }

        /// <summary>
        /// Get graph details
        /// </summary>
        /// <param name="graphId"></param>
        /// <returns></returns>
        public string AdvanceTime(string graphId, string simulationId, string time)
        {
            var url = string.Format("api/graphs/{0}/sims/{1}/time/ ", graphId, simulationId, time);
            string body = "<absoluteTime>" + time + "</absoluteTime>";
            var serviceModel = GetServiceModel(url, Method.POST, body);

            var request = _service.GetRequest(serviceModel);
            var obj = new { time = serviceModel.Body };
            request.AddParameter("application/json", SimpleJson.SerializeObject(obj), ParameterType.RequestBody);
            var response = _service.GetResponse(serviceModel, request);

            var content = response.Content;
            return content;
        }

        /// <summary>
        /// Get refer xml from graph xml
        /// </summary>
        /// <param name="graphId"></param>
        /// <param name="simulationId"></param>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public string GetReferXmlByEventId(string graphId, string simulationId, dynamic eventId)
        {
            var url = string.Format("api/graphs/{0}/sims/{1}?filter=refer&eventid={2} ", graphId, simulationId, eventId);
            var serviceModel = GetServiceModel(url, Method.GET);

            var request = _service.GetRequest(serviceModel);
            var response = _service.GetResponse(serviceModel, request);
            var content = response.Content;
            return content;
        }

        /// <summary>
        /// Merge executed xml with graph
        /// </summary>
        /// <param name="graphId"></param>
        /// <param name="simulationId"></param>
        /// <param name="referXml"></param>
        /// <param name="eventId"></param>
        public void MergeReferXmlWithMainXml(string graphId, string simulationId, dynamic referXml, dynamic eventId)
        {
            var url = string.Format("api/graphs/{0}/sims/{1}?filter=refer ", graphId, simulationId);
            var serviceModel = GetServiceModel(url, Method.PUT);

            var request = _service.GetRequest(serviceModel);
            var obj = new { eventid = eventId, referXML = referXml };
            request.AddParameter("application/json", SimpleJson.SerializeObject(obj), ParameterType.RequestBody);
            var response = _service.GetResponse(serviceModel, request);
            var content = response.Content;
        }

        /// <summary>
        /// Get major revisions of a graph
        /// </summary>
        /// <param name="graphId"></param>
        /// <returns></returns>
        public string GetMajorRevisions(string graphId)
        {
            var url = string.Format("/api/graphs/{0}/versions?versions=major", graphId);
            var serviceModel = GetServiceModel(url, Method.GET);

            var request = _service.GetRequest(serviceModel);
            var response = _service.GetResponse(serviceModel, request);
            var content = response.Content;
            return content;
        }

        /// <summary>
        /// Get active respository service model
        /// </summary>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        private ServiceModel GetServiceModel(string url, Method method, string body = "")
        {
            return new ServiceModel()
            {
                BaseUrl = Configurations.Config.DCRActiveRepository,
                Username = Configurations.Config.DCRActiveRepositoryUser,
                Password = Configurations.Config.DCRActiveRepositoryUserPassword,
                Url = url,
                MethodType = method,
                Body = body
            };
        }
    }
}