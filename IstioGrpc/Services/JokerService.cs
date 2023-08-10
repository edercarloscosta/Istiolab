using System.Text.Json;
using Grpc.Core;
using IstioGrpc.Model;
using Polly;
using Prometheus;

namespace IstioGrpc.Services;

public class JokerService : Joker.JokerBase
{
    private readonly ILogger<JokerService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    
    #region Metrics Prometheus
    private readonly Counter _recordsProcessed = Metrics.CreateCounter(
        "server_success_total", "Total number of records processed.");
    
    private readonly Counter _errorCounter = Metrics.CreateCounter(
        "server_errors_total", "Total errors in gRPC server");
    #endregion
    
    public JokerService(
        ILogger<JokerService> logger, 
        IHttpClientFactory httpClientFactory)
        => (_logger, _httpClientFactory) = (logger, httpClientFactory);

    public override async Task<JokeReply> NewJoke(JokeRequest request, ServerCallContext context)
    {
        try
        {
            var retryPolicy = Policy.Handle<HttpRequestException>()
                .Or<RpcException>()
                .WaitAndRetry(5, retryAttempt 
                    => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
            
            _recordsProcessed.Inc();
            
            var httpClient = _httpClientFactory.CreateClient("joker");
            var response = await retryPolicy.Execute(() 
                => httpClient.GetAsync("random_joke"));
            
            if (!response.IsSuccessStatusCode)
            {
                return new JokeReply
                {
                    Setup = "Nothing here for you! Go away!",
                    Punchline = "I said nothing",
                };
            }
            
            var content = await response.Content.ReadAsStringAsync();
            var joke = JsonSerializer.Deserialize<Joke>(
                content,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true
                });
            
            _logger.LogInformation("New Joke processed at: {time}", DateTimeOffset.Now);
            
            return new JokeReply
            {
                Setup = joke?.Setup,
                Punchline = joke?.PunchLine,
            };
        }
        catch (Exception err)
        {
            _logger.LogError(err, "Error on NewJoke {e}", err);
            _errorCounter.Inc();
            throw;
        }
        
    }
}