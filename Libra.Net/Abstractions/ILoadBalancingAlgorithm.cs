using Libra.Net.Entities;

namespace Libra.Net.Abstractions
{
	internal interface ILoadBalancingAlgorithm
	{
		public Server? GetNextServer();
	}
}
