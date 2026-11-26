FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/FlowerShop.API/FlowerShop.API.csproj", "src/FlowerShop.API/"]
COPY ["src/FlowerShop.Application/FlowerShop.Application.csproj", "src/FlowerShop.Application/"]
COPY ["src/FlowerShop.Infrastructure/FlowerShop.Infrastructure.csproj", "src/FlowerShop.Infrastructure/"]
COPY ["src/FlowerShop.Domain/FlowerShop.Domain.csproj", "src/FlowerShop.Domain/"]

RUN dotnet restore "src/FlowerShop.API/FlowerShop.API.csproj"

COPY . .
WORKDIR "/src/src/FlowerShop.API"

FROM build AS publish
RUN dotnet publish "FlowerShop.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN mkdir -p /app/uploads

ENTRYPOINT ["dotnet", "FlowerShop.API.dll"]
