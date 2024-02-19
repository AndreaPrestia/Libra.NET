using Libra.Net.Abstractions;
using Libra.Net.Configurations;
using Libra.Net.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace Libra.Net.Algorithms
{
	internal class RoundRobinBalancingAlgorithm : ILoadBalancingAlgorithm
	{
		private readonly ConcurrentBag<string> _servers;
		private readonly IOptionsMonitor<LoadBalancingConfiguration> _optionsMonitor;
		private int _currentIndex;
		private readonly ILogger<RoundRobinBalancingAlgorithm> _logger;

		public RoundRobinBalancingAlgorithm(IOptionsMonitor<LoadBalancingConfiguration> optionsMonitor, ILogger<RoundRobinBalancingAlgorithm> logger)
		{
			ArgumentNullException.ThrowIfNull(optionsMonitor, nameof(optionsMonitor));
			ArgumentNullException.ThrowIfNull(logger, nameof(logger));
			_optionsMonitor = optionsMonitor;
			_servers = new ConcurrentBag<string>(optionsMonitor.CurrentValue.Servers);
			_logger = logger;
		}

		public Server? GetNextServer()
		{
			if(_servers == null || _servers.Count == 0)
			{
				_logger.LogWarning("No servers loaded in configuration.");
				return null;
			}

			string server = _servers.ElementAt(_currentIndex);

			if (string.IsNullOrWhiteSpace(server))
			{
				_logger.LogWarning($"No server found at index {_currentIndex}");
				return null;
			}

			_logger.LogDebug($"Found server {server} for index {_currentIndex}");

			_currentIndex = (_currentIndex + 1) % _servers.Count;

			_logger.LogDebug($"New calculated index for next iteration {_currentIndex}");

			return new Server(server);
		}
	}
}
