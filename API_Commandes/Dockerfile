#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["API_Commandes/API_Commandes.csproj", "API_Commandes/"]
COPY ["API_Commandes.Tests/API_Commandes.Tests.csproj", "API_Commandes.Tests/"]
RUN dotnet restore "API_Commandes/API_Commandes.csproj"
COPY . .
WORKDIR "/src/API_Commandes"
RUN dotnet build "API_Commandes.csproj" -c Release -o /app/build

# Run tests
FROM build AS testrunner
WORKDIR /src/API_Commandes.Tests
RUN dotnet test --no-restore --verbosity normal

FROM build AS publish
RUN dotnet publish "API_Commandes.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "API_Commandes.dll"]
