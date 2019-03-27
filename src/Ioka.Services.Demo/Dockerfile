FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY Ioka.Services.Demo/Ioka.Services.Demo.csproj Ioka.Services.Demo/
RUN dotnet restore Ioka.Services.Demo/Ioka.Services.Demo.csproj
COPY . .
WORKDIR /src/Ioka.Services.Demo
RUN dotnet build Ioka.Services.Demo.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish Ioka.Services.Demo.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Ioka.Services.Demo.dll"]
