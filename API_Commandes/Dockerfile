#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat
# Étape de base pour l'application
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Étape de build pour l'application
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copier le fichier de projet API_Commandes
COPY ["API_Commandes/API_Commandes.csproj", "API_Commandes/"]
# Restaurer les dépendances
RUN dotnet restore "API_Commandes/API_Commandes.csproj"
# Copier le reste des fichiers
COPY . ./
# Construire l'application
RUN dotnet build "API_Commandes/API_Commandes.csproj" -c Release -o /app/build

# Étape de publication pour l'application
FROM build AS publish
RUN dotnet publish "API_Commandes/API_Commandes.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Étape pour exécuter les tests
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS test
WORKDIR /src

# Copier les fichiers de test
COPY ["API_Commandes.Tests/API_Commandes.Tests.csproj", "API_Commandes.Tests/"]
COPY ["API_Commandes/"] "API_Commandes/"
# Restaurer et exécuter les tests
RUN dotnet restore "API_Commandes.Tests/API_Commandes.Tests.csproj"
RUN dotnet test "API_Commandes.Tests/API_Commandes.Tests.csproj" --no-restore --verbosity normal

# Étape finale pour l'application
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "API_Commandes.dll"]
