using Libra.Net.Abstractions;
using Libra.Net.Configurations;
using Libra.Net.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace Libra.Net.Algorithms
{
	/// <summary>
	/// This class implements the LeastConnections load balancing algorithm
	/// </summary>
	internal class LeastConnectionsBalancingAlgorithm : ILoadBalancingAlgorithm
	{
		private ConcurrentDictionary<string, int>? _servers;
		private readonly ILogger<LeastConnectionsBalancingAlgorithm> _logger;

		public LeastConnectionsBalancingAlgorithm(IOptionsMonitor<LoadBalancingConfiguration> optionsMonitor, ILogger<LeastConnectionsBalancingAlgorithm> logger)
		{
			ArgumentNullException.ThrowIfNull(optionsMonitor, nameof(optionsMonitor));
			ArgumentNullException.ThrowIfNull(logger, nameof(logger));
			_servers = optionsMonitor.CurrentValue.Servers.Count > 0 ? new ConcurrentDictionary<string, int>(optionsMonitor.CurrentValue.Servers.Select(s => new KeyValuePair<string, int>(s, 0)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value)) : null;
			_logger = logger;

			optionsMonitor.OnChange((config, _) =>
			{
				_servers = config.Servers.Count > 0 ? new ConcurrentDictionary<string, int>(config.Servers.Select(s => new KeyValuePair<string, int>(s, 0)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value)) : null;
			});
		}

		public Server? GetNextServer()
		{
			if(_servers == null || _servers.Count == 0)
			{
				_logger.LogWarning("No servers loaded in configuration.");
				return null;
			}

			var selectedServer = _servers.OrderBy(s => s.Value).First();
			_logger.LogDebug($"Server {selectedServer.Key} connections count {selectedServer.Value}");

			var serverConnectionsUpdated = _servers.TryUpdate(selectedServer.Key, selectedServer.Value + 1, selectedServer.Value);
			
			_logger.LogDebug($"Server {selectedServer.Key} new connections count {selectedServer.Value}");

			return new Server(selectedServer.Key);
		}
	}
}
