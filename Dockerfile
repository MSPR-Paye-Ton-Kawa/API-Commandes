# Utiliser l'image .NET SDK pour construire l'application de test
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Vérifiez la structure des répertoires
RUN echo "Listing files in /src:" && ls -la /src
RUN echo "Listing files in /src/API_Commandes:" && ls -la /src/API_Commandes
RUN echo "Listing files in /src/API_Commandes.Tests:" && ls -la /src/API_Commandes.Tests

# Copier les fichiers de projet
COPY API_Commandes/API_Commandes.csproj ./API_Commandes/
COPY API_Commandes.Tests/API_Commandes.Tests.csproj ./API_Commandes.Tests/
WORKDIR /src/API_Commandes.Tests

# Restaurer les dépendances et construire
RUN dotnet restore
RUN dotnet build -c Release -o /app/build

# Exécuter les tests
ENTRYPOINT ["dotnet", "test", "--no-restore", "--verbosity", "normal"]
