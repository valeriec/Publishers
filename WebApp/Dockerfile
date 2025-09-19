# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY WebApp/WebApp.csproj WebApp/
COPY Publisher.Application/Publisher.Application.csproj Publisher.Application/
COPY Publisher.Domain/Publisher.Domain.csproj Publisher.Domain/
COPY Publisher.Infrastructure/Publisher.Infrastructure.csproj Publisher.Infrastructure/
RUN dotnet restore "WebApp/WebApp.csproj"
COPY . .
RUN dotnet publish "WebApp/WebApp.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5000
ENTRYPOINT ["dotnet", "WebApp.dll"]
