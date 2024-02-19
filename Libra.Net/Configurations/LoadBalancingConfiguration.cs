namespace Libra.Net.Configurations
{
	public record LoadBalancingConfiguration(string LoadBalancingPolicy, List<string> Servers)
	{
		public const string LibraNet = nameof(LibraNet);
	}
}
