# Libra.NET
A load balancer written in .NET

## How does it works?

It can use three load balancing algorithms:

- Round Robin
- Weighted Round Robin
- Least Connections

The algorithm to be used is defined in the **Libra** node of the appsettings.json file.

### Round Robin
This is the simplest algorithm used by **Libra.NET**. It schedules in an equal manner the usage of the **Servers** configured.

### Weighted Round Robin
This is a little bit complex. Weighted Round Robin is a more advanced load balancing configuration. This technique allows you to point records to multiple IP addresses like basic Round Robin, but has the added flexibility of distributing weight based on the needs of your domain.
Every server configured has a weight that is assigned by **Libra.NET** using the position in the array.
If a server is in an higher position in the list the weight applied increase.

### Least Connections
This is not so complex. Least connections load balancing is a dynamic load balancing algorithm where client requests are distributed to the application server with the least number of active connections at the time the client request is received.

## Configuration

Add this section to appsettings.json. 

```
"LibraNet": {
   "LoadBalancingPolicy": "RoundRobin",
   "Servers": [
      "10.0.0.0",
      "10.255.255.255",
      "https://10.0.0.1:443",
      "my-in-balancer-server.xyz",
      "https://my-in-balancer-server2.xyz:443"
    ]
  }
```

The configuration is made of:

Property | Type | Context | Allowed Values |
--- | --- | --- | --- |
LoadBalancingPolicy | string | The value of the load balancing algorithm to apply. | RoundRobin, WeightedRoundRobin, LeastConnections |
Servers | array<string> | The list of servers that are under load balancing. | A valid ipaddress (with or without port) or an hostname |

## Usage

**Libra.NET** exposes one extension:

```
AddLibraNet(this IHostBuilder builder);
```

### AddLibraNet
This extension it is used for:

- Registering of **LoadBalancingConfiguration** from appsettings.json by environment.
- Registering of **ILoadBalancingAlgorithm** implementations.
- Registering of **HttpRequestManager** implementation.

It must be used to use **Libra.NET**

### HttpRequestManager
This component exposes one method called **Task ForwardRequest(HttpContext context, Server? destinationServer, CancellationToken cancellationToken = default)** that given an HttpContext and a Server as destination forwards to it the call, waits for the response and copies it in the HttpContext.Response.

### LoadBalacingMiddleware
To use in a simple way **Libra.NET** it has been created a middleware, called **LoadBalancingMiddleware**.
It is an **ASP.NET** middleware that , using the **LoadBalancingConfiguration.LoadBalancingPolicy** retrieves the correct algorithm and forwards it to the destination server found with the **HttpRequestManager**.

## TODO

- Unit testing
- Code refactoring

