FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

COPY . .
RUN dotnet restore bot-fy.csproj
RUN dotnet build "bot-fy.csproj" --no-restore -c Release -o /app/build

RUN apt-get update -y && \
    apt-get upgrade -y && \
    apt-get install libopus0 libopus-dev libsodium23 libsodium-dev ffmpeg clang zlib1g-dev -y && \
    apt-get clean -y

RUN dotnet publish "bot-fy.csproj" --no-build -c Release -o /app/publish --self-contained true -r linux-x64

##FROM scratch
WORKDIR /app
CMD ls -la
USER root
ENTRYPOINT ["/app/publish/bot-fy.so"]