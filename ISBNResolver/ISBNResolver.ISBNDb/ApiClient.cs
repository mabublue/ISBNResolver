using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ISBNResolver.ISBNDb
{
    public interface IApiClient
    {
        Task<string> CallApiForBookByISBN(string ISBN, CancellationToken cancellationToken);
    }

    public class ApiClient : IApiClient
    {
        const string baseUrl = "https://api2.isbndb.com/book/";
        const string rest_key = "45172_4504ca56cbf7f60e828af87825a5758c";
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiClient> _logger;

        public ApiClient(ILogger<ApiClient> logger)
        {
            _httpClient = new HttpClient();
            _logger = logger;
        }


        public async Task<string> CallApiForBookByISBN(string ISBN, CancellationToken cancellationToken)
        {
            var url = new Uri($"{baseUrl}{ISBN}");

            var apiResponse = await DoPollyHttpRequest(() => _httpClient.SendAsync(CreateRequest(url), cancellationToken));
            if (apiResponse.StatusCode != HttpStatusCode.OK)
                throw new Exception($"Error in Api Response: Status Code = {apiResponse.StatusCode}, Content = {apiResponse.Content}");

            return await apiResponse.Content.ReadAsStringAsync();
        }

        private static HttpRequestMessage CreateRequest(Uri uri)
        {
            var req = new HttpRequestMessage(HttpMethod.Get, uri);
            req.Headers.Add("Authorization", rest_key);
            return req;
        }

        private async Task<HttpResponseMessage> DoPollyHttpRequest(Func<Task<HttpResponseMessage>> functionToExecute)
        {
            var apiResponse = await Policy.HandleResult<HttpResponseMessage>(r => r.StatusCode != HttpStatusCode.OK && r.StatusCode != HttpStatusCode.Unauthorized && r.StatusCode != HttpStatusCode.ServiceUnavailable)
                                          .WaitAndRetryAsync(new[] { TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(8) },
                                                             (result, timeSpan, retryCount, context) =>
                                                                 _logger
                                                                     .LogWarning("Request failed with {UrlStatusCode}. Waiting {timeSpan} before trying again. Retry attempt {retryCount}",
                                                                              result.Result.StatusCode,
                                                                              timeSpan,
                                                                              retryCount)).ExecuteAsync(functionToExecute);

            return apiResponse;
        }
    }
}
