# Build de producción de la API. Contexto de build: apps/api/ (así lo debes
# configurar como "root directory" en Railway/Render).

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar solo los .csproj primero para cachear `dotnet restore` entre builds:
# mientras no cambien las dependencias, esta capa no se reconstruye aunque
# cambie el código.
COPY src/NovaERP.Domain/NovaERP.Domain.csproj src/NovaERP.Domain/
COPY src/NovaERP.Application/NovaERP.Application.csproj src/NovaERP.Application/
COPY src/NovaERP.Infrastructure/NovaERP.Infrastructure.csproj src/NovaERP.Infrastructure/
COPY src/NovaERP.API/NovaERP.API.csproj src/NovaERP.API/
RUN dotnet restore src/NovaERP.API/NovaERP.API.csproj

COPY src/ src/
RUN dotnet publish src/NovaERP.API/NovaERP.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080

# Railway/Render inyectan $PORT en tiempo de ejecución (no siempre 8080), así
# que hace falta expandir la variable al arrancar — de ahí el shell en vez de
# exec directo. `exec` dentro del shell reemplaza su proceso por el de dotnet
# para que SIGTERM le llegue directo (apagado ordenado con `docker stop`).
ENTRYPOINT ["/bin/sh", "-c", "exec dotnet NovaERP.API.dll --urls http://+:${PORT:-8080}"]
