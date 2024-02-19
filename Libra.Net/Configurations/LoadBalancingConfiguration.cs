namespace Libra.Net.Configurations
{
	/// <summary>
	/// The appsettings.json configuration object for Libra.Net
	/// </summary>
	/// <param name="LoadBalancingPolicy"></param>
	/// <param name="Servers"></param>
	public record LoadBalancingConfiguration(string LoadBalancingPolicy, List<string> Servers)
	{
		public const string LibraNet = nameof(LibraNet);
	}
}
