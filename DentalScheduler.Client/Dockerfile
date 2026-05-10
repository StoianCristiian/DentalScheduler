FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DentalScheduler.Client/DentalScheduler.Client.csproj", "DentalScheduler.Client/"]
RUN dotnet restore "DentalScheduler.Client/DentalScheduler.Client.csproj"
COPY ["DentalScheduler.Client/", "DentalScheduler.Client/"]
WORKDIR "/src/DentalScheduler.Client"
RUN dotnet build "./DentalScheduler.Client.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DentalScheduler.Client.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
USER app
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DentalScheduler.Client.dll"]
