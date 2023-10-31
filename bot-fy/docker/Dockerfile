FROM mcr.microsoft.com/dotnet/sdk:7.0 as build
WORKDIR /app

COPY . .

RUN dotnet restore bot-fy.csproj
RUN dotnet build "bot-fy.csproj" -c Release -o /app/build

FROM build AS publish

RUN dotnet publish "bot-fy.csproj" -c Release -o /app/publish
FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base

RUN apt-get update -y && \
    apt-get upgrade -y && \
    apt-get install libopus0 libopus-dev libsodium23 libsodium-dev -y && \
    apt-get clean -y

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "bot-fy.dll"]