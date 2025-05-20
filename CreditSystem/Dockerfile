# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY *.sln .
COPY CreditSystem.API/*.csproj ./CreditSystem.API/
COPY CreditSystem.Application/*.csproj ./CreditSystem.Application/
COPY CreditSystem.Domain/*.csproj ./CreditSystem.Domain/
COPY CreditSystem.Infrastructure/*.csproj ./CreditSystem.Infrastructure/
COPY CreditSystem.Contracts/*.csproj ./CreditSystem.Contracts/
COPY CreditSystem.Tests/*.csproj ./CreditSystem.Tests/

# Restore dependencies
RUN dotnet restore

# Copy the rest of the source code
COPY . .

# Publish the application
RUN dotnet publish CreditSystem.API/CreditSystem.API.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Expose HTTP and HTTPS ports
EXPOSE 5000
EXPOSE 5001

# Set the entry point
ENTRYPOINT ["dotnet", "CreditSystem.API.dll"]
