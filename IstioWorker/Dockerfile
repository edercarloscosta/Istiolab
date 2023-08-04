FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["IstioWorker/IstioWorker.csproj", "IstioWorker/"]
RUN dotnet restore "IstioWorker/IstioWorker.csproj" -r linux-arm64
COPY . .
WORKDIR "/src/IstioWorker"
RUN dotnet build "IstioWorker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IstioWorker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IstioWorker.dll"]
