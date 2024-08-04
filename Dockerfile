# Utilisez l'image de base .NET pour ASP.NET Core
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Utilisez l'image SDK pour construire et publier
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copiez les fichiers de projet
COPY API_Commandes/API_Commandes.csproj API_Commandes/
COPY API_Commandes.Tests/API_Commandes.Tests.csproj API_Commandes.Tests/

# Restaurer les dépendances pour les deux projets
RUN dotnet restore API_Commandes/API_Commandes.csproj
RUN dotnet restore API_Commandes.Tests/API_Commandes.Tests.csproj

# Copiez tous les fichiers sources
COPY . .

# Construisez les deux projets
WORKDIR /src/API_Commandes
RUN dotnet build -c Release -o /app/build

WORKDIR /src/API_Commandes.Tests
RUN dotnet build -c Release -o /app/build

# Publiez les projets
FROM build AS publish
WORKDIR /src/API_Commandes
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Construisez l'image finale
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "API_Commandes.dll"]
