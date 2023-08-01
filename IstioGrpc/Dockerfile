FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build
WORKDIR /src
COPY ["IstioGrpc/IstioGrpc.csproj", "IstioGrpc/"]
RUN dotnet nuget locals all --clear
RUN dotnet restore "IstioGrpc/IstioGrpc.csproj" -r linux-arm64
COPY . .
WORKDIR "/src/IstioGrpc"
RUN dotnet build "IstioGrpc.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IstioGrpc.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IstioGrpc.dll"]
