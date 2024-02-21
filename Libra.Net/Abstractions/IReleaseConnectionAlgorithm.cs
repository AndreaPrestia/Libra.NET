using Libra.Net.Entities;

namespace Libra.Net.Abstractions
{
    /// <summary>
    /// This interface contains the part for release connection to implement for specific policy
    /// </summary>
    public interface IReleaseConnectionAlgorithm : ILoadBalancingAlgorithm
    {
        /// <summary>
        /// Release a connection for the server
        /// </summary>
        /// <param name="server"></param>
        void ReleaseConnection(Server? server);
    }
}
