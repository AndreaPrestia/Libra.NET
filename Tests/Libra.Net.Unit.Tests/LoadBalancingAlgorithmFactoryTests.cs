using Libra.Net.Algorithms;
using Libra.Net.Configurations;
using Libra.Net.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;

namespace Libra.Net.Unit.Tests
{
    public class LoadBalancingAlgorithmFactoryTests
    {
        private readonly IHost _host;

        private readonly Mock<IOptionsMonitor<LoadBalancingConfiguration>> _mockOptionsMonitor = new();

        private readonly LoadBalancingConfiguration _configuration = new("RoundRobin",
            new List<string>()
            {
                "10.0.0.1"
            });

        public LoadBalancingAlgorithmFactoryTests()
        {
            _mockOptionsMonitor.Setup(x => x.CurrentValue).Returns(_configuration);

            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((_, services) =>
                {
                    services.AddSingleton(typeof(IOptionsMonitor<LoadBalancingConfiguration>), _mockOptionsMonitor.Object);
                    services.AddSingleton<LoadBalancingAlgorithmFactory>();
                    services.AddSingleton<RoundRobinBalancingAlgorithm>();
                    services.AddSingleton<WeightedRoundRobinBalancingAlgorithm>();
                    services.AddSingleton<LeastConnectionsBalancingAlgorithm>();
                })
                .Build();
        }

        [Fact]
        public void ResolveByPolicy_RoundRobin_Ok()
        {
            // Arrange
            var service = _host.Services.GetRequiredService<LoadBalancingAlgorithmFactory>();

            // Act
            var result = service.ResolveByPolicy(LoadBalancingPolicy.RoundRobin);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<RoundRobinBalancingAlgorithm>(result);
        }

        [Fact]
        public void ResolveByPolicy_WeightedRoundRobin_Ok()
        {
            // Arrange
            var service = _host.Services.GetRequiredService<LoadBalancingAlgorithmFactory>();

            // Act
            var result = service.ResolveByPolicy(LoadBalancingPolicy.WeightedRoundRobin);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<WeightedRoundRobinBalancingAlgorithm>(result);
        }

        [Fact]
        public void ResolveByPolicy_LeastConnections_Ok()
        {
            // Arrange
            var service = _host.Services.GetRequiredService<LoadBalancingAlgorithmFactory>();

            // Act
            var result = service.ResolveByPolicy(LoadBalancingPolicy.LeastConnections);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<LeastConnectionsBalancingAlgorithm>(result);
        }
    }
}
