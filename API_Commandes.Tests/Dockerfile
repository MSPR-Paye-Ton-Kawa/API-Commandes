#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

# Dockerfile dans API_Commandes.Tests
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
RUN ls -la /src

COPY ["../API_Commandes.Tests/API_Commandes.Tests.csproj", "../"]
RUN dotnet restore "../API_Commandes.Tests/API_Commandes.Tests.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "../API_Commandes.Tests.csproj" -c Release -o /app/build

FROM build AS test
WORKDIR /src/API_Commandes.Tests
ENTRYPOINT ["dotnet", "test", "--no-restore", "--verbosity", "normal"]
