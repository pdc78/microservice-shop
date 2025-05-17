#docker build --progress=plain -t api-gateway-img -f ApiGateway/Dockerfile .
#docker run -d -p 5096:80 --name api-gateway  api-gateway-img

# Use official .NET runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# CMD ["ls", "-l", "/src"]

COPY ./ApiGateway .

RUN ls -la

WORKDIR "/src/ApiGateway"
RUN dotnet restore "APIGateway.csproj"
RUN dotnet publish -c Release -o /app

# Run stage
FROM base AS final
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "APIGateway.dll"]
