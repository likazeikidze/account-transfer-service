FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/AccountTransferService.Api/AccountTransferService.Api.csproj", "src/AccountTransferService.Api/"]
COPY ["src/AccountTransferService.Infrastructure/AccountTransferService.Infrastructure.csproj", "src/AccountTransferService.Infrastructure/"]
COPY ["src/AccountTransferService.Domain/AccountTransferService.Domain.csproj", "src/AccountTransferService.Domain/"]
RUN dotnet restore "src/AccountTransferService.Api/AccountTransferService.Api.csproj"

COPY . .
WORKDIR /src/src/AccountTransferService.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "AccountTransferService.Api.dll"]
