using Libra.Net.Entities;

namespace Libra.Net.Abstractions
{
	public interface ILoadBalancingAlgorithm
	{
		public Server? GetNextServer();
	}
}
