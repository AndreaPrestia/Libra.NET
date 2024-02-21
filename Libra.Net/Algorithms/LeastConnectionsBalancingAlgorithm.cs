using Libra.Net.Abstractions;
using Libra.Net.Configurations;
using Libra.Net.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Libra.Net.Algorithms;

/// <summary>
/// This class implements the LeastConnections load balancing algorithm
/// </summary>
internal class LeastConnectionsBalancingAlgorithm : IReleaseConnectionAlgorithm
{
    private Dictionary<string, int>? _servers;
    private readonly ILogger<LeastConnectionsBalancingAlgorithm> _logger;
    private readonly object _lockObject = new();

    public LeastConnectionsBalancingAlgorithm(IOptionsMonitor<LoadBalancingConfiguration> optionsMonitor, ILogger<LeastConnectionsBalancingAlgorithm> logger)
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
        lock (_lockObject)
        {
            if (_servers == null || _servers.Count == 0)
            {
                _logger.LogWarning("No servers loaded in configuration.");
                return null;
            }
          
            var selectedServer = _servers.MinBy(s => s.Value);
            _logger.LogDebug($"Server {selectedServer.Key} connections count {selectedServer.Value}");


            _servers[selectedServer.Key] = selectedServer.Value + 1;

            _logger.LogDebug($"Server {selectedServer.Key} new connections count {_servers[selectedServer.Key]}");

            return !string.IsNullOrWhiteSpace(selectedServer.Key) ? new Server(selectedServer.Key) : null;
        }
    }

    public void ReleaseConnection(Server? server)
    {
        ArgumentNullException.ThrowIfNull(server);
        ArgumentException.ThrowIfNullOrWhiteSpace(server.Endpoint);

        lock (_lockObject)
        {
            if (_servers == null || _servers.Count == 0)
            {
                _logger.LogWarning("No servers loaded in configuration.");
                return;
            }

            var serverFound = _servers.TryGetValue(server.Endpoint, out var serverConnections);

            if (serverFound && serverConnections > 0)
            {
                _servers[server.Endpoint]--;
                _logger.LogDebug($"Decreased connections for server {server.Endpoint}");
            }
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
            _servers ??= new Dictionary<string, int>();

            _servers.Clear();

            _servers = configuration.Servers.Count > 0 ? new Dictionary<string, int>(configuration.Servers.Select(s => new KeyValuePair<string, int>(s, 0)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value)) : null;
        }
    }
}