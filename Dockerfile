# Imagen de producción de la API de ColombiaGO (.NET 9).
#   docker build -t colombiago-api .
#   docker run -p 8080:8080 -e ConnectionStrings__ColombiaGoDb="..." -e Supabase__Url="..." colombiago-api
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY Directory.Build.props ./
COPY src/ ./src/
RUN dotnet restore src/ColombiaGo.Api/ColombiaGo.Api.csproj
RUN dotnet publish src/ColombiaGo.Api/ColombiaGo.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
# Render, Railway, Fly y Cloud Run indican el puerto con la variable PORT; si no existe se usa 8080.
CMD ["sh", "-c", "ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080} exec dotnet ColombiaGo.Api.dll"]
