using System.Diagnostics.CodeAnalysis;

namespace Libra.Net.Configurations;

/// <summary>
/// The appsettings.json configuration object for Libra.Net
/// </summary>
/// <param name="LoadBalancingPolicy"></param>
/// <param name="Servers"></param>
[ExcludeFromCodeCoverage]
public record LoadBalancingConfiguration(string LoadBalancingPolicy, List<string> Servers)
{
    public const string LibraNet = nameof(LibraNet);
}