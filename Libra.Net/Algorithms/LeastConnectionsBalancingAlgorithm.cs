using Libra.Net.Abstractions;
using Libra.Net.Configurations;
using Libra.Net.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace Libra.Net.Algorithms
{
	internal class LeastConnectionsBalancingAlgorithm : ILoadBalancingAlgorithm
	{
		private readonly ConcurrentDictionary<string, int>? _servers;
		private readonly IOptionsMonitor<LoadBalancingConfiguration> _optionsMonitor;
		private readonly ILogger<LeastConnectionsBalancingAlgorithm> _logger;
		private int _currentIndex;

		public LeastConnectionsBalancingAlgorithm(IOptionsMonitor<LoadBalancingConfiguration> optionsMonitor, ILogger<LeastConnectionsBalancingAlgorithm> logger)
		{
			ArgumentNullException.ThrowIfNull(optionsMonitor, nameof(optionsMonitor));
			ArgumentNullException.ThrowIfNull(logger, nameof(logger));
			_optionsMonitor = optionsMonitor;
			var serversCount = optionsMonitor.CurrentValue.Servers.Count;
			_servers = serversCount > 0 ? new ConcurrentDictionary<string, int>(optionsMonitor.CurrentValue.Servers.Select(s => new KeyValuePair<string, int>(s, 0)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value)) : null;
			_logger = logger;
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
