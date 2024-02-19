using Libra.Net.Abstractions;
using Libra.Net.Configurations;
using Libra.Net.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace Libra.Net.Algorithms
{
	internal class WeightedRoundRobinBalancingAlgorithm : ILoadBalancingAlgorithm
	{
		private readonly ConcurrentDictionary<string, int>? _servers;
		private readonly IOptionsMonitor<LoadBalancingConfiguration> _optionsMonitor;
		private readonly ILogger<WeightedRoundRobinBalancingAlgorithm> _logger;
		private int _currentIndex;

		public WeightedRoundRobinBalancingAlgorithm(IOptionsMonitor<LoadBalancingConfiguration> optionsMonitor, ILogger<WeightedRoundRobinBalancingAlgorithm> logger)
		{
			ArgumentNullException.ThrowIfNull(optionsMonitor, nameof(optionsMonitor));
			ArgumentNullException.ThrowIfNull(logger, nameof(logger));
			_optionsMonitor = optionsMonitor;
			var serversCount = optionsMonitor.CurrentValue.Servers.Count;
			_servers = serversCount > 0 ? new ConcurrentDictionary<string, int>(optionsMonitor.CurrentValue.Servers.Select((s, i) => new KeyValuePair<string, int>(s, (serversCount - i) * 5)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value)) : null;
			_logger = logger;
		}

		public Server? GetNextServer()
		{
			if (_servers == null || _servers.Count == 0)
			{
				_logger.LogWarning("No servers loaded in configuration.");
				return null;
			}

			string selectedServer;

			while (true)
			{
				var server = _servers.ElementAt(_currentIndex);

				if (server.Value > 0)
				{
					_logger.LogDebug($"Server {server.Key} weight {server.Value}");

					selectedServer = server.Key;

					if(string.IsNullOrEmpty(selectedServer) )
					{
						_logger.LogWarning($"Invalid server key at index {_currentIndex}");
						return null;
					}

					var serverWeightUpdated = _servers.TryUpdate(server.Key, server.Value - 1, server.Value);
					_logger.LogDebug($"Server {server.Key} new weight {server.Value}");
					break;
				}
				_currentIndex = (_currentIndex + 1) % _servers.Count;
			}

			_currentIndex = (_currentIndex + 1) % _servers.Count;
			return new Server(selectedServer);
		}

		private int CalculateTotalWeight()
		{
			if(_servers == null || _servers.Count == 0)
			{
				return 0;
			}

			int total = 0;
			foreach (var server in _servers)
			{
				total += server.Value;
			}
			return total;
		}
	}
}
