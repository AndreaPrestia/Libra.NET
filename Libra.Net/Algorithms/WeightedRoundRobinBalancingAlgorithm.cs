using Libra.Net.Abstractions;
using Libra.Net.Configurations;
using Libra.Net.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Libra.Net.Algorithms
{
    /// <summary>
    /// This class implements the WeightedRoundRobin load balancing algorithm
    /// </summary>
    internal class WeightedRoundRobinBalancingAlgorithm : ILoadBalancingAlgorithm
    {
        private Dictionary<string, int>? _servers;
        private readonly ILogger<WeightedRoundRobinBalancingAlgorithm> _logger;
        private int _currentIndex;
        private readonly object _lockObject = new();

        public WeightedRoundRobinBalancingAlgorithm(IOptionsMonitor<LoadBalancingConfiguration> optionsMonitor, ILogger<WeightedRoundRobinBalancingAlgorithm> logger)
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
            if (_servers == null || _servers.Count == 0)
            {
                _logger.LogWarning("No servers loaded in configuration.");
                return null;
            }

            lock (_lockObject)
            {
                var index = _currentIndex;
                string? nextServer = null;

                while (true)
                {

                    index = (index + 1) % _servers.Count;
                    if (index == _currentIndex)
                    {
                        _currentIndex = (_currentIndex + 1) % _servers.Count;
                        break;
                    }

                    var server = _servers.ElementAt(index);

                    if (server.Value > 0)
                    {
                        nextServer = server.Key;
                        _servers[server.Key]--;
                        break;
                    }

                }

                return !string.IsNullOrWhiteSpace(nextServer) ? new Server(nextServer) : null;
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
                _servers ??= new();

                _servers.Clear();

                _currentIndex = 0;

                _servers = configuration.Servers.Count > 0 ? new Dictionary<string, int>(configuration.Servers.Select((s, i) => new KeyValuePair<string, int>(s, (configuration.Servers.Count - i) * 5)).OrderByDescending(e => e.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value)) : null;
            }
        }
    }
}
