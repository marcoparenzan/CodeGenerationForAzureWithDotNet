using Contract;
using Microsoft.Azure.Functions.Worker.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHostApp
{
    public abstract class ServiceBase<TService>
    {
        protected TService service;

        public ServiceBase(TService service)
        {
            this.service = service;
        }

        protected async Task<TRequest> Request<TRequest>(HttpRequestData requestData)
        {
            return await requestData.ReadFromJsonAsync<TRequest>();
        }

        protected async Task<HttpResponseData> Response(HttpRequestData requestData, DoResponse response)
        {
            HttpResponseData responseData = requestData.CreateResponse(System.Net.HttpStatusCode.OK);
            await responseData.WriteAsJsonAsync(response);
            return responseData;
        }
    }
}
