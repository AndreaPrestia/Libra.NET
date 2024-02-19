using Libra.Net.Entities;
using Microsoft.AspNetCore.Http;

namespace Libra.Net
{
	public sealed class HttpRequestManager
	{
		private readonly HttpClient _httpClient;

		public HttpRequestManager(HttpClient httpClient)
		{
			ArgumentNullException.ThrowIfNull(httpClient, nameof(httpClient));
			_httpClient = httpClient;
		}

		/// <summary>
		/// Forward an http request to a destination server
		/// </summary>
		/// <param name="context"></param>
		/// <param name="destinationServer"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task ForwardRequest(HttpContext context, Server? destinationServer, CancellationToken cancellationToken = default)
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

			var responseMessage = await _httpClient.SendAsync(requestMessage, cancellationToken);

			context.Response.StatusCode = (int)responseMessage.StatusCode;
			foreach (var header in responseMessage.Headers)
			{
				context.Response.Headers[header.Key] = header.Value.ToArray();
			}

			await responseMessage.Content.CopyToAsync(context.Response.Body, cancellationToken);
		}
	}
}
