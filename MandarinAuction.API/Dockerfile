FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["MandarinAuction.API/MandarinAuction.API.csproj", "MandarinAuction.API/"]
COPY ["MandarinAuction.Application/MandarinAuction.Application.csproj", "MandarinAuction.Application/"]
COPY ["MandarinAuction.Domain/MandarinAuction.Domain.csproj", "MandarinAuction.Domain/"]
COPY ["MandarinAuction.Infrastructure/MandarinAuction.Infrastructure.csproj", "MandarinAuction.Infrastructure/"]

RUN dotnet restore "MandarinAuction.API/MandarinAuction.API.csproj"

COPY . .

WORKDIR "/src/MandarinAuction.API"
RUN dotnet build "MandarinAuction.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MandarinAuction.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MandarinAuction.API.dll"]
