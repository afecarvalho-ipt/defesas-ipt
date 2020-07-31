FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build-env

WORKDIR /app

COPY *.csproj .

RUN dotnet restore

COPY . .

RUN dotnet publish \
    -r linux-musl-x64 \
    /p:PublishSingleFile=true \
    /p:PublishTrimmed=true \
    /p:CrossGenDuringPublish=false \
    /p:PublishReadyToRun=true \
    /p:PublishReadyToRunShowWarnings=true \
    -c Release -o out

FROM mcr.microsoft.com/dotnet/core/runtime-deps:3.1-alpine

RUN adduser -D -u 1000 app \
    && addgroup app root \
    && mkdir /app \
    && mkdir -p /home/app/.aspnet \
    && chown -R app:root /app /home/app/.aspnet \
    && chmod 664 -R /home/app/.aspnet 

USER app

WORKDIR /app

COPY --from=build-env --chown=app:root /app/out .

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:5000 \
    DOTNET_RUNNING_IN_CONTAINER=true

VOLUME ["/home/app/.aspnet"]

EXPOSE 5000

ENTRYPOINT ["/app/Schedules"]
