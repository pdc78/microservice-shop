FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
# USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR  /src
COPY ["ShippingService.Domain/ShippingService.Domain.csproj", "ShippingService.Domain/"]
COPY ["ShippingService.Application/ShippingService.Application.csproj", "ShippingService.Application/"]
COPY ["ShippingService.Infrastructure/ShippingService.Infrastructure.csproj", "ShippingService.Infrastructure/"]

RUN dotnet restore "ShippingService.Infrastructure/ShippingService.Infrastructure.csproj"

COPY . .
WORKDIR  "/src/ShippingService.Infrastructure"
RUN dotnet build "./ShippingService.Infrastructure.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ShippingService.Infrastructure.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ShippingService.Infrastructure.dll"]