using Libra.Net.Entities;

namespace Libra.Net.Abstractions
{
	/// <summary>
	/// The ILoadBalancingAlgorithm interface, used to implement the LoadBalancing strategies to use
	/// </summary>
	public interface ILoadBalancingAlgorithm
	{
		/// <summary>
		/// Returns the next server available to forward the request by specific load balancing policy
		/// </summary>
		/// <returns></returns>
		public Server? GetNextServer();
	}
}
