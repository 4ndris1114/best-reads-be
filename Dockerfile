# Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5071

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy and restore
COPY ["best-reads-be.csproj", "./"]
RUN dotnet restore "best-reads-be.csproj"

# Copy everything and build
COPY . .

WORKDIR "/src"
RUN dotnet build "best-reads-be.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "best-reads-be.csproj" -c Release -o /app/publish

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "best-reads-be.dll"]
