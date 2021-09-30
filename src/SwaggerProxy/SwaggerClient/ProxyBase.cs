using System.Text.Json;

public abstract class ProxyBase
{
    private HttpClient httpClient = new HttpClient
    {
        BaseAddress = new Uri("https://localhost:5001")
    };

    protected TResponse Invoke<TRequest,TResponse>(string uri, TRequest request)
    {
        var requestJson = JsonSerializer.Serialize(request);
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, uri);
        requestMessage.Content = new StringContent(requestJson);
        requestMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
        var responseMessage = httpClient.Send(requestMessage);
        var responseJson = responseMessage.Content.ReadAsStringAsync().Result;
        return JsonSerializer.Deserialize<TResponse>(responseJson);
    }
}
