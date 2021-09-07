using Contract;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ServiceHostApp;

public class DoRequest
{
    public string a { get; set; }
    public int b { get; set; }
}

public class DoResponse
{
    public string value { get; set; }
}

public partial class Service
{
    [Function("DoSomething")]
    public async Task<HttpResponseData> DoSomething([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData requestData)
    {
        var request = await Request<DoRequest>(requestData);

        var value = this.service.DoSomething(request.a, request.b);

        var response = new DoResponse
        {
            value = value
        };
        return await Response(requestData, response);
    }
}
