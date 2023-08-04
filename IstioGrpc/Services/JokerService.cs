using System.Text.Json;
using Grpc.Core;
using IstioGrpc.Model;

namespace IstioGrpc.Services;

public class JokerService : Joker.JokerBase
{
    private readonly ILogger<JokerService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    
    public JokerService(
        ILogger<JokerService> logger, 
        IHttpClientFactory httpClientFactory)
        => (_logger, _httpClientFactory) = (logger, httpClientFactory);

    public override async Task<JokeReply> NewJoke(JokeRequest request, ServerCallContext context)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("joker");
            var response = await httpClient.GetAsync("random_joke");
            
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
            throw;
        }
        
    }
}