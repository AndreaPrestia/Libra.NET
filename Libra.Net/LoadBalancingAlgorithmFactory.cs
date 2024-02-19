using Libra.Net.Abstractions;
using Libra.Net.Algorithms;
using Libra.Net.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Libra.Net
{
	public sealed class LoadBalancingAlgorithmFactory
	{
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly ILogger<LoadBalancingAlgorithmFactory> _logger;

		public LoadBalancingAlgorithmFactory(IServiceScopeFactory scopeFactory, ILogger<LoadBalancingAlgorithmFactory> logger)
		{
			ArgumentNullException.ThrowIfNull(scopeFactory, nameof(scopeFactory));
			ArgumentNullException.ThrowIfNull(logger, nameof(logger));
			_scopeFactory = scopeFactory;
			_logger = logger;
		}

		public ILoadBalancingAlgorithm? ResolveByPolicy(string loadBalancingPolicy)
		{
			ArgumentNullException.ThrowIfNull(loadBalancingPolicy);

			using var scope = _scopeFactory.CreateScope();
			return loadBalancingPolicy switch
			{
				nameof(LoadBalancingPolicy.RoundRobin) =>
					scope.ServiceProvider.GetService<RoundRobinBalancingAlgorithm>(),
				nameof(LoadBalancingPolicy.WeightedRoundRobin) =>
					scope.ServiceProvider.GetService<WeightedRoundRobinBalancingAlgorithm>(),
				nameof(LoadBalancingPolicy.LeastConnections) =>
					scope.ServiceProvider.GetService<LeastConnectionsBalancingAlgorithm>(),
				_ => null
			};
		}

	}
}
