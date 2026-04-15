# ════════════════════════════════════════════════════════════
# STAGE 1 — Restore: Download NuGet packages
# ════════════════════════════════════════════════════════════
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore
ARG PROJECT=SwiftApp.ERP.WebApi
WORKDIR /build

# Copy solution-level files for restore
COPY Directory.Build.props Directory.Packages.props ./
COPY src/*/*.csproj ./src/
# Flatten .csproj files into correct directories
RUN for file in src/*.csproj; do \
        dir="src/$(basename "$file" .csproj)"; \
        mkdir -p "$dir"; \
        mv "$file" "$dir/"; \
    done

RUN dotnet restore "src/${PROJECT}/${PROJECT}.csproj"

# ════════════════════════════════════════════════════════════
# STAGE 2 — Build: Compile and publish
# ════════════════════════════════════════════════════════════
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG PROJECT=SwiftApp.ERP.WebApi
WORKDIR /build

COPY --from=restore /root/.nuget /root/.nuget
COPY --from=restore /build ./
COPY src/ ./src/

RUN dotnet publish "src/${PROJECT}/${PROJECT}.csproj" \
    -c Release \
    -o /publish \
    --no-restore

# ════════════════════════════════════════════════════════════
# STAGE 3 — Runtime: Minimal ASP.NET Core image
# ════════════════════════════════════════════════════════════
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
LABEL maintainer="SwiftApp ERP <dev@swiftapp.ch>"
LABEL description="SwiftApp ERP — Swiss Watch Manufacturing & Retail ERP"

RUN apk add --no-cache curl tzdata \
    && cp /usr/share/zoneinfo/Europe/Zurich /etc/localtime \
    && echo "Europe/Zurich" > /etc/timezone \
    && apk del tzdata

RUN addgroup -g 1000 -S appgroup \
    && adduser -u 1000 -S appuser -G appgroup

WORKDIR /app
COPY --from=build /publish ./

RUN chown -R appuser:appgroup /app
USER appuser:appgroup

EXPOSE 8080

ARG PROJECT=SwiftApp.ERP.WebApi
ENV APP_DLL="${PROJECT}.dll" \
    ASPNETCORE_URLS="http://+:8080" \
    DOTNET_RUNNING_IN_CONTAINER=true \
    TZ=Europe/Zurich

ENTRYPOINT ["sh", "-c", "exec dotnet \"$APP_DLL\""]

HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD wget -qO- http://127.0.0.1:8080/health || exit 1
