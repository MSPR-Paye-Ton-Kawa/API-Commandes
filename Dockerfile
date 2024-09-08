# Utilisez l'image SDK pour construire, publier, et tester
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copiez les fichiers de projet
COPY API_Commandes/API_Commandes.csproj API_Commandes/
COPY API_Commandes.Tests/API_Commandes.Tests.csproj API_Commandes.Tests/

# Restaurer les dépendances
RUN dotnet restore API_Commandes/API_Commandes.csproj
RUN dotnet restore API_Commandes.Tests/API_Commandes.Tests.csproj

# Copiez tous les fichiers sources
COPY . .

# Construisez les projets
WORKDIR /src/API_Commandes
RUN dotnet build -c Release -o /app/build

WORKDIR /src/API_Commandes.Tests
RUN dotnet build -c Release -o /app/build

# Exécutez les tests
WORKDIR /src/API_Commandes.Tests
RUN dotnet test --no-restore --verbosity normal

# Publiez le projet API_Commandes
WORKDIR /src/API_Commandes
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Utilisez l'image de base .NET pour ASP.NET Core pour la phase finale
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "API_Commandes.dll"]
