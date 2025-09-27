# Use the official ASP.NET runtime as a base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy everything into the container
COPY . .

# Restore dependencies
RUN dotnet restore "DeliStarter.csproj"

# Build and publish
RUN dotnet publish "DeliStarter.csproj" -c Release -o /app/publish

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DeliStarter.dll"]
