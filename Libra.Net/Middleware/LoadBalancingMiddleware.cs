using Libra.Net.Configurations;
using Libra.Net.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;
using System.Security;
using System.Text.Json;

namespace Libra.Net.Middleware
{
	internal class LoadBalancingMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly LoadBalancingAlgorithmFactory _loadBalancingAlgorithmFactory;
		private readonly IOptionsMonitor<LoadBalancingConfiguration> _optionsMonitor;
		private readonly ILogger<LoadBalancingMiddleware> _logger;
		private readonly HttpClient _httpClient;

		public LoadBalancingMiddleware(RequestDelegate next, LoadBalancingAlgorithmFactory loadBalancingAlgorithmFactory, IOptionsMonitor<LoadBalancingConfiguration> optionsMonitor,	
			ILogger<LoadBalancingMiddleware> logger, HttpClient httpClient)
		{
			ArgumentNullException.ThrowIfNull(next, nameof(next));
			ArgumentNullException.ThrowIfNull(loadBalancingAlgorithmFactory, nameof(loadBalancingAlgorithmFactory));
			ArgumentNullException.ThrowIfNull(optionsMonitor, nameof(optionsMonitor));
			ArgumentNullException.ThrowIfNull(logger, nameof(logger));
			ArgumentNullException.ThrowIfNull(httpClient, nameof(httpClient));
			_next = next;
			_loadBalancingAlgorithmFactory = loadBalancingAlgorithmFactory;
			_optionsMonitor = optionsMonitor;
			_logger = logger;
			_httpClient = httpClient;
		}

		public async Task Invoke(HttpContext context)
		{
			var url = context.Request.Path.Value!;

			var method = context.Request.Method;

			try
			{
				_logger.LogInformation($"Processing request {url} {method}");

				var loadBalancingPolicy = _optionsMonitor.CurrentValue.LoadBalancingPolicy;

				var algorithm = _loadBalancingAlgorithmFactory.ResolveByPolicy(_optionsMonitor.CurrentValue.LoadBalancingPolicy);

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

				await ForwardRequest(context, server);

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

		private async Task ForwardRequest(HttpContext context, Server? destinationServer)
		{
			var requestMessage = new HttpRequestMessage();
			requestMessage.Method = new HttpMethod(context.Request.Method);
			requestMessage.RequestUri = new System.Uri($"{destinationServer?.Uri}/{context.Request.Path}");

			foreach (var header in context.Request.Headers)
			{
				requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
			}

			if (context.Request.ContentLength is > 0)
			{
				requestMessage.Content = new StreamContent(context.Request.Body);
			}

			var responseMessage = await _httpClient.SendAsync(requestMessage);

			context.Response.StatusCode = (int)responseMessage.StatusCode;
			foreach (var header in responseMessage.Headers)
			{
				context.Response.Headers[header.Key] = header.Value.ToArray();
			}

			await responseMessage.Content.CopyToAsync(context.Response.Body);
		}
	}
}
