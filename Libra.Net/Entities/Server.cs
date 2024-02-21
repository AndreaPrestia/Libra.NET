using System.Diagnostics.CodeAnalysis;

namespace Libra.Net.Entities;

/// <summary>
/// This is a Server entity containing it's Endpoint
/// </summary>
/// <param name="Endpoint"></param>
[ExcludeFromCodeCoverage]
public record Server(string Endpoint);