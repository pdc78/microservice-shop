FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
# USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR  /src
COPY ["PaymentService.Domain/PaymentService.Domain.csproj", "PaymentService.Domain/"]
COPY ["PaymentService.Application/PaymentService.Application.csproj", "PaymentService.Application/"]
COPY ["PaymentService.Infrastructure/PaymentService.Infrastructure.csproj", "PaymentService.Infrastructure/"]

RUN dotnet restore "PaymentService.Infrastructure/PaymentService.Infrastructure.csproj"

COPY . .
WORKDIR  "/src/PaymentService.Infrastructure"
RUN dotnet build "./PaymentService.Infrastructure.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "PaymentService.Infrastructure.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PaymentService.Infrastructure.dll"]