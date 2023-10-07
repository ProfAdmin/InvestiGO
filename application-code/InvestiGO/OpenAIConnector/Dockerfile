FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["OpenAIConnector/OpenAIConnector.csproj", "OpenAIConnector/"]
RUN dotnet restore "OpenAIConnector/OpenAIConnector.csproj"
COPY . .
WORKDIR "/src/OpenAIConnector"
RUN dotnet build "OpenAIConnector.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OpenAIConnector.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OpenAIConnector.dll"]
