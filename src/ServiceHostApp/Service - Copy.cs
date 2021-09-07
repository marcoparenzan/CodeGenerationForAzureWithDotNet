//using Contract;
//using Microsoft.Azure.Functions.Worker;
//using Microsoft.Azure.Functions.Worker.Http;
//using Microsoft.Extensions.Logging;
//using System.Threading.Tasks;

//namespace ServiceHostApp;

//public class Service
//{
//    private IService service;

//    public Service(IService service)
//    {
//        this.service = service;
//    }

//    [Function("DoSomething")]
//    public async Task<HttpResponseData> DoSomething([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData requestData, ILogger log)
//    {
//        var request = await requestData.ReadFromJsonAsync<DoRequest>();

//        var value = this.service.DoSomething(request.a, request.b);

//        var response = new DoResponse
//        {
//            value = value
//        };

//        HttpResponseData responseData = requestData.CreateResponse(System.Net.HttpStatusCode.OK);
//        await responseData.WriteAsJsonAsync(response);
//        return responseData;
//    }
//}

//public class DoRequest
//{
//    public string a { get; set; }
//    public int b { get; set; }
//}

//public class DoResponse
//{
//    public string value { get; set; }
//}
