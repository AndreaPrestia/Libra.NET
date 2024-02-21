using Libra.Net.Configurations;
using Libra.Net.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Security;
using System.Text.Json;
using Libra.Net.Abstractions;

namespace Libra.Net.Middleware;

/// <summary>
/// This is a middleware that uses the LoadBalancingConfiguration to retrieve the correct ILoadBalancingAlgorithm implementation and forward the request to the server found with HttpRequestManager
/// </summary>
[ExcludeFromCodeCoverage]
public class LoadBalancingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly LoadBalancingAlgorithmFactory _loadBalancingAlgorithmFactory;
    private readonly IOptionsMonitor<LoadBalancingConfiguration> _optionsMonitor;
    private readonly ILogger<LoadBalancingMiddleware> _logger;

    public LoadBalancingMiddleware(RequestDelegate next, LoadBalancingAlgorithmFactory loadBalancingAlgorithmFactory, IOptionsMonitor<LoadBalancingConfiguration> optionsMonitor,	
        ILogger<LoadBalancingMiddleware> logger)
    {
        ArgumentNullException.ThrowIfNull(next, nameof(next));
        ArgumentNullException.ThrowIfNull(loadBalancingAlgorithmFactory, nameof(loadBalancingAlgorithmFactory));
        ArgumentNullException.ThrowIfNull(optionsMonitor, nameof(optionsMonitor));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        _next = next;
        _loadBalancingAlgorithmFactory = loadBalancingAlgorithmFactory;
        _optionsMonitor = optionsMonitor;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context, HttpRequestManager httpRequestManager, CancellationToken cancellationToken)
    {
        var url = context.Request.Path.Value!;

        var method = context.Request.Method;

        try
        {
            _logger.LogInformation($"Processing request {url} {method}");

            var loadBalancingPolicy = _optionsMonitor.CurrentValue.LoadBalancingPolicy;
            var loadBalancingPolicyEnumValue = (LoadBalancingPolicy)Enum.Parse(typeof(LoadBalancingPolicy), loadBalancingPolicy);
            var algorithm = _loadBalancingAlgorithmFactory.ResolveByPolicy(loadBalancingPolicyEnumValue);

            if (algorithm == null)
            {
                throw new InvalidOperationException(
                    $"LoadBalancingPolicy {loadBalancingPolicy} not supported. No request will be processed.");
            }

            var server = algorithm.GetNextServer();

            if(server == null)
            {
                throw new InvalidOperationException(
                    $"LoadBalancingPolicy {loadBalancingPolicy} did not found any server available. No request will be processed.");
            }

            _logger.LogInformation($"Forwarding {url} {method} to {server} with policy {loadBalancingPolicy}");

            await httpRequestManager.ForwardRequest(context, server, cancellationToken);

            var releaseConnectionAlgorithm = algorithm as IReleaseConnectionAlgorithm;

            releaseConnectionAlgorithm?.ReleaseConnection(server);

            await _next.Invoke(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);

            var status = ex switch
            {
                ArgumentException => (StatusCodes.Status400BadRequest, "Bad Request"),
                InvalidOperationException => (StatusCodes.Status400BadRequest, "Bad Request"),
                SecurityException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
                UnauthorizedAccessException => (StatusCodes.Status403Forbidden, "Forbidden"),
                EntryPointNotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
                FileNotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
                InvalidConstraintException => (StatusCodes.Status409Conflict, "Conflict"),
                _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
            };

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = status.Item1;

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(new ProblemDetails()
                {
                    Type = $"https://httpstatuses.io/{status.Item1}",
                    Detail = ex.Message,
                    Status = status.Item1,
                    Title = status.Item2,
                    Instance = $"{url}",
                }));
        }
    }
}