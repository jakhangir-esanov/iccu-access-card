FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore
WORKDIR /src

COPY Directory.Build.props .editorconfig ./

COPY src/Iccu.Api/Iccu.Api.csproj                       src/Iccu.Api/
COPY src/Iccu.Application/Iccu.Application.csproj       src/Iccu.Application/
COPY src/Iccu.Domain/Iccu.Domain.csproj                 src/Iccu.Domain/
COPY src/Iccu.Infrastructure/Iccu.Infrastructure.csproj src/Iccu.Infrastructure/
COPY src/Iccu.Presentation/Iccu.Presentation.csproj     src/Iccu.Presentation/

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages,sharing=locked \
    dotnet restore src/Iccu.Api/Iccu.Api.csproj

FROM restore AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY src/ src/

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages,sharing=locked \
    dotnet publish src/Iccu.Api/Iccu.Api.csproj \
        -c $BUILD_CONFIGURATION \
        -o /app/publish \
        --no-restore \
        /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_EnableDiagnostics=0

RUN mkdir -p /var/iccu/files && chown app:app /var/iccu/files

USER app

COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "Iccu.Api.dll"]
