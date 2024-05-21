#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ChattingAPI/ChattingAPI.csproj", "ChattingAPI/"]
RUN dotnet restore "./ChattingAPI/ChattingAPI.csproj"
COPY . .
WORKDIR "/src/ChattingAPI"
RUN dotnet build "./ChattingAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ChattingAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Install PostgreSQL client
RUN apt-get update \
    && apt-get install -y postgresql-client

# Set up environment variables for PostgreSQL connection
ENV POSTGRES_DB=your_database
ENV POSTGRES_USER=your_user
ENV POSTGRES_PASSWORD=your_password
ENV POSTGRES_HOST=your_postgresql_host
ENV POSTGRES_PORT=5432

# Set up Serilog configuration
ENV Serilog__MinimumLevel__Default=Information
ENV Serilog__MinimumLevel__Override__Microsoft=Warning
ENV Serilog__MinimumLevel__Override__System=Warning
ENV Serilog__WriteTo__File__Path=/logs/app.log
ENV Serilog__WriteTo__File__Formatter=Serilog.Formatting.Compact.CompactJsonFormatter, Serilog.Formatting.Compact

# Create log directory
RUN mkdir -p /logs

ENTRYPOINT ["dotnet", "ChattingAPI.dll"]