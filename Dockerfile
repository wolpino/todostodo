# syntax=docker/dockerfile:1

# ── Frontend build ────────────────────────────────────────────────────────────
FROM node:22-bookworm-slim AS web-build
WORKDIR /web

RUN corepack enable

COPY src/todostodo.web/package.json src/todostodo.web/pnpm-lock.yaml ./
RUN pnpm install --frozen-lockfile

COPY src/todostodo.web/ ./
RUN pnpm exec vite build

# ── API build ─────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS api-build
WORKDIR /src

COPY src/todostodo.api/todostodo.api.csproj src/todostodo.api/
RUN dotnet restore src/todostodo.api/todostodo.api.csproj

COPY src/todostodo.api/ src/todostodo.api/
RUN dotnet publish src/todostodo.api/todostodo.api.csproj -c Release -o /app/publish /p:UseAppHost=false

# ── Runtime ───────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=api-build /app/publish .
COPY --from=web-build /web/dist ./wwwroot

EXPOSE 8080

ENTRYPOINT ["dotnet", "todostodo.api.dll"]
