using Grpc.Net.Client;

namespace IstioWorker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var channel = GrpcChannel.ForAddress("http://grpc-server-service:80");
        var client = new Greeter.GreeterClient(channel);

        var counter = 1;
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            var reply = await client.SayHelloAsync(new HelloRequest
            {
                Name = $"Worker {counter}"
            });
            counter++;
            
            _logger.LogInformation($"Response: {reply.Message}");
            await Task.Delay(20000, stoppingToken);
        }
    }
}