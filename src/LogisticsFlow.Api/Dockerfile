FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY ["src/LogisticsFlow.Api/LogisticsFlow.Api.csproj", "LogisticsFlow.Api/"]

COPY ["src/LogisticsFlow.Application/LogisticsFlow.Application.csproj", "LogisticsFlow.Application/"]

COPY ["src/LogisticsFlow.Domain/LogisticsFlow.Domain.csproj", "LogisticsFlow.Domain/"]

COPY ["src/LogisticsFlow.Infrastructure/LogisticsFlow.Infrastructure.csproj", "LogisticsFlow.Infrastructure/"]

RUN dotnet restore "LogisticsFlow.Api/LogisticsFlow.Api.csproj"

COPY src/ .

WORKDIR /src/LogisticsFlow.Api

RUN dotnet publish "LogisticsFlow.Api.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

USER app

ENTRYPOINT ["dotnet", "LogisticsFlow.Api.dll"]



