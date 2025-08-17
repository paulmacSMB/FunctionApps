using System.Net;
using System.Net.Http.Headers;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;

public class SummarizeProxy
{
    private readonly IConfiguration _config;
    private static readonly HttpClient httpClient = new HttpClient();

    public SummarizeProxy(IConfiguration config)
    {
        _config = config;
    }

    [Function("SummarizeProxy")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
        var targetUrl = _config["SECURE_SUMMARIZE_ENDPOINT"];
        var functionKey = _config["SECURE_SUMMARIZE_API_KEY"];

        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var forwardRequest = new HttpRequestMessage(HttpMethod.Post, targetUrl);
        forwardRequest.Content = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");
        forwardRequest.Headers.Add("x-functions-key", functionKey); 

        var response = await httpClient.SendAsync(forwardRequest);

        var proxyResponse = req.CreateResponse(response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        await proxyResponse.WriteStringAsync(content);
        return proxyResponse;
    }
}
