FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["API/API.csproj", "API/"]
COPY ["Client/Client.csproj", "Client/"]
RUN dotnet restore "./API/./API.csproj"
RUN dotnet restore "./Client/Client.csproj"
COPY . .
#WORKDIR "/src/API"
RUN dotnet build "./API/API.csproj" -c $BUILD_CONFIGURATION -o /app/build
RUN dotnet build "./Client/Client.csproj" -c $BUILD_CONFIGURATION -o /app/wasm/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./API/API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
RUN dotnet publish "./Client/Client.csproj" --no-restore -c Release -o /app/wasm/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=publish /app/wasm/publish/wwwroot ./wwwroot/
ENTRYPOINT ["dotnet", "API.dll"]