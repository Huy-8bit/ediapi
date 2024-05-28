# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Cài đặt các công cụ biên dịch cần thiết
RUN apt-get update \
    && apt-get install -y clang gcc \
    && rm -rf /var/lib/apt/lists/*

# Copy everything and build
COPY . ./
RUN dotnet publish -c Release -o out

# Stage 2: Create the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

ENTRYPOINT ["dotnet", "YourApp.dll"]
