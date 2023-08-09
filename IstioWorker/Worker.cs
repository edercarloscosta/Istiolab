using Grpc.Net.Client;
using IstioGrpc;
using Polly;

namespace IstioWorker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger) => _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var retryPolicy = Policy.Handle<HttpRequestException>()
            .Or<Grpc.Core.RpcException>()
            .WaitAndRetry(5, retryAttempt 
                => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
            
        using var channel = GrpcChannel.ForAddress("http://istiogrpc:80");
        var client = retryPolicy.Execute(() => new Joker.JokerClient(channel));
        
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        
            var reply = await client.NewJokeAsync(new JokeRequest());

            Console.WriteLine($"Setup: {reply.Setup}");
            await Task.Delay(3000, stoppingToken);
            Console.WriteLine($"Punchline: {reply.Punchline}\n");

            await Task.Delay(20000, stoppingToken);
        }
    }
}