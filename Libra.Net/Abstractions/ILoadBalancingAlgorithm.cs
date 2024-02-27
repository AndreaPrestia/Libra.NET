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
        /// <param name="sessionId">Used if needed to check the session id of the incoming connection</param>
        /// <returns></returns>
        public Server? GetNextServer(string? sessionId);
    }
}
