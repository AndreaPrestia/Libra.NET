using Libra.Net.Algorithms;
using Libra.Net.Configurations;
using Libra.Net.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Libra.Net
{
	public static class Extensions
	{
		public static void AddLibraNet(this IHostBuilder builder) 
		{
			builder.ConfigureServices((context, services) =>
			{
				services.Configure<LoadBalancingConfiguration>(context.Configuration.GetSection(
					LoadBalancingConfiguration.LibraNet));

				services.AddHttpClient<LoadBalancingMiddleware>("loadBalancingClient", (_, _) => { })
					.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler()
					{
						PooledConnectionLifetime = TimeSpan.FromMinutes(15)
					})
					.SetHandlerLifetime(Timeout.InfiniteTimeSpan);

				services.AddLoadBalancingAlgorithms();
			});
		}
		
		public static void UseLibraNet(this IApplicationBuilder app, Func<HttpContext, bool> predicate)
		{
			app.UseWhen(predicate,
	appBranch => { appBranch.UseMiddleware<LoadBalancingMiddleware>(); });
		}

		private static void AddLoadBalancingAlgorithms(this IServiceCollection services)
		{
			ArgumentNullException.ThrowIfNull(services);

			services.AddScoped<RoundRobinBalancingAlgorithm>();
			services.AddScoped<WeightedRoundRobinBalancingAlgorithm>();
			services.AddScoped<LeastConnectionsBalancingAlgorithm>();
			services.AddSingleton<LoadBalancingAlgorithmFactory>();
		}
	}
}
