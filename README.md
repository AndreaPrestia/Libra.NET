# Libra.NET
A load balancer written in .NET

## How does it works?

It can use three load balancing algorithms:

- RoundRobin
- WeightedRoundRobin
- LeastConnections

The algorithm to be used is defined in the **Libra** node of the appsettings.json file.

#### RoundRobin
This is the simplest algorithm used by **Libra.NET**. It schedules in an equal manner the usage of the **Servers** configured.

### WeightedRoundRobin
This is a little bit complex. Weighted Round Robin is a more advanced load balancing configuration. This technique allows you to point records to multiple IP addresses like basic Round Robin, but has the added flexibility of distributing weight based on the needs of your domain.
Every server configured has a weight that is assigned by **Libra.NET** using the position in the array.
If a server is in an higher position in the list the weight applied increase.

### LeastConnections
This is not so complex. Least connections load balancing is a dynamic load balancing algorithm where client requests are distributed to the application server with the least number of active connections at the time the client request is received.

### Configuration

Add this section to appsettings.json. 

```
"LibraNet": {
   "LoadBalancingPolicy": "RoundRobin",
   "Servers": [
      "10.0.0.0",
      "10.255.255.255"
    ]
  }
```

The configuration is made of:

Property | Type | Context | Allowed Values |
--- | --- | --- | --- |
LoadBalancingPolicy | string | The value of the load balancing algorithm to apply. | RoundRobin, WeightedRoundRobin, LeastConnections |
Servers | array<string> | The list of servers that are under load balancing. | A valid ipaddress (with or without port) or an hostname |

## TODO

- Unit testing
- Code refactoring

