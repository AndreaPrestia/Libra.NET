using Libra.Net.Algorithms;
using Libra.Net.Configurations;
using Libra.Net.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Libra.Net.Unit.Tests.Algorithms
{
    public class LeastConnectionsBalancingAlgorithmTests
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

        public LeastConnectionsBalancingAlgorithmTests()
        {
            _mockOptionsMonitor.Setup(x => x.CurrentValue).Returns(_configuration);

            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((_, services) =>
                {
                    services.AddSingleton(typeof(IOptionsMonitor<LoadBalancingConfiguration>), _mockOptionsMonitor.Object);
                    services.AddSingleton<LeastConnectionsBalancingAlgorithm>();
                })
                .Build();
        }

        [Fact]
        public void GetNextServer_Ok()
        {
            // Arrange
            var service = _host.Services.GetRequiredService<LeastConnectionsBalancingAlgorithm>();

            foreach (var t in _configuration.Servers)
            {
                // Act
                var result = service.GetNextServer(null);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(t, result.Endpoint);
            }

            // Act
            var finalResult = service.GetNextServer(null);

            // Assert
            Assert.NotNull(finalResult);
            Assert.Equal(_configuration.Servers.FirstOrDefault(), finalResult.Endpoint);
        }

        [Fact]
        public void GetNextServer_WithRelease_Connection_Ok()
        {
            // Arrange
            var service = _host.Services.GetRequiredService<LeastConnectionsBalancingAlgorithm>();

            foreach (var t in _configuration.Servers)
            {
                // Act
                var result = service.GetNextServer(null);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(_configuration.Servers.FirstOrDefault(), result.Endpoint);

                service.ReleaseConnection(result);
            }

            // Act
            var finalResult = service.GetNextServer(null);

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

            var service = _host.Services.GetRequiredService<LeastConnectionsBalancingAlgorithm>();

            // Act
            var result = service.GetNextServer(null);

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

            var service = _host.Services.GetRequiredService<LeastConnectionsBalancingAlgorithm>();

            // Act
            var result = service.GetNextServer(null);

            // Assert
            Assert.Null(result);
        }
    }
}
