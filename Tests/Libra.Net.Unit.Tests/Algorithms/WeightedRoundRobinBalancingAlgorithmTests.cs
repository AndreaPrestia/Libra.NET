using Libra.Net.Algorithms;
using Libra.Net.Configurations;
using Libra.Net.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;

namespace Libra.Net.Unit.Tests.Algorithms
{
    public class WeightedRoundRobinBalancingAlgorithmTests
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

        public WeightedRoundRobinBalancingAlgorithmTests()
        {
            _mockOptionsMonitor.Setup(x => x.CurrentValue).Returns(_configuration);

            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((_, services) =>
                {
                    services.AddSingleton(typeof(IOptionsMonitor<LoadBalancingConfiguration>), _mockOptionsMonitor.Object);
                    services.AddSingleton<WeightedRoundRobinBalancingAlgorithm>();
                })
                .Build();
        }

        [Fact]
        public void GetNextServer_Ok()
        {
            // Arrange
            var service = _host.Services.GetRequiredService<WeightedRoundRobinBalancingAlgorithm>();

            var servers = new List<Server>();

            foreach (var result in _configuration.Servers.Select(_ => service.GetNextServer()))
            {
                // Assert
                Assert.NotNull(result);
                servers.Add(result);
            }

            // Assert
            Assert.NotNull(servers);
            Assert.Collection(servers, e1 =>
                {
                    Assert.Equal(_configuration.Servers[1], e1.Endpoint);
                },
                e2 =>
                {
                    Assert.Equal(_configuration.Servers[1], e2.Endpoint);
                }, e3 =>
                {
                    Assert.Equal(_configuration.Servers[1], e3.Endpoint);
                }, e4 =>
                {
                    Assert.Equal(_configuration.Servers[1], e4.Endpoint);
                });
        }

        [Fact]
        public void GetNextServer_No_Servers()
        {
            // Arrange
            _configuration.Servers.Clear();
            _mockOptionsMonitor.Setup(x => x.CurrentValue).Returns(_configuration);

            var service = _host.Services.GetRequiredService<WeightedRoundRobinBalancingAlgorithm>();

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

            var service = _host.Services.GetRequiredService<WeightedRoundRobinBalancingAlgorithm>();

            // Act
            var result = service.GetNextServer();

            // Assert
            Assert.Null(result);
        }
    }
}
