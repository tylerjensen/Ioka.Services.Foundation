# Ioka.Services.Foundation
Foundation for .NET Core microservices libraries including use of IdentityServer4, Swagger, Serilog and Elasticsearch.

This library is NOT production ready. It simply represents much the shareable work I have been doing in .NET Core and microservices using IdentityServer4. It's a work in progress and my hope is that others may benefit from some of the things I'm learning in this code base. Some of this code comes from work I have done in the past and those for whom I did this work have granted me free license to share it with the community.

## Projects and Code

The Ioka.Services.Foundation solution contains a few projects that could be used as a starting point for your own microservices built using .NET Core, currently using version 2.2 and .NET Standard version 2.0. There are few tests and this is one reason you should NOT use this code for production. Write your own unit tests. I'll be adding unit tests as I continue to work on and refine this code base.

Here's a list of the projects and what their responsibility is.
  - **docker-compose**: run this code in Linux using Docker for Windows. Containers for the identity server, the demo service and a standard ELK instance are defined.
  - **Ioka.Services.Foundation**: this is the heart of the code, gathering all the common dependencies for an *Ioka* service which includes the following easily configured in your service's *Startup.cs*.
    - Logging to ELK via Serilog
    - A version provider to create an */api-meta/version* endpoint that will provide an extensible JSON object with version details of the service and its dependencies.
    - Support for both JSON and ProtoBuf formatters.
    - Support for the included instance of IdentityServer4 to allow the use of standard Authorize attribute on controllers and action methods. This uses a key, public and private keys included in the repo as examples, so another reason to NOT use this code base directly in production.
    - Support for Swagger API docs via Swashbuckle auto generated and made available at the /api-docs/index.html endpoint. (The docker-compose project is set to open this API docs page when you run in Visual Studio.)
  - **Ioka.Services.Demo**: This is a very simple demo ASP.NET Core microservice where all the above features are enabled using the foundation library, abstracting away the configuration to simplify the *Startup.cs*. This gives you an example of how the libary could be used but is not exhaustive, rather it is the easy happy path. There are many choices to make when creating such a library. The suggestion in this code base is that you may wish to consider standardizing the configuration of your ASP.NET Core microservices.
  - **Ioka.Services.Client**: This is a C# client designed to enable easy communication between your .NET Core clients, including microservices built using this code base. It enables easy authentication with the included implementation of IdentityServer4 and makes it easy to execute calls to any endpoint created using the patterns in this codebase using strongly typed request and response classes which would be defined in a shared library of DTOs.
  - **Ioka.Services.Foundation.Tests**: A place holder because I've been too lazy to implement unit tests in this code base. I will be repenting of this horror in the future.
  - **Ioka.Services.IdentityServer**: The IdentityServer4 implementation which supports the simple username/password flow which returns a JWT token and a refresh token. The refresh token service code is very simplistic and a production implementation would need to be injected which would enforce your own refresh rules and have access to any database backend that might store state regarding the refresh token, allowing you to invalidate it for example. The actual implementation of the IUserRepository is abstracted into another project, *Ioka.Services.IdentityServer.Common*, to provide an example of how you might use the default server implementation here but augment it with a specific implementation for actual user lookup and verification using a separate library.
  - **Ioka.Services.IdentityServer.Common**: A very simple example of how you might implement the IUserRepository referenced by the implementation using IdentityServer4 for authentication.
  - **Ioka.Services.TestConsole**: A console app designed to do a simple test of the *Ioka.Services.Client* library. Note that in debug mode, it waits you to manually indicate with an *Enter* key press to run the test, allowing you time to wait for the VS environment to spin up your services, including ELK, which can take some time depending on your environment.

## Linux, Docker, Logging and ELK

You will notice that the solution contains a docker-compose.yml file and is designed to be used with Docker for Windows set to run in Linux. The overall goal of this project includes being able to deploy services to Linux or Windows. It also assumes that logging will be done to ELK via the Serilog libaries. In your own system, you will want to modify this to your own environment and preferred logging library.
