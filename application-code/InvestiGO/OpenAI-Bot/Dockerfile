FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["OpenAI-Bot/OpenAI-Bot.csproj", "OpenAI-Bot/"]
RUN dotnet restore "OpenAI-Bot/OpenAI-Bot.csproj"
COPY . .
WORKDIR "/src/OpenAI-Bot"
RUN dotnet build "OpenAI-Bot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OpenAI-Bot.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OpenAI-Bot.dll"]
