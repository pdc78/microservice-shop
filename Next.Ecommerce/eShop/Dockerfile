# docker build --progress=plain -t eshop-img -f ./eShop/Dockerfile .
#docker run -d -p 5040:80 --name eshop  eshop-img


FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

RUN pwd

COPY ./eShop .
# CMD ["ls", "-l", "/src"]
RUN ls -l


# COPY ["eShop.csproj", "./"]

WORKDIR /src
RUN ls -l
RUN dotnet restore "eShop.csproj"
# COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "eshop.dll"]