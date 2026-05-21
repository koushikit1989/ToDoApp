FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/ToDoManagementSystem.API/ToDoManagementSystem.API.csproj", "src/ToDoManagementSystem.API/"]
COPY ["src/ToDoManagementSystem.Application/ToDoManagementSystem.Application.csproj", "src/ToDoManagementSystem.Application/"]
COPY ["src/ToDoManagementSystem.Domain/ToDoManagementSystem.Domain.csproj", "src/ToDoManagementSystem.Domain/"]
COPY ["src/ToDoManagementSystem.Infrastructure/ToDoManagementSystem.Infrastructure.csproj", "src/ToDoManagementSystem.Infrastructure/"]
COPY ["src/ToDoManagementSystem.Persistence/ToDoManagementSystem.Persistence.csproj", "src/ToDoManagementSystem.Persistence/"]
COPY ["src/ToDoManagementSystem.Shared/ToDoManagementSystem.Shared.csproj", "src/ToDoManagementSystem.Shared/"]
RUN dotnet restore "src/ToDoManagementSystem.API/ToDoManagementSystem.API.csproj"
COPY . .
WORKDIR "/src/src/ToDoManagementSystem.API"
RUN dotnet build "ToDoManagementSystem.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ToDoManagementSystem.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ToDoManagementSystem.API.dll"]
