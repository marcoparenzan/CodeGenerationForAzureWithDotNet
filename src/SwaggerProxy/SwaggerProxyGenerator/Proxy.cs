using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SwaggerProxyGenerator
{
    public abstract class ProxyBase
    {
        private HttpClient httpClient = new HttpClient();

        protected TResponse Invoke<TRequest,TResponse>(string uri, TRequest request)
        {
            var requestJson = JsonSerializer.Serialize(request);
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
            requestMessage.Content = new StringContent(requestJson);
            requestMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var responseMessage = httpClient.Send(requestMessage);
            var responseJson = responseMessage.Content.ReadAsStringAsync().Result;
            return JsonSerializer.Deserialize<TResponse>(responseJson);
        }
    }
}
