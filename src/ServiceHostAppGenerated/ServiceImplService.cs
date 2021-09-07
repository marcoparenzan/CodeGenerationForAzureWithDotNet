using Contract;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ServiceLibrary;
using System.Threading.Tasks;

namespace ServiceHostAppGenerated;

public partial class ServiceImplService
{
    private ServiceImpl service;

    public ServiceImplService(ServiceImpl service)
    {
        this.service = service;
    }

    protected async Task<TRequest> Request<TRequest>(HttpRequestData requestData)
    {
        return await requestData.ReadFromJsonAsync<TRequest>();
    }

    protected async Task<HttpResponseData> Response<TResponse>(HttpRequestData requestData, TResponse response)
    {
        HttpResponseData responseData = requestData.CreateResponse(System.Net.HttpStatusCode.OK);
        await responseData.WriteAsJsonAsync(response);
        return responseData;
    }
}
