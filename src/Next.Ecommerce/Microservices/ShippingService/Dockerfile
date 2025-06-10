FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj files
COPY ["Shared/Contracts.Events/Contracts.Events.csproj", "Shared/Contracts.Events/"]
COPY ["ShippingService/ShippingService.Application/ShippingService.Application.csproj", "ShippingService/ShippingService.Application/"]
COPY ["ShippingService/ShippingService.Infrastructure/ShippingService.Infrastructure.csproj", "ShippingService/ShippingService.Infrastructure/"]

# Restore
RUN dotnet restore "ShippingService/ShippingService.Infrastructure/ShippingService.Infrastructure.csproj"

# Copy all source
COPY . .

# Build
WORKDIR "/src/ShippingService/ShippingService.Infrastructure"
RUN dotnet build "ShippingService.Infrastructure.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
RUN dotnet publish "ShippingService.Infrastructure.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ShippingService.Infrastructure.dll"]
