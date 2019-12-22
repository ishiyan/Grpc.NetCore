# GrpcNetCore

gRPC playground

## Greeter

The standard greeter example shows how to:

- create unary, client streaming, server streaming and duplex (bi-directional) gRPC methods in ASP.NET Core, and call them from a client
- cancel streaming calls
- implement logger interceptors on both the server and client
- use insecure `http` in a client (see ConsumerHttp project)
- write unit tests to test a gRPC service directly (see `ProducerUnitTests` project)
- write integration tests using [Microsoft.AspNetCore.TestHost](https://www.nuget.org/packages/Microsoft.AspNetCore.TestHost/) to host a gRPC service with an in-memory test server and call it using a gRPC client (see `ProducerIntegrationTests` project)

This example was composed from different examples provided in [gRPC for .NET](https://github.com/grpc/grpc-dotnet).
