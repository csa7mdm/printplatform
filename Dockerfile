# Stage 1 — restore & build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY PrintPlatform.sln .
COPY src/PrintPlatform.API/PrintPlatform.API.csproj                          src/PrintPlatform.API/
COPY src/PrintPlatform.Domain/PrintPlatform.Domain.csproj                    src/PrintPlatform.Domain/
COPY src/PrintPlatform.Application/PrintPlatform.Application.csproj          src/PrintPlatform.Application/
COPY src/PrintPlatform.Infrastructure/PrintPlatform.Infrastructure.csproj    src/PrintPlatform.Infrastructure/
COPY src/packages/PrintPlatform.Gamification/PrintPlatform.Gamification.csproj src/packages/PrintPlatform.Gamification/
COPY src/packages/PrintPlatform.Loyalty/PrintPlatform.Loyalty.csproj         src/packages/PrintPlatform.Loyalty/
COPY tests/PrintPlatform.Domain.Tests/PrintPlatform.Domain.Tests.csproj      tests/PrintPlatform.Domain.Tests/
COPY tests/PrintPlatform.Application.Tests/PrintPlatform.Application.Tests.csproj tests/PrintPlatform.Application.Tests/
COPY tests/PrintPlatform.Gamification.Tests/PrintPlatform.Gamification.Tests.csproj tests/PrintPlatform.Gamification.Tests/
COPY tests/PrintPlatform.Loyalty.Tests/PrintPlatform.Loyalty.Tests.csproj    tests/PrintPlatform.Loyalty.Tests/

RUN dotnet restore

COPY . .
RUN dotnet build -c Release --no-restore

# Stage 2 — publish
FROM build AS publish
RUN dotnet publish src/PrintPlatform.API/PrintPlatform.API.csproj \
    -c Release --no-build -o /app/publish

# Stage 3 — runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

COPY --from=publish /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

HEALTHCHECK --interval=30s --timeout=5s --start-period=15s --retries=3 \
  CMD curl -f http://localhost:8080/health/live || exit 1

ENTRYPOINT ["dotnet", "PrintPlatform.API.dll"]
