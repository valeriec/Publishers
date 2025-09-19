# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY API1/API1.csproj API1/
COPY Publisher.Application/Publisher.Application.csproj Publisher.Application/
COPY Publisher.Domain/Publisher.Domain.csproj Publisher.Domain/
COPY Publisher.Infrastructure/Publisher.Infrastructure.csproj Publisher.Infrastructure/
RUN dotnet restore "API1/API1.csproj"
COPY . .
RUN dotnet publish "API1/API1.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5001
ENTRYPOINT ["dotnet", "API1.dll", "--server"]
