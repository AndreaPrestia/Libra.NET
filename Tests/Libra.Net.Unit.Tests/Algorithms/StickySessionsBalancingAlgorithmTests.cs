using Libra.Net.Algorithms;
using Libra.Net.Configurations;
using Libra.Net.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;

namespace Libra.Net.Unit.Tests.Algorithms
{
    public class StickySessionsBalancingAlgorithmTests
    {
        private readonly IHost _host;

        private readonly Mock<IOptionsMonitor<LoadBalancingConfiguration>> _mockOptionsMonitor = new();

        private readonly LoadBalancingConfiguration _configuration = new("StickySessions",
            new List<string>()
            {
            "10.0.0.1",
            "10.0.0.2",
            "10.0.0.3",
            "10.0.0.4"
            });

        public StickySessionsBalancingAlgorithmTests()
        {
            _mockOptionsMonitor.Setup(x => x.CurrentValue).Returns(_configuration);

            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((_, services) =>
                {
                    services.AddSingleton(typeof(IOptionsMonitor<LoadBalancingConfiguration>), _mockOptionsMonitor.Object);
                    services.AddSingleton<StickySessionsBalancingAlgorithm>();
                })
                .Build();
        }

        [Fact]
        public void GetNextServer_Ok()
        {
            // Arrange
            var service = _host.Services.GetRequiredService<StickySessionsBalancingAlgorithm>();

            var sessionIds = new List<string>()
            {
                "abc", "def", "ghi", "lmn"
            };

            var i = 0;

            var servers = new List<Server?>();

            //Act
            foreach (var result in _configuration.Servers.Select(_ => service.GetNextServer(sessionIds[i])))
            {
                servers.Add(result);
                i++;
            }

            var repeatedValues = servers.GroupBy(x => x?.Endpoint)
                .Where(g => g.Count() > 1)
                .Select(g => new { Value = g.Key, Count = g.Count() }).ToList();
           
            var hasRepeatedValues = repeatedValues.Any(x => x.Count > 1);

            // Assert
            Assert.True(hasRepeatedValues);
        }

        [Fact]
        public void GetNextServer_No_Servers()
        {
            // Arrange
            _configuration.Servers.Clear();
            _mockOptionsMonitor.Setup(x => x.CurrentValue).Returns(_configuration);

            var service = _host.Services.GetRequiredService<StickySessionsBalancingAlgorithm>();

            // Act
            var result = service.GetNextServer("sessionId");

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

            var service = _host.Services.GetRequiredService<StickySessionsBalancingAlgorithm>();

            // Act
            var result = service.GetNextServer("sessionId");

            // Assert
            Assert.Null(result);
        }
    }
}
