FROM mcr.microsoft.com/dotnet/runtime:7.0-alpine AS base
WORKDIR /app

RUN apk add --no-cache icu-libs

FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build
WORKDIR /src
COPY ["IstioWorker/IstioWorker.csproj", "IstioWorker/"]
RUN dotnet nuget locals all --clear
RUN dotnet restore "IstioWorker/IstioWorker.csproj"
COPY . .
WORKDIR "/src/IstioWorker"
RUN dotnet build "IstioWorker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IstioWorker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IstioWorker.dll"]
