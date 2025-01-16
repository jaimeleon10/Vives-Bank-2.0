FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8087

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Vives_Bank_Net/Vives_Bank_Net.csproj", "Vives_Bank_Net/"]
RUN dotnet restore "Vives_Bank_Net/Vives_Bank_Net.csproj"
COPY . .
WORKDIR "/src/Vives_Bank_Net"
RUN dotnet build "Vives_Bank_Net.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Vives_Bank_Net.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Vives_Bank_Net.dll"]