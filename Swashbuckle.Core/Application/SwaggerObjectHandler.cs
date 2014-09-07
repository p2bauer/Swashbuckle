﻿using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Net.Http.Formatting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using Swashbuckle.Swagger2;

namespace Swashbuckle.Application
{
    public class SwaggerObjectHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            object apiVersion;
            request.GetRouteData().Values.TryGetValue("apiVersion", out apiVersion);

            var swaggerObject = request.SwaggerProvider().GetSwaggerFor(apiVersion.ToString());

            var content = ContentFor(request, swaggerObject);
            return TaskFor(new HttpResponseMessage { Content = content });
        }

        private HttpContent ContentFor<T>(HttpRequestMessage request, T swaggerObject)
        {
            var negotiator = request.GetConfiguration().Services.GetContentNegotiator();
            var result = negotiator.Negotiate(typeof(T), request, GetSupportedSwaggerFormatters());

            return new ObjectContent(typeof(T), swaggerObject, result.Formatter, result.MediaType);
        }

        private IEnumerable<MediaTypeFormatter> GetSupportedSwaggerFormatters()
        {
            var jsonFormatter = new JsonMediaTypeFormatter
            {
                SerializerSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }
            };
            return new[] { jsonFormatter };
        }

        private Task<HttpResponseMessage> TaskFor(HttpResponseMessage response)
        {
            var tsc = new TaskCompletionSource<HttpResponseMessage>();
            tsc.SetResult(response);
            return tsc.Task;
        }
    }
}