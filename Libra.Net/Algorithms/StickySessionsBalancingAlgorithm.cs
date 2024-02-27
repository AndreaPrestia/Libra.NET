using Libra.Net.Abstractions;
using Libra.Net.Configurations;
using Libra.Net.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Libra.Net.Algorithms
{
    /// <summary>
    /// This class implements the StickySession load balancing algorithm
    /// </summary>
    internal class StickySessionsBalancingAlgorithm : ILoadBalancingAlgorithm
    {
        private List<string>? _servers;
        private readonly ILogger<RoundRobinBalancingAlgorithm> _logger;
        private readonly object _lockObject = new();

        public StickySessionsBalancingAlgorithm(IOptionsMonitor<LoadBalancingConfiguration> optionsMonitor, ILogger<RoundRobinBalancingAlgorithm> logger)
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

        public Server? GetNextServer(string? sessionId)
        {
            if (sessionId == null)
            {
                _logger.LogWarning("No sessionId provided.");
                return null;
            }

            lock (_lockObject)
            {
                if (_servers == null || _servers.Count == 0)
                {
                    _logger.LogWarning("No servers loaded in configuration.");
                    return null;
                }

                var hash = sessionId.GetHashCode();
                var index = Math.Abs(hash) % _servers.Count;

                var server = _servers.ElementAt(index);

                if (string.IsNullOrWhiteSpace(server))
                {
                    _logger.LogWarning($"No server found at index {index}");
                    return null;
                }

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

                _servers = configuration.Servers;
            }
        }
    }
}
