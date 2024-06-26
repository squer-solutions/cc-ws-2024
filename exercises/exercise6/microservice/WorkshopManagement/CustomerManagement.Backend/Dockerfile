﻿FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CustomerManagement.Backend/CustomerManagement.Backend.csproj", "CustomerManagement.Backend/"]
COPY ["WorkshopManagement.App/WorkshopManagement.App.csproj", "WorkshopManagement.App/"]
RUN dotnet restore "CustomerManagement.Backend/CustomerManagement.Backend.csproj"
COPY . .
WORKDIR "/src/CustomerManagement.Backend"
RUN dotnet build "CustomerManagement.Backend.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "CustomerManagement.Backend.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CustomerManagement.Backend.dll"]
