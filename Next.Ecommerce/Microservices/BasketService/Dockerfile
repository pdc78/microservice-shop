FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
# USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR  /src
COPY ["BasketService.Api/BasketService.Api.csproj", "BasketService.Api/"]
COPY ["BasketService.Domain/BasketService.Domain.csproj", "BasketService.Domain/"]
COPY ["BasketService.Application/BasketService.Application.csproj", "BasketService.Application/"]
# COPY ["BasketService.Infrastructure/BasketService.Infrastructure.csproj", "BasketService.Infrastructure/"]

RUN dotnet restore "BasketService.Api/BasketService.Api.csproj"

COPY . .
WORKDIR  "/src/BasketService.Api"
RUN dotnet build "./BasketService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "BasketService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BasketService.Api.dll"]