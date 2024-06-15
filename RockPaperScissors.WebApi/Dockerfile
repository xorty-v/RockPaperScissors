FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["RockPaperScissors.WebApi/RockPaperScissors.WebApi.csproj", "RockPaperScissors.WebApi/"]
RUN dotnet restore "RockPaperScissors.WebApi/RockPaperScissors.WebApi.csproj"
COPY . .
WORKDIR "/src/RockPaperScissors.WebApi"
RUN dotnet build "RockPaperScissors.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RockPaperScissors.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RockPaperScissors.WebApi.dll"]
