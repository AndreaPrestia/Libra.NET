using Libra.Net.Abstractions;
using Libra.Net.Algorithms;
using Libra.Net.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Libra.Net
{
	/// <summary>
	/// This factory is used to retrieve the ILoadBalancingAlgorithm for loadBalancingPolicy provided
	/// </summary>
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

		/// <summary>
		/// Returns the ILoadBalancingAlgorithm implementation for loadBalancingPolicy passed as parameter.
		/// </summary>
		/// <param name="loadBalancingPolicy"></param>
		/// <returns></returns>
		public ILoadBalancingAlgorithm? ResolveByPolicy(LoadBalancingPolicy loadBalancingPolicy)
		{
			ArgumentNullException.ThrowIfNull(loadBalancingPolicy);

			using var scope = _scopeFactory.CreateScope();
			return loadBalancingPolicy switch
			{
				LoadBalancingPolicy.RoundRobin =>
					scope.ServiceProvider.GetService<RoundRobinBalancingAlgorithm>(),
				LoadBalancingPolicy.WeightedRoundRobin =>
					scope.ServiceProvider.GetService<WeightedRoundRobinBalancingAlgorithm>(),
				LoadBalancingPolicy.LeastConnections =>
					scope.ServiceProvider.GetService<LeastConnectionsBalancingAlgorithm>(),
				_ => null
			};
		}

	}
}
