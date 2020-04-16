using NLog;
using OpenCaseManager.Commons;
using OpenCaseManager.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;

namespace OpenCaseManager.App_Start
{
    public class UnhandledExceptionLogger : ExceptionLogger
    {
        public override void Log(ExceptionLoggerContext context)
        {
            Common.LogError(context.Exception);
        }
    }

    public class GlobalExceptionHandler : ExceptionHandler
    {
        public override void Handle(ExceptionHandlerContext context)
        {
            if (context.Exception is ArgumentNullException)
            {
                var result = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(context.Exception.Message),
                    ReasonPhrase = "ArgumentNullException"
                };

                context.Result = new ArgumentNullResult(context.Request, result);
            }
            else
            {
                var result = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(context.Exception.Message),
                    ReasonPhrase = "Exception"
                };

                context.Result = new ExceptionResult(context.Request, result);
            }
        }

        public class ArgumentNullResult : IHttpActionResult
        {
            private HttpRequestMessage _request;
            private HttpResponseMessage _httpResponseMessage;


            public ArgumentNullResult(HttpRequestMessage request, HttpResponseMessage httpResponseMessage)
            {
                _request = request;
                _httpResponseMessage = httpResponseMessage;
            }

            public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
            {
                return Task.FromResult(_httpResponseMessage);
            }
        }


        public class ExceptionResult : IHttpActionResult
        {
            private HttpRequestMessage _request;
            private HttpResponseMessage _httpResponseMessage;


            public ExceptionResult(HttpRequestMessage request, HttpResponseMessage httpResponseMessage)
            {
                _request = request;
                _httpResponseMessage = httpResponseMessage;
            }

            public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
            {
                return Task.FromResult(_httpResponseMessage);
            }
        }
    }
}