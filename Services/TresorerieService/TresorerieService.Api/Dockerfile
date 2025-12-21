# Consultez https://aka.ms/customizecontainer pour savoir comment personnaliser votre conteneur de débogage et comment Visual Studio utilise ce Dockerfile pour générer vos images afin d’accélérer le débogage.

# Cet index est utilisé lors de l’exécution à partir de VS en mode rapide (par défaut pour la configuration de débogage)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# Cette phase est utilisée pour générer le projet de service
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Services/TresorerieService/TresorerieService.Api/TresorerieService.Api.csproj", "Services/TresorerieService/TresorerieService.Api/"]
COPY ["Services/TresorerieService/TresorerieService.Application/TresorerieService.Application.csproj", "Services/TresorerieService/TresorerieService.Application/"]
COPY ["Services/TresorerieService/TresorerieService.Domain/TresorerieService.Domain.csproj", "Services/TresorerieService/TresorerieService.Domain/"]
COPY ["Services/TresorerieService/TresorerieService.Infrastructure/TresorerieService.Infrastructure.csproj", "Services/TresorerieService/TresorerieService.Infrastructure/"]
RUN dotnet restore "./Services/TresorerieService/TresorerieService.Api/TresorerieService.Api.csproj"
COPY . .
WORKDIR "/src/Services/TresorerieService/TresorerieService.Api"
RUN dotnet build "./TresorerieService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Cette étape permet de publier le projet de service à copier dans la phase finale
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TresorerieService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Cette phase est utilisée en production ou lors de l’exécution à partir de VS en mode normal (par défaut quand la configuration de débogage n’est pas utilisée)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TresorerieService.Api.dll"]