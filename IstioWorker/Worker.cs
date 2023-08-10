using System.Diagnostics;
using Grpc.Net.Client;
using IstioGrpc;
using Polly;
using Prometheus;

namespace IstioWorker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    
    #region Metrics Prometheus
    private readonly Counter _recordsProcessed = Metrics.CreateCounter(
        "worker_success_total", "Total number of records processed.");
    
    private readonly Counter _errorCounter = Metrics.CreateCounter(
        "worker_errors_total", "Total errors in gRPC worker");
    #endregion
    public Worker(ILogger<Worker> logger) => _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var retryPolicy = Policy.Handle<HttpRequestException>()
                .Or<Grpc.Core.RpcException>()
                .WaitAndRetry(5, retryAttempt 
                    => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        
            var stopwatch = new Stopwatch();
        
            using var channel = GrpcChannel.ForAddress("http://istiogrpc:80");
            var client = retryPolicy.Execute(() => new Joker.JokerClient(channel));
        
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        
                // Start the stopwatch before making the gRPC call
                stopwatch.Start();
            
                _recordsProcessed.Inc();
            
                var reply = await client.NewJokeAsync(new JokeRequest());
            
                // Stop the stopwatch after the gRPC call
                stopwatch.Stop();
            
                Console.WriteLine($"Setup: {reply.Setup}");
                await Task.Delay(3000, stoppingToken);
                Console.WriteLine($"Punchline: {reply.Punchline}");
            
                // Print the elapsed time for the gRPC call
                Console.WriteLine($"Time taken for gRPC call: {stopwatch.Elapsed}\n");

                // Reset the stopwatch for the next iteration
                stopwatch.Reset();
            
                await Task.Delay(20000, stoppingToken);
            }

        }
        catch (Exception err)
        {
            _logger.LogError(err, "Error on ExecuteAsync {e}", err);
            _errorCounter.Inc();
            throw;
        }
    }
}