using IstioGrpc.Services;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

builder.Services.AddGrpc();
builder.Services.AddHttpClient("joker", client =>
    {
        client.BaseAddress = new Uri("https://official-joke-api.appspot.com/");
        client.DefaultRequestHeaders.Add("Accept", "application/json");
    })
    .AddTransientHttpErrorPolicy(policyBuilder
        => policyBuilder.WaitAndRetryAsync(
            Backoff.DecorrelatedJitterBackoffV2(
                TimeSpan.FromSeconds(1), 
                5)));

var app = builder.Build();

// Enable routing, which is necessary to both:
// 1) capture metadata about gRPC requests, to add to the labels.
// 2) expose /metrics in the same pipeline.
app.UseRouting();

// Capture metrics about received gRPC requests.
app.UseGrpcMetrics();

// Capture metrics about received HTTP requests.
app.UseHttpMetrics();

app.UseEndpoints(endpoints =>
{
    // Configure the HTTP request pipeline.
    app.MapGrpcService<JokerService>();
    
    // Metrics published in this sample:
    // * built-in process metrics giving basic information about the .NET runtime (enabled by default)
    // * metrics from .NET Event Counters (enabled by default, updated every 10 seconds)
    // * metrics from .NET Meters (enabled by default)
    // * metrics about HTTP requests handled by the web app (configured above)
    // * metrics about gRPC requests handled by the web app (configured above)
    app.MapMetrics();
});

app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();