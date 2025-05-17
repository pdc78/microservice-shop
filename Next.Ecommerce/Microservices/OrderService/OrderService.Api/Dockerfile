#docker build --progress=plain -t api-order-img -f .\Microservices\OrderService\OrderService.Api\Dockerfile .\Microservices\OrderService\OrderService.Api\
#docker run -d -p 5097:8080 --name api-order api-order-img

# Explanation of Each Part:
# docker run → Starts a new Docker container.

# -d → Runs the container in detached mode, meaning it runs in the background.

# -p 5097:8080 → Maps port 8080 inside the container to 5097 on your local machine.

# So if the application inside the container listens on 8080, you can access it at localhost:5097.

# --name api-catalog → Gives the container the name api-catalog for easy identification.

# api-catalog-img → Specifies the Docker image used to create the container.

# Use official .NET runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

RUN pwd

COPY . .
CMD ["ls", "-l", "/src"]
RUN ls -l
# WORKDIR "/src"
RUN dotnet restore "OrderService.Api.csproj"
RUN dotnet publish -c Release -o /app

# Run stage
FROM base AS final
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "OrderService.Api.dll"]
