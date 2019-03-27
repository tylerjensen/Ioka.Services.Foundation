FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["Ioka.Services.IdentityServer/Ioka.Services.IdentityServer.csproj", "Ioka.Services.IdentityServer/"]
RUN dotnet restore "Ioka.Services.IdentityServer/Ioka.Services.IdentityServer.csproj"
COPY . .
WORKDIR "/src/Ioka.Services.IdentityServer"
RUN dotnet build "Ioka.Services.IdentityServer.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Ioka.Services.IdentityServer.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Ioka.Services.IdentityServer.dll"]