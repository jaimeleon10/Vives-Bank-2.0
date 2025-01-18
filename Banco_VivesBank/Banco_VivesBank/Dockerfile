FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8087

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Banco_VivesBank/Banco_VivesBank.csproj", "Banco_VivesBank/"]
RUN dotnet restore "Banco_VivesBank/Banco_VivesBank.csproj"
COPY . .
WORKDIR "/src/Banco_VivesBank"
RUN dotnet build "Banco_VivesBank.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Banco_VivesBank.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Banco_VivesBank.dll"]