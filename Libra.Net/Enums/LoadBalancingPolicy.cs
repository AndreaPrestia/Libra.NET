namespace Libra.Net.Enums;

/// <summary>
/// Valid Load balancing policies allowed
/// </summary>
public enum LoadBalancingPolicy
{
    RoundRobin,
    WeightedRoundRobin,
    LeastConnections
}