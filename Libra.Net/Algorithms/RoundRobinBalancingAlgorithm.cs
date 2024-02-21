using Libra.Net.Abstractions;
using Libra.Net.Configurations;
using Libra.Net.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Libra.Net.Algorithms
{
	/// <summary>
	/// This class implements the RoundRobin load balancing algorithm
	/// </summary>
	internal class RoundRobinBalancingAlgorithm : ILoadBalancingAlgorithm
	{
        private List<string>? _servers;
        private int _currentIndex;
		private readonly ILogger<RoundRobinBalancingAlgorithm> _logger;
        private readonly object _lockObject = new();

		public RoundRobinBalancingAlgorithm(IOptionsMonitor<LoadBalancingConfiguration> optionsMonitor, ILogger<RoundRobinBalancingAlgorithm> logger)
		{
			ArgumentNullException.ThrowIfNull(optionsMonitor, nameof(optionsMonitor));
			ArgumentNullException.ThrowIfNull(logger, nameof(logger));
            _logger = logger;

            UpdateServers(optionsMonitor.CurrentValue);

            optionsMonitor.OnChange((config, _) =>
			{
                UpdateServers(config);
            });
		}

		public Server? GetNextServer()
		{
			if(_servers == null || _servers.Count == 0)
			{
				_logger.LogWarning("No servers loaded in configuration.");
				return null;
			}

            lock (_lockObject)
            {
                var server = _servers.ElementAt(_currentIndex);

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

        private void UpdateServers(LoadBalancingConfiguration? configuration)
        {
            if (configuration == null || configuration.Servers.Count == 0)
            {
                _logger.LogDebug("Servers from configuration not provided");
                return;
            }

            lock (_lockObject)
            {
                _servers ??= new List<string>();

                _servers.Clear();

                _currentIndex = 0;

                _servers = configuration.Servers;
            }
        }
    }
}
