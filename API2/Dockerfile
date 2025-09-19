# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY API2/API2.csproj API2/
COPY Publisher.Application/Publisher.Application.csproj Publisher.Application/
COPY Publisher.Domain/Publisher.Domain.csproj Publisher.Domain/
COPY Publisher.Infrastructure/Publisher.Infrastructure.csproj Publisher.Infrastructure/
COPY . .
RUN dotnet restore "API2/API2.csproj"
RUN dotnet publish "API2/API2.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5002
ENTRYPOINT ["dotnet", "API2.dll", "--server"]
