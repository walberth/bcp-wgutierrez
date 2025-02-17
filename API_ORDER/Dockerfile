# These ARGs allow for swapping out the base used to make the final image when debugging from VS
ARG LAUNCHING_FROM_VS

# This sets the base image for final, but only if LAUNCHING_FROM_VS has been defined
ARG FINAL_BASE_IMAGE=${LAUNCHING_FROM_VS:+aotdebug}

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 4070

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Install clang/zlib1g-dev dependencies for publishing to native
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
    clang \
    zlib1g-dev \
    curl \
    && rm -rf /var/lib/apt/lists/*
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["API_ORDER/API_ORDER.csproj", "API_ORDER/"]
RUN dotnet restore "./API_ORDER/API_ORDER.csproj"
COPY . .
WORKDIR "/src/API_ORDER"
RUN dotnet build "./API_ORDER.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./API_ORDER.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=true

# This stage is used as the base for the final stage when launching from VS to support debugging in regular mode (Default when not using the Debug configuration)
FROM base AS aotdebug
USER root

# Install GDB to support native debugging
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
    gdb \
    curl
USER app

# Configurar el puerto a usar
ENV ASPNETCORE_URLS=http://+:5258
ENV JaegerServer=http://localhost:4317
ENV ApiDeliveryUrl=http://localhost:5291
ENV Serilog__WriteTo__2__Args__nodes__0=http://localhost:9200

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM ${FINAL_BASE_IMAGE:-mcr.microsoft.com/dotnet/aspnet:8.0} AS final

# Install GDB to support native debugging
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
    curl

WORKDIR /app
EXPOSE 4070
COPY --from=publish /app/publish .
ENTRYPOINT ["./API_ORDER"]