using Libra.Net.Algorithms;
using Libra.Net.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;

namespace Libra.Net.Unit.Tests.Algorithms;

public class RoundRobinBalancingAlgorithmTests
{
    private readonly IHost _host;

    private readonly Mock<IOptionsMonitor<LoadBalancingConfiguration>> _mockOptionsMonitor = new();

    private readonly LoadBalancingConfiguration _configuration = new("RoundRobin",
        new List<string>()
        {
            "10.0.0.1",
            "10.0.0.2",
            "10.0.0.3",
            "10.0.0.4"
        });

    public RoundRobinBalancingAlgorithmTests()
    {
        _mockOptionsMonitor.Setup(x => x.CurrentValue).Returns(_configuration);

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                services.AddSingleton(typeof(IOptionsMonitor<LoadBalancingConfiguration>), _mockOptionsMonitor.Object);
                services.AddSingleton<RoundRobinBalancingAlgorithm>();
            })
            .Build();
    }

    [Fact]
    public void GetNextServer_Ok()
    {
        // Arrange
        var service = _host.Services.GetRequiredService<RoundRobinBalancingAlgorithm>();

        foreach (var t in _configuration.Servers)
        {
            // Act
            var result = service.GetNextServer();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(t, result.Endpoint);
        }

        // Act
        var finalResult = service.GetNextServer();

        // Assert
        Assert.NotNull(finalResult);
        Assert.Equal(_configuration.Servers.FirstOrDefault(), finalResult.Endpoint);
    }

    [Fact]
    public void GetNextServer_No_Servers()
    {
        // Arrange
        _configuration.Servers.Clear();
        _mockOptionsMonitor.Setup(x => x.CurrentValue).Returns(_configuration);

        var service = _host.Services.GetRequiredService<RoundRobinBalancingAlgorithm>();

        // Act
        var result = service.GetNextServer();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetNextServer_No_Name_Server()
    {
        // Arrange
        _mockOptionsMonitor.Setup(x => x.CurrentValue).Returns(new LoadBalancingConfiguration("RoundRobin", new List<string>()
        {
            string.Empty
        }));

        var service = _host.Services.GetRequiredService<RoundRobinBalancingAlgorithm>();

        // Act
        var result = service.GetNextServer();

        // Assert
        Assert.Null(result);
    }
}