using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvanceTimeWindowsService
{
    public static class Common
    {
        /// <summary>
        /// Call to a rest service
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static IRestResponse ExecuteServiceUsingWindowsLogin(string baseUrl, string resource, Method method)
        {
            var client = new RestClient
            {
                BaseUrl = new Uri(baseUrl),
            };

            var request = new RestRequest
            {
                Resource = resource,
                Method = method,
                UseDefaultCredentials = true
            };

            IRestResponse response = client.Execute(request);
            if (response.StatusCode >= System.Net.HttpStatusCode.BadRequest)
                throw new Exception(response.Content);
            return response;
        }
    }
}
